using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ExpertMed.Controllers
{
    public class PatientController : Controller
    {
        private readonly UserService _usersService;
        private readonly ILogger<PatientController> _logger;
        private readonly SelectsService _selectService;
        private readonly PatientService _patientService;

        public PatientController(UserService usersService, ILogger<PatientController> logger, SelectsService selectService, PatientService patientService)
        {
            _usersService = usersService;
            _logger = logger;
            _selectService = selectService;
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            try
            {
                var patientDetails = await _patientService.GetPatientDetailsAsync(patientId);
                return patientDetails != null ? Ok(patientDetails) : NotFound("Paciente no encontrado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los detalles del paciente.");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// MANDA LA LISTA DE PACIENTES
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> PatientList()
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var perfilId = HttpContext.Session.GetInt32("PerfilId");

                if (!usuarioId.HasValue || !perfilId.HasValue)
                {
                    _logger.LogWarning("La sesión no contiene un UsuarioId o PerfilId válido.");
                    TempData["ErrorMessage"] = "Debe iniciar sesión correctamente para acceder a los pacientes.";
                    return RedirectToAction("Login", "Account"); // O redirigir a algún lugar seguro
                }

                var patients = await _patientService.GetAllPatientsAsync(perfilId.Value, usuarioId.Value);
                return View(patients);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Error SQL al obtener la lista de pacientes.");
                TempData["ErrorMessage"] = "Hubo un problema de conexión a la base de datos."+sqlEx;
                return View("PatientList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener la lista de pacientes.");
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al cargar los pacientes."+ex;
                return View("PatientList");
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChangePatientStatus(int patientId, int status)
        {
            var result = await _patientService.DesactiveOrActivePatientAsync(patientId, status);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.message;
            return RedirectToAction("PatientList");
        }

        [HttpGet]
        public async Task<IActionResult> NewPatient() => await LoadPatientFormAsync();

        [HttpGet]
        public async Task<IActionResult> RegistroPaciente(int? est = null)
        {
            return await LoadPatientFormAsync(null, est);
        }

        [HttpPost]
        public async Task<IActionResult> NewPatient(Patient patient, int? doctorUserId = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new {
                        Field = x.Key,
                        Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    });

                return BadRequest(new { success = 0, message = "Datos inválidos.", errores = errors });
            }

            try
            {
                await _patientService.CreatePatientAsync(patient, doctorUserId);
                TempData["SuccessMessage"] = "Paciente creado exitosamente.";
                return RedirectToAction("PatientList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return await RegistroPaciente();
            }
        }

        /// <summary>
        /// Autoregistro
        /// </summary>
        /// <param name="patient"></param>
        /// <param name="doctorUserId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> NewPatientA(Patient patient, int? doctorUserId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = 0, message = "Datos inválidos." });

            try
            {
                // Asignamos el usuario que crea y modifica
                patient.PatientCreationuser = doctorUserId;
                patient.PatientModificationuser = doctorUserId;

                int patientId;

                // Verificamos si el paciente ya existe utilizando el número de documento
                var existingPatient = await _patientService.GetPatientDataByDocumentNumberAsync(patient.PatientDocumentnumber);
                if (existingPatient != null)
                {
                    patientId = existingPatient.PatientId;
                    TempData["SuccessMessage"] = "El paciente ya existe, se registrará una cita de emergencia.";
                }
                else
                {
                    patientId = await _patientService.CreatePatientAsync(patient, doctorUserId);
                    TempData["SuccessMessage"] = "Paciente creado exitosamente. Se registrará una cita de emergencia.";
                }

                // Indicamos que debe dispararse automáticamente una cita de emergencia
                TempData["DoctorUserId"] = doctorUserId;
                TempData["PatientId"] = patientId;
                TempData["ForceEmergency"] = true;

                return RedirectToAction("RegistroPaciente");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return await RegistroPaciente();
            }
        }




        [HttpGet]
        public async Task<IActionResult> UpdatePatient(int id)
        {
            var patient = await _patientService.GetPatientDetailsAsync(id);
            if (patient == null) return NotFound("Patient Not Found");
            return await LoadPatientFormAsync(patient);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePatient(Patient patient)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = 0, message = "Datos inválidos." });

            try
            {
                if (patient.PatientId > 0)
                    await _patientService.UpdatePatientAsync(patient);
                else
                    await _patientService.CreatePatientAsync(patient);

                TempData["SuccessMessage"] = "Paciente actualizado exitosamente.";
                return RedirectToAction("PatientList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;  
                return await LoadPatientFormAsync(patient);
            }
        }

        /// <summary>
        /// Carga de todos los datos y listados
        /// </summary>
        /// <param name="patient"></param>
        /// <returns></returns>
        private async Task<IActionResult> LoadPatientFormAsync(Patient patient = null, int? establishmentId = null)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            var estid = HttpContext.Session.GetInt32("UsuarioEstablecimientoId") ?? 0;
            var viewModel = new NewPatientViewModel
            {
                Patient = patient,
                GenderTypes = await _selectService.GetGenderTypeAsync(),
                BloodTypes = await _selectService.GetBloodTypeAsync(),
                DocumentTypes = await _selectService.GetDocumentTypeAsync(),
                CivilTypes = await _selectService.GetCivilTypeAsync(),
                ProfessionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync(),
                SureHealthTypes = await _selectService.GetSureHealtTypeAsync(),
                Countries = await _selectService.GetAllCountriesAsync(),
                Provinces = await _selectService.GetAllProvinceAsync(),
                Users = await _patientService.GetDoctorsByAssistantAsync(usuarioId, perfilId),
                UsersP = establishmentId.HasValue
                    ? await _selectService.GetAllMedicsDetailsAsync(establishmentId.Value)
                    : new List<MedicDetails>(), // o retornar todos, según tu lógica
                InsuranceCompanies =  await _selectService.GetInsuranceByEstablishmentAsync(estid)
                  
            };

            return View(viewModel);
        }


    }
}
