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
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

                var resumen = await _reportService.GetResumenConsultasAsync(
                    fechaDesde, fechaHasta, perfilId, usuarioId);

                return View(resumen); // o Json(resumen) si es para frontend JS/AJAX
            }
            catch (Exception ex)
            {
                // Log y redirigir o mostrar mensaje de error
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el resumen de consultas.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
