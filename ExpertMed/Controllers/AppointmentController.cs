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
        /// Displays a list of appointments filtered by status and payment criteria for the current user.
        /// </summary>
        /// <remarks>If both <paramref name="appointmentStatus"/> and <paramref
        /// name="appointmentStatus2"/> are null and no query string is present, the method defaults to showing active
        /// and emergency appointments. The method requires the user to be authenticated; otherwise, it redirects to the
        /// sign-in page.</remarks>
        /// <param name="appointmentStatus">The primary appointment status to filter the list. Use -1 to include all statuses. If null and no query
        /// string is present, defaults to active appointments.</param>
        /// <param name="appointmentStatus2">An optional secondary appointment status to further filter the list. If null, the secondary status is
        /// ignored.</param>
        /// <param name="isPaidOnly">Specifies whether to include only paid appointments. Set to <see langword="true"/> to filter for paid
        /// appointments; otherwise, all appointments are included.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the appointment list view with the filtered appointments, or
        /// redirects to the sign-in page if the user is not authenticated.</returns>
        [HttpGet]
        public async Task<IActionResult> AppointmentList(
           int? appointmentStatus,
           int? appointmentStatus2,
           bool isPaidOnly = false)
        {
            try
            {
                // 1. Validación de Sesión (Lógica de Seguridad)
                var userId = HttpContext.Session.GetInt32("UsuarioId");
                var userProfile = HttpContext.Session.GetInt32("PerfilId");

                if (!userId.HasValue || !userProfile.HasValue)
                {
                    TempData["Error"] = "Por favor, inicie sesión para continuar.";
                    return RedirectToAction("SignIn", "Authentication");
                }

                // 2. Lógica de Parámetros de Filtro
                // Si es la carga inicial (sin parámetros), enviamos NULL para que el SP aplique sus defaults (1 y 5)
                if (!appointmentStatus.HasValue && !appointmentStatus2.HasValue && !Request.QueryString.HasValue)
                {
                    appointmentStatus = null;
                    appointmentStatus2 = null;
                }
                else if (appointmentStatus == -1)
                {
                    // Si el usuario selecciona "Todas", forzamos nulidad en el segundo estado
                    appointmentStatus2 = null;
                }

                // 3. Llamada al Servicio (Capa de Datos)
                var appointments = await _appointmentService.GetAllAppointmentAsync(
                    userProfile.Value,
                    appointmentStatus, // El servicio y SP ya aceptan int?
                    userId.Value,
                    isPaidOnly,
                    appointmentStatus2
                ) ?? new List<AppointmentDTO>();

                // 4. Construcción del ViewModel Principal
                var vm = new AppointmentListViewModel
                {
                    Appointments = appointments
                };

                // 5. Carga de Catálogos (Agrupado para mantener orden)
                // Se asume que BuildNewPatientViewModelAsync ya es eficiente
                var catalogData = await BuildNewPatientViewModelAsync();

                vm.Users = catalogData.Users ?? new List<User>();
                vm.InsuranceCompanies = catalogData.InsuranceCompanies ?? new List<InsuranceCompanyDto>();
                vm.Patient = catalogData.Patient;
                vm.GenderTypes = catalogData.GenderTypes ?? new List<Catalog>();
                vm.BloodTypes = catalogData.BloodTypes ?? new List<Catalog>();
                vm.CivilTypes = catalogData.CivilTypes ?? new List<Catalog>();
                vm.ProfessionalTrainingTypes = catalogData.ProfessionalTrainingTypes ?? new List<Catalog>();
                vm.SureHealthTypes = catalogData.SureHealthTypes ?? new List<Catalog>();
                vm.Countries = catalogData.Countries ?? new List<Country>();
                vm.Provinces = catalogData.Provinces ?? new List<Province>();
                vm.UsersP = catalogData.UsersP ?? new List<MedicDetails>();

                // 6. ViewBags para persistencia en UI
                ViewBag.CurrentStatus = appointmentStatus ?? 1; // Para que el Select muestre "Activas" por defecto
                ViewBag.CurrentStatus2 = appointmentStatus2;
                ViewBag.IsPaidOnly = isPaidOnly;
                ViewBag.UserProfile = userProfile.Value;
                ViewBag.UserId = userId.Value;

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico en AppointmentList para Usuario: {UserId}", HttpContext.Session.GetInt32("UsuarioId"));
                TempData["Error"] = "Ocurrió un error al cargar la lista de citas.";
                return View(new AppointmentListViewModel { Appointments = new List<AppointmentDTO>() });
            }
        }


        /// <summary>
        /// Asynchronously prepares and returns the patient form view for creating or editing a patient record.
        /// </summary>
        /// <param name="patient">An optional <see cref="Patient"/> object representing the patient to edit. If <see langword="null"/>, the
        /// form will be initialized for creating a new patient.</param>
        /// <param name="establishmentId">An optional identifier for the establishment associated with the patient. If specified, the form will be
        /// pre-populated with establishment-specific data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the patient form view.</returns>
        private async Task<IActionResult> LoadPatientFormAsync(
            Patient patient = null,
            int? establishmentId = null)
        {
            var viewModel = await BuildNewPatientViewModelAsync(patient, establishmentId);
            return View(viewModel);
        }

        /// <summary>
        /// Asynchronously builds and returns a view model for creating a new patient, including selectable lists and
        /// user context information.
        /// </summary>
        /// <remarks>The returned view model includes lists for gender, blood type, document type, civil
        /// status, professional training, health insurance, countries, provinces, and users relevant to the current
        /// session or specified establishment. This method relies on session values to determine user and establishment
        /// context when parameters are not provided.</remarks>
        /// <param name="patient">An optional patient entity to prepopulate the view model. If null, the view model will be initialized for a
        /// new patient.</param>
        /// <param name="establishmentId">An optional establishment identifier used to filter selectable lists and user data. If null, the value is
        /// obtained from the current session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a populated NewPatientViewModel
        /// with patient data and related selection lists.</returns>
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
        /// Retrieves the list of available appointment hours for a specified user and date, optionally filtered by
        /// doctor.
        /// </summary>
        /// <remarks>If no available hours are found, the response will have no content and an error
        /// message will be stored in TempData. In case of an error, a descriptive message is returned in the response
        /// and stored in TempData.</remarks>
        /// <param name="userId">The unique identifier of the user for whom available hours are requested.</param>
        /// <param name="date">The date for which to retrieve available appointment hours.</param>
        /// <param name="doctorUserId">The unique identifier of the doctor to filter available hours. If null, available hours are retrieved
        /// without filtering by doctor.</param>
        /// <returns>An HTTP 200 response containing a list of available hour strings if any are found; otherwise, an HTTP 204 No
        /// Content response if no hours are available. Returns an HTTP 500 response if an error occurs.</returns>
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



        [HttpGet]
        public IActionResult GetAvailableHoursEvents(int userId, DateTime start, DateTime end)
        {
            try
            {
                var eventList = new List<object>();
                int slotDuration = 30; // Minutos para dar cuerpo visual al bloque

                for (DateTime date = start.Date; date <= end.Date; date = date.AddDays(1))
                {
                    // Llama a tu servicio que ejecuta el SP sp_GetAvailableHours
                    List<string> hours = _appointmentService.GetAvailableHours(userId, date, null);

                    foreach (var h in hours)
                    {
                        DateTime startTime = DateTime.Parse(date.ToString("yyyy-MM-dd") + " " + h);
                        eventList.Add(new
                        {
                            title = "Disponible",
                            start = startTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                            end = startTime.AddMinutes(slotDuration).ToString("yyyy-MM-ddTHH:mm:ss"),
                            // Clase CSS personalizada para el diseño premium
                            className = "event-slot-available"
                        });
                    }
                }
                return Ok(eventList);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Retrieves a list of available offices for scheduling appointments at the specified date and hour, optionally
        /// filtered by doctor.
        /// </summary>
        /// <remarks>Returns an unauthorized response if the user session is invalid. Returns a bad
        /// request response if the hour parameter is not a valid time or if an error occurs during
        /// processing.</remarks>
        /// <param name="date">The date for which to check office availability.</param>
        /// <param name="hour">The hour, in 'HH:mm' format, for which to check office availability. Must be a valid time string.</param>
        /// <param name="doctorUserId">The user ID of the doctor to filter available offices by. If null, offices for all doctors are returned.</param>
        /// <returns>An IActionResult containing a JSON object with the success status and a list of available offices if the
        /// request is valid; otherwise, an error message.</returns>
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
        /// Creates a new appointment using the provided appointment details and assigns an available medical office
        /// automatically.
        /// </summary>
        /// <remarks>The method automatically selects an available medical office for the appointment
        /// based on the provided date, time, and doctor. If no offices are available, the request fails. The response
        /// may include a WhatsApp reminder URL for the patient if a valid cellular phone number is available. The
        /// caller must be authenticated; otherwise, an unauthorized response is returned.</remarks>
        /// <param name="request">The appointment information to be created. Must not be null and should contain valid date, time, patient,
        /// and status details.</param>
        /// <param name="doctorUserId">The optional user ID of the doctor for whom the appointment is being scheduled. If not specified, the
        /// appointment will be assigned based on the current session user.</param>
        /// <returns>An IActionResult containing the result of the appointment creation. Returns a success response with
        /// appointment details if created successfully; otherwise, returns an error response indicating the reason for
        /// failure.</returns>
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
        /// Creates a new appointment using the specified appointment details and optional doctor user identifier.
        /// </summary>
        /// <remarks>If the appointment is created successfully and the patient has a registered cellular
        /// phone, a WhatsApp message link is generated for appointment reminders. The method validates the appointment
        /// time and user authentication before creating the appointment.</remarks>
        /// <param name="request">The appointment information to be created. Must not be null and should contain valid appointment date, time,
        /// patient, and status details.</param>
        /// <param name="doctorUserId">The optional identifier of the doctor user associated with the appointment. If not provided, the appointment
        /// will be created without linking to a specific doctor.</param>
        /// <returns>An IActionResult containing the result of the appointment creation. Returns a success response with
        /// appointment details if creation is successful; otherwise, returns an error response indicating the reason
        /// for failure.</returns>
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
        /// Retrieves the details of an appointment by its unique identifier and user profile.
        /// </summary>
        /// <remarks>The returned appointment details vary based on the specified user profile. For
        /// certain user profiles, the doctor user ID is included; for others, it is omitted. The method returns
        /// formatted date and time values for easier client consumption.</remarks>
        /// <param name="id">The unique identifier of the appointment to retrieve.</param>
        /// <param name="userProfile">The user profile type requesting the appointment. Determines which appointment details are included in the
        /// response. Valid values correspond to specific user roles.</param>
        /// <returns>An <see cref="IActionResult"/> containing the appointment details if found; otherwise, a 404 Not Found
        /// result if the appointment does not exist, or a 500 Internal Server Error result if an unexpected error
        /// occurs.</returns>
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
        /// Modifies an existing appointment with the specified details and returns the result of the operation.
        /// </summary>
        /// <remarks>If the patient associated with the appointment has a registered cellular phone
        /// number, the response includes a WhatsApp URL for sending a notification message. The appointment
        /// modification is performed using the current session user as the modifying user. Returns a bad request result
        /// if an error occurs during processing.</remarks>
        /// <param name="request">An <see cref="Appointment"/> object containing the updated appointment information. All required fields must
        /// be provided; the appointment to modify is identified by <c>AppointmentId</c>.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the outcome of the modification. If successful, the result
        /// includes a success message and, if available, a WhatsApp URL for patient notification.</returns>
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
        /// Modifies an existing appointment with the specified details.
        /// </summary>
        /// <remarks>This method updates the appointment using the provided details. If the appointment
        /// status is not specified, it defaults to 1. The modification date is set to the current date and time. The
        /// method does not perform any notification actions.</remarks>
        /// <param name="request">The appointment information to update. Must include a valid appointment ID and the updated appointment
        /// details.</param>
        /// <returns>An IActionResult indicating the result of the operation. Returns a success message if the appointment is
        /// updated; otherwise, returns an error message.</returns>
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

        /// <summary>
        /// Deactivates an existing appointment based on the provided appointment details.
        /// </summary>
        /// <remarks>This endpoint expects a valid appointment ID and modifying user ID. If the parameters
        /// are invalid or an error occurs during deactivation, an error response is returned. The response is formatted
        /// as JSON.</remarks>
        /// <param name="request">An <see cref="Appointment"/> object containing the appointment ID and the user ID performing the
        /// modification. The appointment ID and modifying user ID must be greater than zero.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns a success message if the
        /// appointment is deactivated; otherwise, returns an error message with the appropriate HTTP status code.</returns>
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

        /// <summary>
        /// Sends a WhatsApp reminder message to the patient associated with the specified appointment and redirects the
        /// caller to the WhatsApp messaging interface.
        /// </summary>
        /// <remarks>The patient must have a registered cellular phone number to receive the WhatsApp
        /// reminder. If the appointment or patient cannot be found, or if the patient lacks a cellular phone number, an
        /// appropriate error response is returned.</remarks>
        /// <param name="appointmentId">The unique identifier of the appointment for which the reminder will be sent.</param>
        /// <param name="userProfile">The identifier of the user profile used to retrieve appointment details.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the WhatsApp messaging interface if the reminder can be
        /// sent; otherwise, a result indicating why the reminder could not be sent, such as not found or bad request.</returns>
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
        /// Retrieves the list of appointments scheduled for the current day for the authenticated user.
        /// </summary>
        /// <remarks>This method requires the user to be authenticated and have a valid session. The
        /// returned appointments are specific to the user associated with the current session.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing the list of today's appointments for the authenticated user.
        /// Returns <see cref="UnauthorizedResult"/> if the user is not authenticated, <see cref="NotFoundResult"/> if
        /// no appointments are found, or <see cref="OkObjectResult"/> with the appointments if successful.</returns>
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
            // Usamos el servicio que ya mapea correctamente el TimeOnly
            var appointment = _appointmentService.GetAppointmentByPatientAndDay(patientId, date);

            if (appointment != null)
            {
                return Json(new
                {
                    exists = true,
                    appointmentId = appointment.AppointmentId,
                    // Enviamos la hora formateada para mostrarla en el SweetAlert
                    hour = appointment.AppointmentHour.ToString("HH:mm")
                });
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
