using Microsoft.AspNetCore.Mvc;
using ExpertMed.Services;
using ExpertMed.Models; // Asegúrate de que aquí estén tus DTOs

namespace ExpertMed.Controllers
{
    public class DocumentosFirmaController : Controller
    {
        private readonly DocumentosFirmaService _documentosService;
        private readonly ILogger<DocumentosFirmaController> _logger;

        public DocumentosFirmaController(DocumentosFirmaService documentosService, ILogger<DocumentosFirmaController> logger)
        {
            _documentosService = documentosService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Verificación de sesión (opcional, según tu sistema de seguridad)
                if (HttpContext.Session.GetInt32("UsuarioId") == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // 2. Obtención de datos agrupados desde el SP
                var listaAgrupada = await _documentosService.GetPacientesDocumentosAgrupadosAsync();

                // 3. Retornar a la vista
                return View(listaAgrupada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado agrupado de documentos.");
                TempData["ErrorMessage"] = "No se pudo cargar el listado de documentos.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Acción adicional para previsualizar el documento si es necesario
        [HttpGet]
        public IActionResult VerDocumento(string rutaFisica)
        {
            if (string.IsNullOrEmpty(rutaFisica) || !System.IO.File.Exists(rutaFisica))
            {
                return NotFound("El archivo no existe en el servidor.");
            }

            var contenido = System.IO.File.ReadAllBytes(rutaFisica);
            return File(contenido, "application/pdf");
        }
    }
}