using Microsoft.AspNetCore.Mvc;
using ExpertMed.Models;
using ExpertMed.Services;
using Org.BouncyCastle.Crypto.Utilities;
using Microsoft.EntityFrameworkCore;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace ExpertMed.Controllers
{

    public class BillingController : Controller
    {
        private readonly BillingServices _facturacion;
        private readonly DbExpertmedContext _context;
        private readonly ILogger<BillingController> _logger;
        public BillingController(BillingServices billingService, ILogger<BillingController> logger, DbExpertmedContext context)
        {
            _facturacion = billingService;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Facturacion(int? appointmentId)
        {
            if (!appointmentId.HasValue)
                return BadRequest("Falta el ID de la cita.");

            var cita = await _facturacion.GetAppointmentBillingDataAsync(appointmentId.Value);

            if (cita == null)
                return NotFound("No se encontró la cita.");

            ViewBag.AppointmentId = cita.AppointmentId;
            ViewBag.AppointmentPatientId = cita.PatientId;
            ViewBag.PatientFullName = cita.PatientFullName;

            ViewBag.HasInsurance = cita.InsuranceCompanyId != null;
            ViewBag.InsuranceCompanyId = cita.InsuranceCompanyId ?? 0;
            ViewBag.InsuranceCompanyName = cita.InsuranceCompanyName ?? "Sin compañía";
            ViewBag.AuthorizationCode = cita.InsuranceAuthCode ?? "";

            return View(cita);
        }



        [HttpPost("Vista")]
        [RequestSizeLimit(52428800)] // 50MB
        public async Task<IActionResult> Billing([FromForm] Facturacions viewModel, IFormFile? comprobantePagoFile)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Errores en la validación del modelo: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                TempData["ErrorMessage"] = "Verifique los datos ingresados.";
                return View("Facturacion", viewModel); // Ojo: Vista debe llamarse igual que la Razor asociada
            }

            try
            {
                // Carga el comprobante (o dummy si no se subió)
                if (comprobantePagoFile != null && comprobantePagoFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await comprobantePagoFile.CopyToAsync(memoryStream);
                    viewModel.ComprobantePagoFacturacion = memoryStream.ToArray();
                }
                else
                {
                    viewModel.ComprobantePagoFacturacion = new byte[] { 0x01, 0x02, 0x03 }; // Dummy opcional
                }

                // Ejecutar servicio para crear y enviar la factura
                string response = await _facturacion.CreateAndSendInvoiceAsync(
                    viewModel.CitaId ?? 0,
                    DateTime.UtcNow,
                    viewModel.TotalFactura,
                    viewModel.MetodoPago ?? string.Empty,
                    viewModel.ComprobantePagoFacturacion,
                    viewModel.BillingDetailsNames,
                    viewModel.BillingDetailsCiNumber,
                    viewModel.BillingDetailsDocumentType,
                    viewModel.BillingDetailsAddress,
                    viewModel.BillingDetailsPhone,
                    viewModel.BillingDetailsEmail,
                    viewModel.Items
                );

                _logger.LogInformation("Factura generada con éxito para la cita ID: {CitaId}", viewModel.CitaId);
                TempData["SuccessMessage"] = "Factura generada y enviada correctamente.";

                return RedirectToAction("AppointmentList", "Appointment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al facturar cita ID: {CitaId}", viewModel.CitaId);
                TempData["ErrorMessage"] = $"Error al generar la factura: {ex.Message}";
                return View("Facturacion", viewModel); // Mismo nombre que en error de validación
            }
        }


        [HttpGet("Facturas")]
        public async Task<IActionResult> FacturasEmitidas()
        {
            var facturas = await _facturacion.ObtenerFacturasEmitidasAsync();
            return View(facturas);
        }

        [HttpGet]
        public async Task<IActionResult> NotaVenta(int facturaId)
        {
            var datosFactura = await _facturacion.GetFacturaConDetalleAsync(facturaId);
            if (datosFactura == null)
                return NotFound();

            using (var stream = new MemoryStream())
            {
                var doc = new Document(PageSize.A4, 50, 50, 40, 40);
                var writer = PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                // Encabezado centrado
                var empresa = new Paragraph("EXPERTMED S.A.", titleFont);
                empresa.Alignment = Element.ALIGN_CENTER;
                doc.Add(empresa);

                var ruc = new Paragraph("RUC: 1234567890001", normalFont);
                ruc.Alignment = Element.ALIGN_CENTER;
                doc.Add(ruc);

                var direccion = new Paragraph("Dirección: Av. Salud y Vida 123, Quito", normalFont);
                direccion.Alignment = Element.ALIGN_CENTER;
                doc.Add(direccion);

                var telefono = new Paragraph("Teléfono: 0999999999", normalFont);
                telefono.Alignment = Element.ALIGN_CENTER;
                doc.Add(telefono);

                doc.Add(new Paragraph(" "));
                var nota = new Paragraph("NOTA DE VENTA", subtitleFont);
                nota.Alignment = Element.ALIGN_CENTER;
                doc.Add(nota);
                doc.Add(new Paragraph(" "));

                // Línea decorativa (simulación)
                doc.Add(new Paragraph("--------------------------------------------------------------", normalFont) { Alignment = Element.ALIGN_CENTER });

                // Datos del paciente
                var infoTable = new PdfPTable(2) { WidthPercentage = 100 };
                infoTable.SetWidths(new float[] { 25f, 75f });

                infoTable.AddCell(new PdfPCell(new Phrase("Fecha:", boldFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Fecha.ToString("dd/MM/yyyy"), normalFont)) { Border = 0 });

                infoTable.AddCell(new PdfPCell(new Phrase("Paciente:", boldFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Paciente, normalFont)) { Border = 0 });

                infoTable.AddCell(new PdfPCell(new Phrase("Método de Pago:", boldFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.MetodoPago, normalFont)) { Border = 0 });

                if (!string.IsNullOrWhiteSpace(datosFactura.Aseguradora))
                {
                    infoTable.AddCell(new PdfPCell(new Phrase("Aseguradora:", boldFont)) { Border = 0 });
                    infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Aseguradora, normalFont)) { Border = 0 });
                }

                doc.Add(infoTable);
                doc.Add(new Paragraph(" "));

                // Tabla de ítems
                var itemTable = new PdfPTable(4) { WidthPercentage = 100 };
                itemTable.SetWidths(new float[] { 50f, 15f, 15f, 20f });

                string[] headers = { "Descripción", "Cantidad", "P. Unitario", "Subtotal" };
                foreach (var h in headers)
                {
                    var cell = new PdfPCell(new Phrase(h, boldFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    itemTable.AddCell(cell);
                }

                foreach (var item in datosFactura.Items)
                {
                    itemTable.AddCell(new Phrase(item.Descripcion, normalFont));
                    itemTable.AddCell(new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    itemTable.AddCell(new PdfPCell(new Phrase($"${item.PrecioUnitario:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                    decimal subtotal = item.PrecioUnitario * item.Cantidad;
                    itemTable.AddCell(new PdfPCell(new Phrase($"${subtotal:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                doc.Add(itemTable);
                doc.Add(new Paragraph(" "));

                // Totales
                var totalTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
                totalTable.SetWidths(new float[] { 60f, 40f });

                totalTable.AddCell(new PdfPCell(new Phrase("Total Aseguradora:", boldFont)) { Border = 0 });
                totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.TotalAseguradora:F2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                totalTable.AddCell(new PdfPCell(new Phrase("Total Copago:", boldFont)) { Border = 0 });
                totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.TotalCopago:F2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                totalTable.AddCell(new PdfPCell(new Phrase("TOTAL:", boldFont)) { Border = 0 });
                totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.Subtotal:F2}", boldFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                doc.Add(totalTable);

                doc.Close();
                var pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", $"NotaVenta_{facturaId}.pdf");
            }
        }




    }
}
