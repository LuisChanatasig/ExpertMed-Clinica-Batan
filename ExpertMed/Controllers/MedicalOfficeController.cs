using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class MedicalOfficeController : Controller

    {

        private readonly UserService _userService;
        private readonly SelectsService _selectsService;
        private readonly ILogger<UsersController> _logger;
        private readonly MedicalOfficeService _clinicaService;

        public MedicalOfficeController(UserService userService, SelectsService selectsService, ILogger<UsersController> logger, MedicalOfficeService medicalOfficeService)
        {
            _userService = userService;
            _selectsService = selectsService;
            _logger = logger;
            _clinicaService = medicalOfficeService;
        }

        /// <summary>
        /// Ver pantalla para crear un nuevo consultorio.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task <IActionResult> CreateMedicalOffice()
        {
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

          
                // Administrador Global ve todas las clínicas
                ViewBag.Clinicas =  await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);
            

            return View();
        }

        /// <summary>
        /// Crea una nueva oficina médica.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ClinicaId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateMedicalOffice(MedicalOfficeCreateDto model, int? ClinicaId)
        {
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int establecimientoId;

           
                if (ClinicaId == null)
                {
                    TempData["ErrorMessage"] = "Debe seleccionar una clínica.";
                    ViewBag.Clinicas = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);
                    return View(model);
                }
                establecimientoId = ClinicaId.Value;



            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                // Coloca un breakpoint aquí, inspecciona la variable 'errors' en debug
                TempData["ErrorMessage"] = "Por favor, verifique los datos ingresados: " +
                    string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Errors)}"));

                ViewBag.Clinicas = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);

                return View(model);
            }


            try
            {
                var newMedicalOfficeId = await _clinicaService.CreateMedicalOfficeAsync(model, usuarioId, establecimientoId);
                TempData["SuccessMessage"] = "Consultorio creado exitosamente!";
                return RedirectToAction("ListaConsultorios");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear el consultorio: {ex.Message}";
                if (perfilId == 1)
                    ViewBag.Clinicas = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);

                return View(model);
            }
        }

        /// <summary>
        ///  Listado de consultorios
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListaConsultorios()
        {
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
         

            var consultorios = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId);

            return View(consultorios);
        }

        /// <summary>
        /// Traer por id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetConsultorioById(int id)
        {
     
            var consultorio = await _clinicaService.GetMedicalOfficeByIdAsync(id);
            if (consultorio == null)
                return NotFound();

            return Json(consultorio);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActualizarConsultorio(MedicalOfficeListDto model)
        {
            if (!ModelState.IsValid)
            {
                // Construir lista de errores
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Unir todos los errores en una sola cadena con saltos de línea
                TempData["ErrorMessage"] = "Errores de validación:\n" + string.Join("\n", errores);

                return RedirectToAction("ListaConsultorios");
            }

            try
            {
                await _clinicaService.UpdateMedicalOfficeAsync(model);
                TempData["SuccessMessage"] = "Consultorio actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el consultorio: " + ex.Message;
            }

            return RedirectToAction("ListaConsultorios");
        }

        /// <summary>
        /// Metodo para mostrar la vista de las asignaciones
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> OfficeAssignment()
        {
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            ViewBag.Consultorios = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId);
            ViewBag.Medicos = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);

            var asignaciones = await _clinicaService.GetAssignedOfficesAsync(perfilId, usuarioId);
            return View(asignaciones); // La vista ahora recibe un IEnumerable<AssignedOfficeDto>
        }


        /// <summary>
        /// Funcion que manda a guardar las asignaciones
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="consultorios"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AsignarConsultorios(int userId, List<int> consultorios)
        {
            int assignedBy = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            if (userId == 0 || consultorios == null || !consultorios.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar un médico y al menos un consultorio.";
                return RedirectToAction("OfficeAssignment");
            }

            var success = await _clinicaService.AssignConsultoriosAsync(userId, assignedBy, consultorios);

            if (success)
                TempData["SuccessMessage"] = "Asignación realizada correctamente.";
            else
                TempData["ErrorMessage"] = "Error al realizar la asignación.";

            return RedirectToAction("OfficeAssignment");
        }


        /// <summary>
        /// Metodo para obtener la asignacion por id
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsignacionById(int id)
        {
            try
            {
                // id ahora representa el UserId (médico)
                var result = await _clinicaService.GetAsignacionByUserIdAsync(id);
                if (result == null)
                    return NotFound();

                return Json(new
                {
                    assignmentId = result.AssignmentId, // puede omitirse si no se usa
                    assignmentStatus = result.AssignmentStatus,
                    userId = result.UserId,
                    medicalOfficeId = result.ConsultorioIds // lista tipo [1, 2, 3]
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        /// <summary>
        /// Metodo de actualizar asignacion
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="MedicalOfficeId"></param>
        /// <param name="AssignmentStatus"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ActualizarAsignacion(int UserId, List<int> MedicalOfficeId, bool AssignmentStatus)
        {
            try
            {
                await _clinicaService.ActualizarAsignacionAsync(UserId, MedicalOfficeId, AssignmentStatus);
                TempData["SuccessMessage"] = "Asignación actualizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar la asignación: {ex.Message}";
            }

            return RedirectToAction("OfficeAssignment");
        }


    }
}
