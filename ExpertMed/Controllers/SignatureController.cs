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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid token, [FromForm] string signatureDataUrl)
        {
            // 1. Guardar firma
            var result = await _signatureService.SubmitAsync(token, signatureDataUrl,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers.UserAgent.ToString());

            if (!result.Ok) return BadRequest(result.Message);

            // 2. Generar y obtener NOMBRES de archivos
            List<string> nombresArchivos = await _signatureService.ProcessPatientDocumentsAsync(token, signatureDataUrl);

            // 3. Enviar a la vista (Importante: nombresArchivos es el Model)
            return View("SignOk", nombresArchivos);
        }

        // NUEVO MÉTODO: Permite descargar desde C:\ExpertMedStorage
        [HttpGet]
        public IActionResult Download(string fileName)
        {
            // Ruta base que vimos en tus logs
            string basePath = @"C:\ExpertMedStorage\DocumentosFirmados";
            string fullPath = Path.Combine(basePath, fileName);

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/pdf", fileName);
        }
        // Celular: Vista de éxito después de firmar
        [HttpGet]
        public IActionResult SignOk()
        {
            return View();
        }
    }
}
