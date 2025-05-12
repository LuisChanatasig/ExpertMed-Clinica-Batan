using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExpertMed.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly AppointmentService _appointmentService;
        private readonly PatientService _patientService;

        public AppointmentController(ILogger<AppointmentController> logger, AppointmentService appointmentService,PatientService patientService)
        {
            _logger = logger;
            _appointmentService = appointmentService;
            _patientService = patientService;
        }
        public class ErrorViewModel
        {
            public string Message { get; set; }
        }

        /// <summary>
        /// Listado de citas vista
        /// </summary>
        /// <param name="appointmentStatus"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AppointmentList(int appointmentStatus = 1)
        {
            try
            {
                // Obtener información del usuario desde la sesión
                var userId = HttpContext.Session.GetInt32("UsuarioId");
                var userProfile = HttpContext.Session.GetInt32("PerfilId");

                // Verificar si los valores necesarios están presentes
                if (!userId.HasValue || !userProfile.HasValue)
                {
                    TempData["Error"] = "Por favor, inicie sesión para continuar.";
                    return RedirectToAction("SignIn", "Authentication");
                }

                // Establecer valores en ViewBag para usarlos en la vista
                ViewBag.CurrentStatus = appointmentStatus;
                ViewBag.UserProfile = userProfile.Value;
                ViewBag.UserId = userId.Value;

                // Obtener citas a través del servicio
                var appointments = await _appointmentService.GetAllAppointmentAsync(
                    userProfile.Value,
                    appointmentStatus,
                    userId
                );

                // Verificar si no se obtuvieron citas
                if (appointments == null || !appointments.Any())
                {
                    TempData["Info"] = "No se encontraron citas para los parámetros especificados.";
                    return View(new List<Appointment>());
                }

                // Retornar la vista con las citas obtenidas
                return View(appointments);
            }
            catch (Exception ex)
            {
                // Registrar error y retornar una vista vacía con mensaje de error
                _logger.LogError($"Unhandled exception in AppointmentList: {ex.Message}");
                TempData["Error"] = "Ocurrió un error inesperado. Inténtalo de nuevo más tarde.";
                return View(new List<Appointment>());
            }
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
                if (!TimeOnly.TryParse(request.AppointmentHour.ToString(), out TimeOnly appointmentHour))
                {
                    return BadRequest(new { success = false, message = "Formato de hora inválido." });
                }

                // Crear objeto de cita incluyendo consultorio
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
                    AppointmentMedicalofficeid = request.AppointmentMedicalofficeid  // <- NUEVO
                };

                // Llamar al servicio
                await _appointmentService.CreateAppointmentAsync(appointment, doctorUserId);

                // WhatsApp reminder
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    var message = $"¡Hola {patient.PatientFirstname.Trim()}! Te recordamos que tienes una cita programada para el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:HH:mm}. ¡Será un gusto atenderte!";
                    var encodedMessage = WebUtility.UrlEncode(message);
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMessage}";
                }

                return Ok(new { success = true, message = "CITA CREADA CON ÉXITO", whatsappUrl });
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
            {
                return BadRequest(new { success = false, message = "El cuerpo de la solicitud está vacío." });
            }

            try
            {
                var usuarioId = request.AppointmentModifyuser;
                var perfilId = 2;

                if (usuarioId == null)
                {
                    return Unauthorized(new { success = false, message = "Usuario no autenticado o la sesión expiró." });
                }

                // Convertir la hora desde string (por ejemplo "16:00") a TimeOnly
                if (!TimeOnly.TryParse(request.AppointmentHour.ToString(), out TimeOnly appointmentHour))
                {
                    return BadRequest(new { success = false, message = "Formato de hora inválido." });
                }

                // Crear el objeto de la cita
                var appointment = new Appointment
                {
                    AppointmentCreatedate = DateTime.Now,
                    AppointmentModifydate = DateTime.Now,
                    AppointmentCreateuser = usuarioId.Value,
                    AppointmentModifyuser = usuarioId.Value,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentHour = appointmentHour,  // Asignar la hora convertida
                    AppointmentPatientid = request.AppointmentPatientid,
                    AppointmentStatus = 1,
                };

                // Llamar al servicio para crear la cita
                await _appointmentService.CreateAppointmentAsync(appointment, doctorUserId);

                // Lógica adicional para generar la URL de recordatorio vía WhatsApp
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    // Construir el mensaje de recordatorio de forma amigable
                    var message = $"¡Hola {patient.PatientFirstname.Trim()}! Te recordamos que tienes una cita programada para el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:HH:mm}. ¡Será un gusto atenderte!";
                    // Codificar el mensaje para incluirlo en la URL
                    var encodedMessage = WebUtility.UrlEncode(message);
                    // Usar la URL de la API de WhatsApp para una redirección más fluida
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMessage}";
                }

                // Devolver la respuesta con la URL de WhatsApp (si se pudo generar)
                return Ok(new { success = true, message = "CITA CREADA CON ÉXITO", whatsappUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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
                    doctorUserId = (userProfile == 3) ? appt.DoctorUserId : (int?)null,
                    medicalOfficeId = appt.AppointmentMedicalofficeid,
                    status = appt.AppointmentStatus,
                    hasConsultation = appt.AppointmentConsultationid.HasValue && appt.AppointmentConsultationid.Value > 0
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la cita por ID.");
                return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }

        /// <summary>
        /// Modificar reagendar una cita
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        [HttpPost("ModifyAppointment")]
        public async Task<IActionResult> ModifyAppointment([FromBody] Appointment request)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

                // Lógica para modificar la cita, ahora con el ID de consultorio
                var appointment = new Appointment
                {
                    AppointmentId = request.AppointmentId,
                    AppointmentModifydate = DateTime.Now,
                    AppointmentModifyuser = usuarioId ?? 0,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentHour = request.AppointmentHour,
                    AppointmentPatientid = request.AppointmentPatientid,
                    AppointmentStatus = request.AppointmentStatus ?? 1,
                    AppointmentMedicalofficeid = request.AppointmentMedicalofficeid // nuevo campo
                };

                await _appointmentService.ModifyAppointmentAsync(appointment);

                // Construcción de mensaje WhatsApp si el paciente tiene número registrado
                string whatsappUrl = null;
                var patient = await _patientService.GetPatientDetailsAsync(appointment.AppointmentPatientid ?? 0);
                if (patient != null && !string.IsNullOrEmpty(patient.PatientCellularPhone))
                {
                    var message = $"¡Hola {patient.PatientFirstname.Trim()}! Queríamos avisarte que tu cita médica se ha reagendado para el {appointment.AppointmentDate:dd/MM/yyyy} a las {appointment.AppointmentHour:HH:mm}. ¡Estamos encantados de poder atenderte y esperamos verte pronto! Si tienes alguna pregunta, no dudes en contactarnos.";
                    var encodedMessage = WebUtility.UrlEncode(message);
                    whatsappUrl = $"https://api.whatsapp.com/send?phone={patient.PatientCellularPhone}&text={encodedMessage}";
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
        public async Task<IActionResult> ModifyAppointmentA([FromBody] Appointment request )
        {
            try
            {
               
                // Lógica para modificar la cita
                var appointment = new Appointment
                {
                    AppointmentId = request.AppointmentId,                  // ID de la cita a modificar
                    AppointmentModifydate = DateTime.Now,                   // Fecha de modificación
                    AppointmentModifyuser = request.AppointmentModifyuser ?? 0,                 // Usuario que realiza la modificación
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
                return BadRequest(new { success = false, message = "Datos inválidos." });

            var result = await _appointmentService.InsertVitalSignsAsync(model);

            if (result.StartsWith("Error"))
                return StatusCode(500, new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }

    }
}
