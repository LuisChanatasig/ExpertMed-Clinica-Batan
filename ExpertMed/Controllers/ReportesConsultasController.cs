using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class ReportesConsultasController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportesConsultasController> _logger;
        private readonly DbExpertmedContext _dbContext;
        private readonly ReportService _reportService;
        public ReportesConsultasController(IHttpContextAccessor httpContextAccessor, ILogger<ReportesConsultasController> logger, DbExpertmedContext dbContext,ReportService reportService)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _dbContext = dbContext;
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> ResumenConsultas(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                // 1. Validación de Sesión (Seguridad Crítica)
                int? sessionUsuarioId = HttpContext.Session.GetInt32("UsuarioId");
                int? sessionPerfilId = HttpContext.Session.GetInt32("PerfilId");

                if (!sessionUsuarioId.HasValue || !sessionPerfilId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                int usuarioId = sessionUsuarioId.Value;
                int perfilId = sessionPerfilId.Value;

                // 2. Llamada al Servicio (Ya actualizado con asistencia)
                var resumen = await _reportService.GetResumenConsultasAsync(
                    fechaDesde, fechaHasta, perfilId, usuarioId);

                // 3. Retorno de Información
                // Si usas una vista Razor, asegúrate de que el modelo sea ResumenConsultasDto
                return View(resumen);
            }
            catch (Exception ex)
            {
                // 4. Gestión de Errores Logística
                _logger.LogError(ex, "Error en el Controller ResumenConsultas para el UsuarioId: {UsuarioId}", HttpContext.Session.GetInt32("UsuarioId"));
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el resumen de consultas.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
