using Microsoft.AspNetCore.Mvc;
using ExpertMed.Models;
using ExpertMed.Services;
using Org.BouncyCastle.Crypto.Utilities;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Facturacion(int? appointmentId)
        {
            if (!appointmentId.HasValue)
                return BadRequest("Appointment ID is missing.");

            var cita = _context.Appointments
                .Include(a => a.AppointmentPatient)
                .FirstOrDefault(a => a.AppointmentId == appointmentId.Value);

            if (cita == null)
                return NotFound("Cita no encontrada.");

            var paciente = cita.AppointmentPatient;

            ViewBag.AppointmentId = cita.AppointmentId;
            ViewBag.AppointmentPatientId = paciente?.PatientId;

            ViewBag.HasInsurance = paciente?.PatientHealtInsurance != null;
            ViewBag.InsuranceCompanyId = paciente?.PatientHealtInsurance ?? 0;
            ViewBag.InsuranceCompanyName = paciente?.PatientCompany ?? "";
            ViewBag.AuthorizationCode = paciente?.PatientInsuranceAuthorizationCode ?? "";

            return View();
        }



        [HttpPost("Vista")]
        [RequestSizeLimit(52428800)] // 50MB limit
        public async Task<IActionResult> Billing([FromForm] Facturacions viewModel, IFormFile? comprobantePagoFile)
        {
             if (!ModelState.IsValid)
            {
                // Registrar errores del modelo
                _logger.LogWarning("Errores en la validación del modelo: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                return View(viewModel);
            }

            try
            {
                // Valor binario por defecto (5 bytes arbitrarios)
                byte[] comprobantePagoDummy = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

                // Procesar el archivo si se ha adjuntado
                if (comprobantePagoFile?.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await comprobantePagoFile.CopyToAsync(memoryStream);
                    viewModel.ComprobantePagoFacturacion = memoryStream.ToArray();
                }
                else
                {
                    // Si no hay archivo, usar el valor binario por defecto
                    viewModel.ComprobantePagoFacturacion = comprobantePagoDummy;
                }


                // Crear objeto de facturación
                var facturacion = new Facturacions
                {
                    CitaId = viewModel.CitaId,
                    FechaFacturacion = DateTime.UtcNow, // UTC recomendado para precisión en servidores
                    TotalFactura = viewModel.TotalFactura,
                    MetodoPago = viewModel.MetodoPago,
                    ComprobantePagoFacturacion = viewModel.ComprobantePagoFacturacion
                };

                // Recoger los datos adicionales de facturación desde el formulario
                string billingDetailsNames = viewModel.BillingDetailsNames;
                string billingDetailsCiNumber = viewModel.BillingDetailsCiNumber;
                string billingDetailsDocumentType = viewModel.BillingDetailsDocumentType;
                string billingDetailsAddress = viewModel.BillingDetailsAddress;
                string billingDetailsPhone = viewModel.BillingDetailsPhone;
                string billingDetailsEmail = viewModel.BillingDetailsEmail;

                // Llamar al servicio para crear y enviar la factura
                string response = await _facturacion.CreateAndSendInvoiceAsync(
                    facturacion.CitaId ?? 0,
                    facturacion.FechaFacturacion,
                    facturacion.TotalFactura,
                    facturacion.MetodoPago,
                    facturacion.ComprobantePagoFacturacion,
                    billingDetailsNames,
                    billingDetailsCiNumber,
                    billingDetailsDocumentType,
                    billingDetailsAddress,
                    billingDetailsPhone,
                    billingDetailsEmail
                );

                _logger.LogInformation("Factura generada con éxito para la cita ID: {CitaId}", facturacion.CitaId);

                TempData["SuccessMessage"] = "Su factura ha sido generada y enviada al correo proporcionado.";

                return RedirectToAction("AppointmentList", "Appointment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al procesar la facturación para la cita ID: {CitaId}", viewModel.CitaId);
                TempData["ErrorMessage"] = "Ocurrió un error al generar la factura. Intente nuevamente."+ex;

                return StatusCode(500, "Error interno del servidor");
            }
        }

    }
}
