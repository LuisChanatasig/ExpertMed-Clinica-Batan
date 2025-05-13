using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class FisioterapiaController : Controller
    {
        private readonly PatientService _patientService;
        private readonly SelectsService _selectsService;
        private readonly UserService _usersService;
        private readonly TherapyService _therapyService;
        private readonly ILogger<FisioterapiaController> _logger;

        public FisioterapiaController(PatientService patientService, SelectsService selectsService,  ILogger<FisioterapiaController> logger, UserService usersService, TherapyService therapyService)
        {
            _patientService = patientService;
            _selectsService = selectsService;
            _logger = logger;
            _usersService = usersService;
            _therapyService = therapyService;
        }

        [HttpGet]
        public async Task<IActionResult> TherapySchedule()
        {
            try
            {
                int perfilId = Convert.ToInt32(HttpContext.Session.GetInt32("PerfilId"));
                int usuarioId = Convert.ToInt32(HttpContext.Session.GetInt32("UsuarioId"));

                var pacientes = await _patientService.GetAllPatientsAsync(perfilId, usuarioId);

                // Aquí usamos tu método nuevo que ejecuta sp_ListAllUser
                var todosLosUsuarios = _usersService.GetAllUsers(usuarioId, perfilId);

                var terapeutas = todosLosUsuarios
                    .Where(u => u.ProfileId == 7) // Solo los fisioterapeutas
                    .Select(u => new SelectDTO
                    {
                        Value = u.UserId.ToString(),
                        Text = $"{u.Names} {u.Surnames}"
                    })
                    .ToList();

                ViewBag.Pacientes = pacientes;
                ViewBag.Terapeutas = terapeutas;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando la agenda de terapias.");
                TempData["ErrorMessage"] = "No se pudo cargar la agenda de terapias.";
                return RedirectToAction("TherapySchedule", "Fisioterapia");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitTherapySessions([FromBody] TherapySubmissionDto model)
        {
            try
            {
                int creationUser = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                if (creationUser == 0)
                    return Unauthorized(new { success = false, message = "Sesión inválida." });

                await _therapyService.GuardarTerapiasAsync(model, creationUser);

                return Ok(new { success = true, message = "Terapias guardadas correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en SubmitTherapySessions.");
                return StatusCode(500, new { success = false, message = "Error al guardar terapias." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TherapyHistory()
        {
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var data = await _therapyService.ObtenerHistorialTerapiasAsync(perfilId, usuarioId);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvance(int sessionId, int avance, string comentarios)
        {
            try
            {
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                if (usuarioId == 0)
                    return Unauthorized();

                await _therapyService.ActualizarAvanceSesionAsync(sessionId, avance, comentarios, usuarioId);

                TempData["SuccessMessage"] = "Avance actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar avance de sesión.");
                TempData["ErrorMessage"] = "No se pudo guardar el avance. Verifica los datos.";
            }

            return RedirectToAction("TherapyHistory");
        }


        public IActionResult TherapyAlerts()
        {
            return View();
        }
    }
}
