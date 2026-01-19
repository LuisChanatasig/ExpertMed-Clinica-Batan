using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class SignatureController : Controller
    {
        private readonly SignatureService _signatureService;
        private readonly ILogger<SignatureService> _logger;

        public SignatureController(SignatureService signatureService, ILogger<SignatureService> logger)
        {
            _signatureService = signatureService;
            _logger = logger;
        }

        // Laptop: crea request y devuelve URL para QR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest([FromForm] string patientCode)
        {
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            var req = await _signatureService.CreateRequestAsync(patientCode, userId, expiresMinutes: 10);

            var signUrl = Url.Action("Sign", "Signature", new { token = req.Token }, HttpContext.Request.Scheme);

            return Json(new
            {
                ok = true,
                token = req.Token,
                expiresAt = req.ExpiresAtUtc,
                signUrl
            });
        }

        // Laptop: polling
        [HttpGet]
        public async Task<IActionResult> Status(Guid token)
        {
            var st = await _signatureService.GetStatusAsync(token);
            if (st == null) return NotFound(new { ok = false, message = "Token no existe." });

            return Json(new
            {
                ok = true,
                status = st.Status,
                expiresAt = st.ExpiresAtUtc,
                signedAt = st.SignedAtLocal,
                signatureDataUrl = st.Status == 1 ? st.SignatureDataUrl : null
            });
        }

        // Celular: vista de firma
        [HttpGet]
        public async Task<IActionResult> Sign(Guid token)
        {
            var vm = await _signatureService.GetForSignAsync(token);
            if (vm == null) return View("SignInvalid");
            return View("Sign", vm); // Views/Signature/Sign.cshtml     
        }


        // Celular: enviar firma y generar documentos
        // Método Submit corregido con logging detallado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            Guid token,
            [FromForm] string signatureDataUrl,
            [FromForm] string consentVersion)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers.UserAgent.ToString();

            _logger.LogInformation("Iniciando proceso de firma. Token: {Token}", token);

            // 1. Validar y guardar la firma
            var result = await _signatureService.SubmitAsync(token, signatureDataUrl, ip, ua);
            if (!result.Ok)
            {
                _logger.LogError("Error al guardar firma: {Message}", result.Message);
                return BadRequest(new { ok = false, message = result.Message });
            }

            // 2. Obtener información del paciente desde la petición
            int? patientId = await _signatureService.GetPatientIdFromTokenAsync(token);

            if (!patientId.HasValue)
            {
                _logger.LogError("No se pudo obtener el paciente asociado al token {Token}", token);
                return BadRequest(new { ok = false, message = "Token no asociado a un paciente válido" });
            }

            try
            {
                _logger.LogInformation("Procesando documentos para paciente {PatientId}", patientId.Value);

                // 3. Generar y guardar documentos firmados
                bool procesado = await _signatureService.ProcessPatientDocumentsAsync(
                    token,
                    patientId.Value,
                    signatureDataUrl,
                    consentVersion
                );

                if (!procesado)
                {
                    _logger.LogWarning("Los documentos no se procesaron correctamente");
                    // Aún así mostramos éxito porque la firma SÍ se guardó
                }
                else
                {
                    _logger.LogInformation("Documentos procesados exitosamente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico procesando documentos físicos para paciente {PatientId}", patientId);

                // Retornar error en lugar de ocultarlo
                return StatusCode(500, new
                {
                    ok = false,
                    message = "Error al procesar documentos. La firma fue guardada pero los archivos no se generaron.",
                    error = ex.Message
                });
            }

            return View("SignOk");
        }
        // Método para que la Laptop o el Celular puedan DESCARGAR el archivo desde la ruta externa
        [HttpGet]
        public async Task<IActionResult> DownloadDoc(int documentoId)
        {
            // Aquí deberías implementar un método en tu servicio que busque la ruta física
            // en la tabla PacienteDocumentos usando el ID.

            // Ejemplo rápido:
            // var doc = await _signatureService.GetDocumentMetadata(documentoId);
            // if (doc == null) return NotFound();
            // byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(doc.RutaFisica);
            // return File(fileBytes, "application/pdf", doc.NombreArchivo);

            return Ok();
        }
        // Celular: Vista de éxito después de firmar
        [HttpGet]
        public IActionResult SignOk()
        {
            return View();
        }
    }
}
