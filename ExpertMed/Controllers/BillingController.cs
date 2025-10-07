using ExpertMed.Models;
using ExpertMed.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{

    public class BillingController : Controller
    {
        private readonly BillingServices _facturacion;
        private readonly DbExpertmedContext _context;
        private readonly ILogger<BillingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BillingController(BillingServices billingService, ILogger<BillingController> logger, DbExpertmedContext context, IWebHostEnvironment webHostEnvironment)
        {
            _facturacion = billingService;
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="comprobantePagoFile"></param>
        /// <returns></returns>

        [HttpPost]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> Billing(
    [FromForm] Facturacions viewModel,
    IFormFile comprobantePagoFile = null,
    List<IFormFile> PaymentProofs = null)
        {
            if (!ModelState.IsValid)
            {
                // Log detallado de errores
                var errores = ModelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .Select(ms => new
                    {
                        Campo = ms.Key,
                        Errores = ms.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                foreach (var error in errores)
                {
                    _logger.LogWarning("Campo: {Campo}, Errores: {Errores}",
                        error.Campo, string.Join("; ", error.Errores));
                }

                TempData["ErrorMessage"] = "Verifique los datos ingresados.";
                return View("Facturacion", viewModel);
            }

            try
            {
                // Array de bytes dummy para cuando no hay comprobante
                byte[] dummyBytes = new byte[] { 0x00 };

                // Determinar si usar sistema antiguo o nuevo
                bool usarMultiplesPagos = viewModel.PaymentMethods != null && viewModel.PaymentMethods.Any();

                if (usarMultiplesPagos)
                {
                    // Validar suma de pagos
                    var totalPagos = viewModel.PaymentMethods.Sum(p => p.PaymentAmount);
                    if (Math.Abs(totalPagos - viewModel.TotalFactura) > 0.01m)
                    {
                        TempData["ErrorMessage"] = $"La suma de los pagos ({totalPagos:F2}) no coincide con el total ({viewModel.TotalFactura:F2}).";
                        return View("Facturacion", viewModel);
                    }

                    // Procesar comprobantes múltiples
                    if (PaymentProofs != null && PaymentProofs.Count > 0)
                    {
                        for (int i = 0; i < PaymentProofs.Count && i < viewModel.PaymentMethods.Count; i++)
                        {
                            if (PaymentProofs[i] != null && PaymentProofs[i].Length > 0)
                            {
                                using var ms = new MemoryStream();
                                await PaymentProofs[i].CopyToAsync(ms);
                                viewModel.PaymentMethods[i].PaymentProof = ms.ToArray();
                            }
                            else
                            {
                                // Asignar dummy bytes si no hay archivo
                                viewModel.PaymentMethods[i].PaymentProof = dummyBytes;
                            }
                        }
                    }
                    else
                    {
                        // Si no se enviaron archivos, asignar dummy bytes a todos
                        foreach (var payment in viewModel.PaymentMethods)
                        {
                            payment.PaymentProof = dummyBytes;
                        }
                    }
                }
                else
                {
                    // Sistema antiguo: un solo comprobante
                    if (comprobantePagoFile != null && comprobantePagoFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await comprobantePagoFile.CopyToAsync(ms);
                        viewModel.ComprobantePagoFacturacion = ms.ToArray();
                    }
                    else
                    {
                        // Asignar dummy bytes
                        viewModel.ComprobantePagoFacturacion = dummyBytes;
                    }
                }

                // Llamar al servicio
                string response = await _facturacion.CreateAndSendInvoiceAsync(
                    viewModel.CitaId ?? 0,
                    DateTime.Now,
                    viewModel.TotalFactura,
                    usarMultiplesPagos ? null : viewModel.MetodoPago,
                    usarMultiplesPagos ? null : viewModel.ComprobantePagoFacturacion,
                    viewModel.BillingDetailsNames,
                    viewModel.BillingDetailsCiNumber,
                    viewModel.BillingDetailsDocumentType,
                    viewModel.BillingDetailsAddress,
                    viewModel.BillingDetailsPhone,
                    viewModel.BillingDetailsEmail,
                    viewModel.InsuranceCompanyId,
                    viewModel.Items,
                    usarMultiplesPagos ? viewModel.PaymentMethods : null
                );

                _logger.LogInformation("Factura generada con éxito para la cita ID: {CitaId}", viewModel.CitaId);
                TempData["SuccessMessage"] = "Factura generada y enviada correctamente.";
                return RedirectToAction("AppointmentList", "Appointment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al facturar cita ID: {CitaId}", viewModel.CitaId);
                TempData["ErrorMessage"] = $"Error al generar la factura: {ex.Message}";
                return View("Facturacion", viewModel);
            }
        }


        [HttpGet("Facturas")]
        public async Task<IActionResult> FacturasEmitidas(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var facturas = await _facturacion.ObtenerFacturasEmitidasAsync(fechaDesde, fechaHasta);
                return View(facturas);
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error en FacturasEmitidas: {ex.Message}");
                return View(new List<FacturaEmitidaDTO>());
            }
        }
        [HttpPost("Facturas/Filtrar")]
        public async Task<JsonResult> FiltrarFacturas([FromBody] FiltroFechasRequest request)
        {
            try
            {
                var facturas = await _facturacion.ObtenerFacturasEmitidasAsync(request.FechaDesde, request.FechaHasta);

                var facturasSerialized = facturas.Select(f => new
                {
                    facturaId = f.FacturaId,
                    secuencial = f.Secuencial,
                    secuencialFormateado = f.SecuencialFormateado, // ⬅️ AGREGAR ESTA LÍNEA
                    fecha = f.Fecha.ToString("yyyy-MM-ddTHH:mm:ss"),
                    paciente = f.Paciente ?? "(Sin nombre)",
                    medico = f.Medico ?? "(Sin médico)",
                    subtotal = f.Subtotal,
                    totalAseguradora = f.TotalAseguradora,
                    totalCopago = f.TotalCopago,
                    metodoPago = f.MetodoPago ?? "-",
                    aseguradora = f.Aseguradora ?? "Particular",
                    totalItems = f.TotalItems,
                    origen = f.Origen ?? "LOCAL"
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = facturasSerialized,
                    count = facturasSerialized.Count,
                    fechaDesde = request.FechaDesde?.ToString("dd/MM/yyyy"),
                    fechaHasta = request.FechaHasta?.ToString("dd/MM/yyyy")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en FiltrarFacturas: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Error al filtrar facturas: " + ex.Message,
                    data = new List<object>()
                });
            }
        }

        // Asegúrate de que esta clase esté definida
        public class FiltroFechasRequest
        {
            public DateTime? FechaDesde { get; set; }
            public DateTime? FechaHasta { get; set; }
        }

        //[HttpGet]
        //public async Task<IActionResult> NotaVenta(int facturaId)
        //{
        //    var datosFactura = await _facturacion.GetFacturaConDetalleAsync(facturaId);
        //    if (datosFactura == null)
        //        return NotFound();

        //    using (var stream = new MemoryStream())
        //    {
        //        var doc = new Document(PageSize.A4, 50, 50, 40, 40);
        //        var writer = PdfWriter.GetInstance(doc, stream);
        //        doc.Open();

        //        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        //        var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        //        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
        //        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

        //        // Encabezado centrado
        //        var empresa = new Paragraph("CLÍNICA MÉDICA EL BATÁN", titleFont);
        //        empresa.Alignment = Element.ALIGN_CENTER;
        //        doc.Add(empresa);

        //        var ruc = new Paragraph("RUC: 1790012345001", normalFont);
        //        ruc.Alignment = Element.ALIGN_CENTER;
        //        doc.Add(ruc);

        //        var direccion = new Paragraph("Dirección: Av. 6 de Diciembre y Bosmediano, Quito", normalFont);
        //        direccion.Alignment = Element.ALIGN_CENTER;
        //        doc.Add(direccion);

        //        var telefono = new Paragraph("Teléfono: (02) 234-5678 / 099-876-5432", normalFont);
        //        telefono.Alignment = Element.ALIGN_CENTER;
        //        doc.Add(telefono);

        //        doc.Add(new Paragraph(" "));
        //        var nota = new Paragraph("NOTA DE VENTA", subtitleFont);
        //        nota.Alignment = Element.ALIGN_CENTER;
        //        doc.Add(nota);
        //        doc.Add(new Paragraph(" "));

        //        // Línea decorativa (simulación)
        //        doc.Add(new Paragraph("--------------------------------------------------------------", normalFont) { Alignment = Element.ALIGN_CENTER });

        //        // Datos del paciente
        //        var infoTable = new PdfPTable(2) { WidthPercentage = 100 };
        //        infoTable.SetWidths(new float[] { 25f, 75f });

        //        infoTable.AddCell(new PdfPCell(new Phrase("Fecha:", boldFont)) { Border = 0 });
        //        infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Fecha.ToString("dd/MM/yyyy"), normalFont)) { Border = 0 });

        //        infoTable.AddCell(new PdfPCell(new Phrase("Paciente:", boldFont)) { Border = 0 });
        //        infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Paciente, normalFont)) { Border = 0 });

        //        infoTable.AddCell(new PdfPCell(new Phrase("Método de Pago:", boldFont)) { Border = 0 });
        //        infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.MetodoPago, normalFont)) { Border = 0 });

        //        if (!string.IsNullOrWhiteSpace(datosFactura.Aseguradora))
        //        {
        //            infoTable.AddCell(new PdfPCell(new Phrase("Aseguradora:", boldFont)) { Border = 0 });
        //            infoTable.AddCell(new PdfPCell(new Phrase(datosFactura.Aseguradora, normalFont)) { Border = 0 });
        //        }

        //        doc.Add(infoTable);
        //        doc.Add(new Paragraph(" "));

        //        // Tabla de ítems
        //        var itemTable = new PdfPTable(4) { WidthPercentage = 100 };
        //        itemTable.SetWidths(new float[] { 50f, 15f, 15f, 20f });

        //        string[] headers = { "Descripción", "Cantidad", "P. Unitario", "Subtotal" };
        //        foreach (var h in headers)
        //        {
        //            var cell = new PdfPCell(new Phrase(h, boldFont))
        //            {
        //                BackgroundColor = BaseColor.LIGHT_GRAY,
        //                HorizontalAlignment = Element.ALIGN_CENTER
        //            };
        //            itemTable.AddCell(cell);
        //        }

        //        foreach (var item in datosFactura.Items)
        //        {
        //            itemTable.AddCell(new Phrase(item.Descripcion, normalFont));
        //            itemTable.AddCell(new PdfPCell(new Phrase(item.Cantidad.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
        //            itemTable.AddCell(new PdfPCell(new Phrase($"${item.PrecioUnitario:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

        //            decimal subtotal = item.PrecioUnitario * item.Cantidad;
        //            itemTable.AddCell(new PdfPCell(new Phrase($"${subtotal:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
        //        }

        //        doc.Add(itemTable);
        //        doc.Add(new Paragraph(" "));

        //        // Totales
        //        var totalTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
        //        totalTable.SetWidths(new float[] { 60f, 40f });

        //        totalTable.AddCell(new PdfPCell(new Phrase("Total Aseguradora:", boldFont)) { Border = 0 });
        //        totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.TotalAseguradora:F2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        //        totalTable.AddCell(new PdfPCell(new Phrase("Total Copago:", boldFont)) { Border = 0 });
        //        totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.TotalCopago:F2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        //        totalTable.AddCell(new PdfPCell(new Phrase("TOTAL:", boldFont)) { Border = 0 });
        //        totalTable.AddCell(new PdfPCell(new Phrase($"${datosFactura.Subtotal:F2}", boldFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

        //        doc.Add(totalTable);

        //        doc.Close();
        //        var pdfBytes = stream.ToArray();
        //        return File(pdfBytes, "application/pdf", $"NotaVenta_{facturaId}.pdf");
        //    }
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="facturaId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> NotaVentaConTablaDinamica(int facturaId)
        {
            var datosFactura = await _facturacion.GetFacturaConDetalleAsync(facturaId);
            if (datosFactura == null)
                return NotFound();

            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "Nota_de_venta.pdf");

            if (!System.IO.File.Exists(templatePath))
                return StatusCode(500, "PDF template file not found.");

            // ✅ NO usar `using` directamente sobre el PdfReader cuando se combina con PdfStamper
            PdfReader reader = null;
            MemoryStream stream = new MemoryStream();

            try
            {
                reader = new PdfReader(templatePath);
                PdfStamper stamper = new PdfStamper(reader, stream);
                AcroFields form = stamper.AcroFields;
                PdfContentByte canvas = stamper.GetOverContent(1);

                // Fuentes
                string arialPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "Fuentes", "ARIAL.TTF");
                BaseFont arialBaseFont = BaseFont.CreateFont(arialPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var normalFont = new Font(arialBaseFont, 9, Font.NORMAL, BaseColor.BLACK);
                var boldFont = new Font(arialBaseFont, 9, Font.BOLD, BaseColor.BLACK);

                // Campos estáticos
                form.SetField("txt_fecha_es_:signer:date", datosFactura.Fecha.ToString("dd/MM/yyyy"));
                form.SetField("txt_nombre", datosFactura.Paciente);
                form.SetField("txt_metodo_pago", datosFactura.MetodoPago.ToUpper());
                form.SetField("txt_direccion", datosFactura.PacienteDireccion ?? "");
                form.SetField("txt_telefono_paciente", (datosFactura.PacienteNumeroCelular ?? "") + " / " +(datosFactura.PacienteNumeroFijo ?? ""));
                form.SetField("pacienteEmail_es_:signer:email", datosFactura.PacienteEmail ?? "");
                form.SetField("aseguradora", datosFactura.Aseguradora ?? "");
                form.SetField("txt_numero_factura", datosFactura.NumeroFactura ?? "");

                // Crear tabla
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

                // Posicionar la tabla en la ubicación del campo
                var fieldPositions = form.GetFieldPositions("txt_items_facturacion");

                if (fieldPositions != null && fieldPositions.Count > 0)
                {
                    var position = fieldPositions[0].position;
                    ColumnText ct = new ColumnText(canvas);
                    ct.SetSimpleColumn(position.Left, position.Bottom, position.Right, position.Top);
                    ct.AddElement(itemTable);
                    ct.Go();

                    form.RemoveField("txt_items_facturacion");
                }

                // Totales
                form.SetField("totalAseguradora", $"{datosFactura.TotalAseguradora:F2}");
                form.SetField("totalCopago", $"{datosFactura.TotalCopago:F2}");
                form.SetField("totalFinal", $"{datosFactura.Subtotal:F2}");

                stamper.FormFlattening = true;
                stamper.Close();
                reader.Close();

                var pdfBytes = stream.ToArray();
                return File(pdfBytes, "application/pdf", $"NotaVenta_Form_DynamicItems_{facturaId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para factura {facturaId}", facturaId);
                return StatusCode(500, "Error generando el PDF.");
            }
            finally
            {
                reader?.Close();
                stream?.Dispose();
            }
        }


    }
}
