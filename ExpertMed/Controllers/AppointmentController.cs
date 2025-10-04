using ExpertMed.Models;
using ExpertMed.Services;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExpertMed.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;
        private readonly SelectsService _selectService;

        public AppointmentController(ILogger<AppointmentController> logger, AppointmentService appointmentService, PatientService patientService,SelectsService selectsService)
        {
            _logger = logger;
            _appointmentService = appointmentService;
            _patientService = patientService;
            _selectService = selectsService;
        }
        public class ErrorViewModel
        {
            public string Message { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appointmentStatus"></param>
        /// <param name="appointmentStatus2"></param>
        /// <param name="isPaidOnly"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AppointmentList(
            int? appointmentStatus,      // <- sin defaults
            int? appointmentStatus2,     // <- sin defaults
            bool isPaidOnly = false)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UsuarioId");
                var userProfile = HttpContext.Session.GetInt32("PerfilId");
                if (!userId.HasValue || !userProfile.HasValue)
                {
                    TempData["Error"] = "Por favor, inicie sesión para continuar.";
                    return RedirectToAction("SignIn", "Authentication");
                }

                // Primera carga (no hay QS ni params): Activas + Emergencias
                if (!appointmentStatus.HasValue && !appointmentStatus2.HasValue && !Request.QueryString.HasValue)
                {
                    appointmentStatus = 1;
                    appointmentStatus2 = 5;
                }

                // “Todas” => ignora el segundo estado
                if (appointmentStatus == -1)
                    appointmentStatus2 = null;

                // ViewBags usados por la vista
                ViewBag.CurrentStatus = appointmentStatus ?? -1;
                ViewBag.CurrentStatus2 = appointmentStatus2;
                ViewBag.IsPaidOnly = isPaidOnly;
                ViewBag.UserProfile = userProfile.Value;
                ViewBag.UserId = userId.Value;

                // Traer citas
                var appointments = await _appointmentService.GetAllAppointmentAsync(
                    userProfile.Value,
                    appointmentStatus ?? -1,  // -1 = todas
                    userId.Value,
                    isPaidOnly,
                    appointmentStatus2        // null => ignora segundo estado
                ) ?? new List<AppointmentDTO>();

                // Construir VM
                var vm = new AppointmentListViewModel
                {
                    Appointments = appointments
                };

                // Carga de catálogos/form de paciente para el modal de agendar
                var newPatientForm = await BuildNewPatientViewModelAsync();
                vm.Users = newPatientForm.Users ?? new List<User>();
                vm.InsuranceCompanies = newPatientForm.InsuranceCompanies ?? new List<InsuranceCompanyDto>();
                vm.Patient = newPatientForm.Patient;
                vm.GenderTypes = newPatientForm.GenderTypes ?? new List<Catalog>();
                vm.BloodTypes = newPatientForm.BloodTypes ?? new List<Catalog>();
                vm.CivilTypes = newPatientForm.CivilTypes ?? new List<Catalog>();
                vm.ProfessionalTrainingTypes = newPatientForm.ProfessionalTrainingTypes ?? new List<Catalog>();
                vm.SureHealthTypes = newPatientForm.SureHealthTypes ?? new List<Catalog>();
                vm.Countries = newPatientForm.Countries ?? new List<Country>();
                vm.Provinces = newPatientForm.Provinces ?? new List<Province>();
                vm.UsersP = newPatientForm.UsersP ?? new List<MedicDetails>();

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in AppointmentList");
                TempData["Error"] = "Ocurrió un error inesperado. Inténtalo de nuevo más tarde.";
                return View(new AppointmentListViewModel { Appointments = new List<AppointmentDTO>() });
            }
        }
        // 2️⃣ Sigue existiendo para cuando quieras devolver SOLO el formulario:
        private async Task<IActionResult> LoadPatientFormAsync(
            Patient patient = null,
            int? establishmentId = null)
        {
            var viewModel = await BuildNewPatientViewModelAsync(patient, establishmentId);
            return View(viewModel);
        }

        // 1️⃣ Construye SOLO el modelo:
        private async Task<NewPatientViewModel> BuildNewPatientViewModelAsync(
            Patient patient = null,
            int? establishmentId = null)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            var estid = establishmentId ?? HttpContext.Session.GetInt32("UsuarioEstablecimientoId") ?? 0;

            return new NewPatientViewModel
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
                         : new List<MedicDetails>(),
                InsuranceCompanies = await _selectService.GetInsuranceByEstablishmentAsync(estid)
            };
        }

        /// <summary>
        /// Obtiene las horas del medico
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="date"></param>
        /// <param name="doctorUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAvailableHours([FromQuery] int userId, [FromQuery] DateTime date, [FromQuery] int? doctorUserId = null)
        {
            try
            {
                // Si doctorUserId es nulo, lo que indica que no es asistente, llamamos al servicio de la manera normal
                List<string> availableHours = _appointmentService.GetAvailableHours(userId, date, doctorUserId);

                if (availableHours.Count == 0)
                {
                    TempData["ErrorMessage"] = "No existen horas disponibles .";  // Almacenar el mensaje en TempData
                    return NoContent();  // Si no hay horas disponibles, devolver un estado 204 No Content
                }

                return Ok(availableHours);  // Si hay horas disponibles, devolverlas con un estado 200 OK
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";  // Almacenar el mensaje de error en TempData
                return StatusCode(500, new { message = ex.Message });  // Manejo de errores en caso de fallos en el servicio
            }
        }

        /// <summary>
        /// Obtiene los consultorios disponibles para una fecha y hora especifica
        /// </summary>
        /// <param name="date"></param>
        /// <param name="hour"></param>
        /// <param name="doctorUserId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAvailableOffices(DateTime date, string hour, int? doctorUserId = null)
        {
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            if (userId == null)
                return Unauthorized(new { success = false, message = "Sesión no válida." });

            if (!TimeSpan.TryParse(hour, out var parsedHour))
                return BadRequest(new { success = false, message = "Hora inválida." });

            try
            {
                var offices = await _appointmentService.GetAvailableOfficesAsync(userId.Value, date, parsedHour, doctorUserId);
                return Ok(new { success = true, offices });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Metodo para crear la cita
        /// </summary>
        /// <param name="request"></param>
        /// <param name="doctorUserId"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment request, [FromQuery] int? doctorUserId = null)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud está vacío." });
            }

            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var perfilId = HttpContext.Session.GetInt32("PerfilId");

                if (usuarioId == null)
                {
                    return Unauthorized(new { success = false, message = "Usuario no autenticado o la sesión expiró." });
                }

                // Validar formato de hora
                TimeOnly appointmentHour;
                try
                {
                    appointmentHour = TimeOnly.Parse(request.AppointmentHour.ToString());
                }
                catch
                {
                    return BadRequest(new { success = false, message = "Formato de hora inválido." });
                }

                // Convertir TimeOnly a TimeSpan para el servicio
                TimeSpan hourSpan = appointmentHour.ToTimeSpan();

                // Obtener oficinas disponibles automáticamente (sin mostrar)
                var availableOffices = await _appointmentService.GetAvailableOfficesAsync(
                    usuarioId.Value,
                    request.AppointmentDate.Date,
                    hourSpan,
                    doctorUserId
                );

                if (availableOffices == null || !availableOffices.Any())
                {
                    return BadRequest(new { success = false, message = "No hay consultorios disponibles para la fecha y hora seleccionadas." });
                }

                int selectedOfficeId = availableOffices.First().MedicalOfficeId;

                var appointment = new Appointment
                {
                    AppointmentCreatedate = DateTime.Now,
                    AppointmentModifydate = DateTime.Now,
                    AppointmentCreateuser = usuarioId.Value,
                    AppointmentModifyuser = usuarioId.Value,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentHour = appointmentHour,
                    AppointmentPatientid = request.AppointmentPatientid,
                    AppointmentStatus = request.AppointmentStatus,
                    AppointmentMedicalofficeid = selectedOfficeId,
                    AppointmentInsuranceCompanyId = request.AppointmentInsuranceCompanyId,
                    AppointmentInsuranceAuthCode = request.AppointmentInsuranceAuthCode,
                    AppointmentReason = request.AppointmentReason
                };

                var (success, message, appointmentId, isEmergency) = await _appointmentService.CreateAppointmentAsync(appointment, doctorUserId);

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                // Generar URL de WhatsApp para recordatorio
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    var reminderMessage = $"¡Hola {patient.PatientFirstname.Trim()}! Te recordamos que tienes una cita programada para el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:HH\\:mm}. ¡Será un gusto atenderte!";
                    var encodedMessage = WebUtility.UrlEncode(reminderMessage);
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMessage}";
                }

                return Ok(new
                {
                    success = true,
                    message = "CITA CREADA CON ÉXITO",
                    appointmentId,
                    isEmergency,
                    whatsappUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



        /// <summary>
        /// Crerar una cita  por fuera
        /// </summary>
        /// <param name="request"></param>
        /// <param name="doctorUserId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateAppointmentA([FromBody] Appointment request, [FromQuery] int? doctorUserId = null)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud está vacío." });

            try
            {
                var usuarioId = request.AppointmentModifyuser;
                if (usuarioId == null)
                    return Unauthorized(new { success = false, message = "Usuario no autenticado o sesión inválida." });

                // Validar y convertir la hora
                TimeOnly appointmentHour;
                if (request.AppointmentHour == default)
                {
                    // Si viene como string (por ejemplo "16:00"), intentar convertir
                    if (!TimeOnly.TryParse(request.AppointmentHour.ToString(), out appointmentHour))
                        return BadRequest(new { success = false, message = "Formato de hora inválido." });
                }
                else
                {
                    appointmentHour = request.AppointmentHour;
                }

                // Asignar status real (por ejemplo: 5 = emergencia)
                var appointmentStatus = request.AppointmentStatus;

                var appointment = new Appointment
                {
                    AppointmentCreatedate = DateTime.Now,
                    AppointmentModifydate = DateTime.Now,
                    AppointmentCreateuser = usuarioId.Value,
                    AppointmentModifyuser = usuarioId.Value,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentHour = appointmentHour,
                    AppointmentPatientid = request.AppointmentPatientid,
                    AppointmentStatus = appointmentStatus,
                    AppointmentMedicalofficeid = request.AppointmentMedicalofficeid
                };

                // Llamar al SP mediante el servicio
                var (success, message, appointmentId, isEmergency) = await _appointmentService.CreateAppointmentAsync(appointment, doctorUserId);

                if (!success)
                    return BadRequest(new { success = false, message });

                // Generar mensaje por WhatsApp si aplica
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    var msg = $"¡Hola {patient.PatientFirstname.Trim()}! Te recordamos que tienes una cita {(isEmergency ? "de emergencia " : "")}el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:hh\\:mm tt}. ¡Será un gusto atenderte!";
                    var encodedMsg = WebUtility.UrlEncode(msg);
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMsg}";
                }

                return Ok(new
                {
                    success = true,
                    message = "CITA CREADA CON ÉXITO",
                    appointmentId,
                    isEmergency,
                    whatsappUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error: " + ex.Message });
            }
        }


        /// <summary>
        /// Obtener una cita por el id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userProfile"></param>
        /// <returns></returns>



        [HttpGet("AppointmentGetById")]
        public IActionResult AppointmentGetById(int id, int userProfile)
        {
            try
            {
                var appt = _appointmentService.GetAppointmentById(id, userProfile);
                if (appt == null)
                    return NotFound(new { message = "La cita no se encontró." });

                // Formatea la hora
                string hora = appt.AppointmentHour.ToString("HH\\:mm");

                var response = new
                {
                    appointmentId = appt.AppointmentId,
                    patientId = appt.AppointmentPatientid,
                    date = appt.AppointmentDate.ToString("yyyy-MM-dd"),
                    time = hora,
                    doctorUserId = (userProfile == 1 || userProfile == 3 || userProfile == 4 || userProfile == 8) ? appt.DoctorUserId : (int?)null,
                    medicalOfficeId = appt.AppointmentMedicalofficeid,
                    status = appt.AppointmentStatus,
                    appointmentReason = appt.AppointmentReason,
                    appointmentInsuranceCompanyId = appt.AppointmentInsuranceCompanyId,
                    paymentStatus = appt.AppointmentPaymentStatus,
                    hasConsultation = appt.AppointmentConsultationid.HasValue && appt.AppointmentConsultationid.Value > 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la cita por ID.");
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud."+ex });
            }
        }

        /// <summary>
        /// Modificar reagendar una cita
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> ModifyAppointment([FromBody] Appointment request)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                // Prepara el DTO completo con todos los campos
                var appointment = new Appointment
                {
                    AppointmentId = request.AppointmentId,
                    AppointmentModifydate = DateTime.Now,
                    AppointmentModifyuser = usuarioId,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentHour = request.AppointmentHour,
                    AppointmentPatientid = request.AppointmentPatientid,
                    AppointmentStatus = request.AppointmentStatus ?? 1,
                    AppointmentMedicalofficeid = request.AppointmentMedicalofficeid,
                    DoctorUserId = request.DoctorUserId,                   // ← nuevo
                    AppointmentInsuranceCompanyId = request.AppointmentInsuranceCompanyId,  // ← nuevo
                    AppointmentInsuranceAuthCode = request.AppointmentInsuranceAuthCode,   // ← nuevo
                    AppointmentReason = request.AppointmentReason               // ← nuevo
                };

                await _appointmentService.ModifyAppointmentAsync(appointment);

                // Construye la URL de WhatsApp si el paciente tiene celular
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    var msg = $"¡Hola {patient.PatientFirstname.Trim()}! " +
                              $"Tu cita se reagendó para el {appointment.AppointmentDate:dd/MM/yyyy} " +
                              $"a las {appointment.AppointmentHour:HH:mm}. " +
                              "Si tienes alguna duda, estamos a tu disposición.";
                    var encoded = WebUtility.UrlEncode(msg);
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encoded}";
                }

                return Ok(new
                {
                    success = true,
                    message = "CITA ACTUALIZADA CON ÉXITO",
                    whatsappUrl
                });
            }
            catch (Exception ex)
            {
                // Aquí podrías distinguir errores de validación vs. de sistema si quieres
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("ModifyAppointmentA")]
        public async Task<IActionResult> ModifyAppointmentA([FromBody] Appointment request)
        {
            try
            {

                // Lógica para modificar la cita
                var appointment = new Appointment
                {
                    AppointmentId = request.AppointmentId,                  // ID de la cita a modificar
                    AppointmentModifydate = DateTime.Now,                   // Fecha de modificación
                    AppointmentModifyuser = request.AppointmentModifyuser ?? 0,  // Usuario que realiza la modificación
                    AppointmentDate = request.AppointmentDate,              // Nueva fecha de la cita
                    AppointmentHour = request.AppointmentHour,              // Nueva hora de la cita
                    AppointmentPatientid = request.AppointmentPatientid,    // ID del paciente
                    AppointmentStatus = request.AppointmentStatus ?? 1      // Estado de la cita (por defecto 1 si no se especifica)
                };

                await _appointmentService.ModifyAppointmentAsync(appointment);

                // Eliminar la lógica de WhatsApp
                return Ok(new { success = true, message = "CITA ACTUALIZADA CON ÉXITO" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost("desactivate")]
        public IActionResult DesactivateAppointment([FromBody] Appointment request)
        {
            // Validar que la petici�n sea correcta
            if (request.AppointmentId <= 0 || request.AppointmentModifyuser <= 0)
            {
                return BadRequest(new { message = "Los par�metros proporcionados no son v�lidos." });
            }

            try
            {
                // Llamar al servicio para desactivar la cita
                _appointmentService.DesactivateAppointment(request.AppointmentId, request.AppointmentModifyuser ?? 0);

                // Retornar una respuesta exitosa en formato JSON
                return Ok(new { message = "Cita desactivada correctamente." });
            }
            catch (Exception ex)
            {
                // En caso de error, devolver mensaje de error en formato JSON
                return StatusCode(500, new { message = $"Error al desactivar la cita: {ex.Message}" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> SendWhatsAppReminder(int appointmentId, int userProfile)
        {
            // Obtener la cita
            var appointment = _appointmentService.GetAppointmentById(appointmentId, userProfile);
            if (appointment == null)
            {
                return NotFound(new { message = "Cita no encontrada." });
            }

            // Obtener los detalles del paciente usando el servicio
            var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
            if (patient == null)
            {
                return NotFound(new { message = "Paciente no encontrado." });
            }

            // Validar que el paciente tenga un número celular registrado
            if (string.IsNullOrEmpty(patient.PatientCellularPhone))
            {
                return BadRequest(new { message = "El paciente no tiene un número celular registrado." });
            }

            // Construir el nombre completo asegurando espacios correctos
            var fullName = $"{patient.PatientFirstname.Trim()} {patient.PatientFirstsurname.Trim()}";

            // Construir el mensaje de recordatorio (más amigable)
            var message = $"¡Hola {fullName}! Esperamos que estés teniendo un excelente día. Te recordamos que tienes una cita programada para el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:HH:mm}. ¡Será un gusto atenderte y compartir un buen momento! Si tienes cualquier duda, estamos aquí para ayudarte. ¡Nos vemos pronto!";

            // Codificar el mensaje para URL
            var encodedMessage = WebUtility.UrlEncode(message);

            // Construir la URL para WhatsApp usando la API (en algunos dispositivos se redirige de forma más inmediata)
            var whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMessage}";

            // Redirigir directamente a la URL de WhatsApp
            return Redirect(whatsappUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAppointmentsForToday()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return Unauthorized("Usuario no autenticado.");

            var appointments = await _appointmentService.GetAppointmentsForTodayAsync(usuarioId.Value);

            if (appointments == null || appointments.Count == 0)
                return NotFound("No appointments found for today.");

            return Ok(appointments);
        }

        /// <summary>
        /// Endpoint para validar si ya existe una cita para ese paciente y fecha.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="patientId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ValidateAppointment(DateTime date, int patientId)
        {
            var appointment = _appointmentService.GetAppointmentByPatientAndDay(patientId, date);
            if (appointment != null)
            {
                return Json(new { exists = true, appointmentId = appointment.AppointmentId });

            }
            return Json(new { exists = false });
        }

        /// <summary>
        /// Insertar los signos vitales
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> InsertVitalSigns([FromBody] VitalSignInputModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, message = "Datos inválidos.", errors });
            }

            var result = await _appointmentService.InsertVitalSignsAsync(model);

            if (result.StartsWith("Error"))
                return StatusCode(500, new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }

    }
}
