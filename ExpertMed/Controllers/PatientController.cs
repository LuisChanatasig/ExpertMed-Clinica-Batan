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
        private readonly SignatureService _signatureQrService;

        public PatientController(UserService usersService, ILogger<PatientController> logger, SelectsService selectService, PatientService patientService, SignatureService signatureQrService)
        {
            _usersService = usersService;
            _logger = logger;
            _selectService = selectService;
            _patientService = patientService;
            _signatureQrService = signatureQrService;
        }
        /// <summary>
        /// Metodo para obtener los detalles del paciente
        /// </summary>
        /// <param name="patientId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Changes the status of the specified patient to active or inactive.
        /// </summary>
        /// <remarks>Displays a success or error message based on the outcome of the status change
        /// operation. The method is intended to be called via an HTTP POST request.</remarks>
        /// <param name="patientId">The unique identifier of the patient whose status will be updated.</param>
        /// <param name="status">The new status value to assign to the patient. Typically, use 1 for active and 0 for inactive.</param>
        /// <returns>A redirect result to the patient list view after the status change operation completes.</returns>
        [HttpPost]
        public async Task<IActionResult> ChangePatientStatus(int patientId, int status)
        {
            var result = await _patientService.DesactiveOrActivePatientAsync(patientId, status);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.message;
            return RedirectToAction("PatientList");
        }
        /// <summary>
        /// Displays the form for creating a new patient record.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the new patient form view.</returns>
        [HttpGet]
        public async Task<IActionResult> NewPatient() => await LoadPatientFormAsync();
        

        /// <summary>
        /// Displays the patient registration form, optionally pre-filtered by status.
        /// </summary>
        /// <param name="est">An optional status value used to filter the patient registration form. If null, no status filter is applied.</param>
        /// <returns>An asynchronous operation that returns an <see cref="IActionResult"/> representing the patient registration
        /// form view.</returns>
        [HttpGet]
        public async Task<IActionResult> RegistroPaciente(int? est = null)
        {
            return await LoadPatientFormBAsync(null, est);
        }


        /// <summary>
        /// Creates a new patient record and optionally associates it with a specified doctor.
        /// </summary>
        /// <remarks>If the patient data is invalid, the method returns a BadRequest containing details
        /// about the validation errors. On successful creation, a success message is stored in TempData and the user is
        /// redirected to the patient list. If an exception occurs during creation, an error message is stored and the
        /// patient registration view is returned.</remarks>
        /// <param name="patient">The patient information to be created. Must contain valid data according to the model requirements.</param>
        /// <param name="doctorUserId">The user ID of the doctor to associate with the new patient. If null, no doctor will be linked.</param>
        /// <returns>An IActionResult that redirects to the patient list on success, or returns a BadRequest with validation
        /// errors if the input is invalid.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPatient(Patient patient, int? doctorUserId = null, Guid? SignatureToken = null)
        {
            // 1. Validaciones iniciales
            if (!ModelState.IsValid) return BadRequest(new { success = 0, message = "Datos incompletos." });

            bool hasDirectSig = !string.IsNullOrWhiteSpace(patient.PatientSignature);
            bool hasQrToken = SignatureToken.HasValue && SignatureToken.Value != Guid.Empty;

            if (!hasDirectSig && !hasQrToken)
            {
                TempData["ErrorMessage"] = "Falta la firma del paciente (QR o firma local).";
                return await RegistroPaciente();
            }

            bool isQrMode = hasQrToken && !hasDirectSig;
            if (isQrMode) { patient.PatientSignature = null; }

            try
            {
                // 2. Contexto de Usuario
                int currentUserId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                patient.PatientCreationuser = currentUserId;
                patient.PatientModificationuser = currentUserId;

                // 3. Crear Paciente
                var resultado = await _patientService.CreatePatientAsync(patient, doctorUserId, skipSignatureInsert: isQrMode);

                if (!resultado.Success)
                {
                    TempData["ErrorMessage"] = resultado.Message;
                    return await RegistroPaciente();
                }

                // --- VINCULACIÓN DE DOCUMENTOS FISICOS ---
                List<string> signedFilesUrls = new List<string>();

                if (isQrMode && SignatureToken.HasValue)
                {
                    var st = await _signatureQrService.GetStatusAsync(SignatureToken.Value);
                    if (st != null)
                    {
                        resultado.SignatureData = st.SignatureDataUrl;
                        resultado.SignedAt = st.SignedAtLocal?.ToString("yyyy-MM-dd HH:mm:ss");

                        // RUTA REAL EN EL DISCO
                        string storageFolder = @"C:\ExpertMedStorage\DocumentosFirmados";
                        var docTypes = new[] {
                    new { Prefix = "CONSENT", Label = "Consentimiento" },
                    new { Prefix = "LOPDP", Label = "LOPDP" }
                };

                        if (Directory.Exists(storageFolder))
                        {
                            var directoryInfo = new DirectoryInfo(storageFolder);
                            foreach (var doc in docTypes)
                            {
                                // Buscamos cualquier archivo que empiece con el prefijo y contenga el GUID
                                var file = directoryInfo.GetFiles($"{doc.Prefix}_{SignatureToken.Value}*.pdf").FirstOrDefault();

                                if (file != null)
                                {
                                    // IMPORTANTE: Generamos la URL que apunta a nuestra acción de descarga
                                    string downloadUrl = Url.Action("Download", "Signature", new { fileName = file.Name });
                                    signedFilesUrls.Add(downloadUrl);

                                    // Guardamos metadata en la base de datos (SP sp_GuardarDocumentoFirmado)
                                    await _signatureQrService.SaveDocumentMetadataAsync(resultado.PatientId, file.Name, file.FullName, doc.Label);
                                }
                            }
                        }
                    }
                    await _signatureQrService.ConsumeToPatientAsync(SignatureToken.Value, resultado.PatientId);
                }

                // 4. Datos para el Modal (TempData)
                // El modal se abre solo si SecurityToken tiene valor
                TempData["SuccessMessage"] = "Registro completado con éxito.";
                TempData["SecurityToken"] = Guid.NewGuid().ToString("N"); // Token para el Acta
                TempData["PatientName"] = $"{patient.PatientFirstname} {patient.PatientFirstsurname}";
                TempData["PatientCode"] = resultado.PatientCode;
                TempData["SignatureData"] = resultado.SignatureData;
                TempData["SignedAt"] = resultado.SignedAt;
                TempData["SignedFiles"] = Newtonsoft.Json.JsonConvert.SerializeObject(signedFilesUrls);

                return RedirectToAction(nameof(NewPatient));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en registro");
                TempData["ErrorMessage"] = ex.Message;
                return await RegistroPaciente();
            }
        }

        /// <summary>
        /// Creates a new patient record or associates an existing patient with an emergency appointment, then redirects
        /// to the patient registration view.
        /// </summary>
        /// <remarks>If the patient already exists based on the document number, an emergency appointment
        /// is registered for the existing patient. Otherwise, a new patient is created and an emergency appointment is
        /// registered. Success or error messages are provided via TempData for use in the redirected view.</remarks>
        /// <param name="patient">The patient information to be created or checked for existence. Must contain valid data according to the
        /// model requirements.</param>
        /// <param name="doctorUserId">The user ID of the doctor creating or modifying the patient record. If null, no doctor will be associated.</param>
        /// <returns>An asynchronous action result that redirects to the patient registration view. Returns a bad request result
        /// if the patient data is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> NewPatientA(Patient patient, int? doctorUserId = null, bool isNewPatient = true)
        {

            // Asignamos el usuario que crea y modifica
            patient.PatientCreationuser = doctorUserId;
            patient.PatientModificationuser = doctorUserId;

            // 1. Validación de entrada básica
            if (string.IsNullOrEmpty(patient.PatientDocumentnumber))
                return Json(new { success = false, message = "El número de documento es obligatorio." });

            if (doctorUserId == null)
                return Json(new { success = false, message = "Debe seleccionar un médico." });

            try
            {
                int patientId;
                // Buscamos si ya existe en la base de datos
                var existingPatient = await _patientService.GetPatientDataByDocumentNumberAsync(patient.PatientDocumentnumber);

                if (isNewPatient)
                {
                    // CASO: PACIENTE NUEVO
                    if (existingPatient != null)
                    {
                        patientId = existingPatient.PatientId;
                    }
                    else
                    {
                        // CORRECCIÓN lógica del &&: Evaluamos si cualquiera de los dos está vacío
                        if (string.IsNullOrEmpty(patient.PatientFirstname) || string.IsNullOrEmpty(patient.PatientFirstsurname))
                            return Json(new { success = false, message = "Los nombres y apellidos son requeridos para nuevos registros." });

                        patient.PatientCreationuser = doctorUserId;

                        // CORRECCIÓN de conversión: Accedemos a la propiedad del objeto respuesta
                        var response = await _patientService.CreatePatientAsync(patient, doctorUserId);
                        patientId = response.PatientId; // Ajusta 'PatientId' según el nombre real en PatientCreateResponse
                    }
                }
                else
                {
                    // CASO: YA ES PACIENTE
                    if (existingPatient == null)
                    {
                        return Json(new { success = false, message = "No se encontró ningún paciente con ese documento. Por favor, regístrese como Paciente Nuevo." });
                    }
                    patientId = existingPatient.PatientId;
                }

                // 2. Respuesta para el AJAX
                return Json(new
                {
                    success = true,
                    patientId = patientId,
                    doctorUserId = doctorUserId,
                    message = "Validación exitosa"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno: " + ex.Message });
            }
        }



        /// <summary>
        /// Displays the patient update form for the specified patient identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the patient whose details are to be updated. Must be a valid patient ID.</param>
        /// <returns>An <see cref="IActionResult"/> that renders the patient update form if the patient exists; otherwise, a
        /// NotFound result if the patient is not found.</returns>
        [HttpGet]
        public async Task<IActionResult> UpdatePatient(int id)
        {
            var patient = await _patientService.GetPatientDetailsAsync(id);
            if (patient == null) return NotFound("Patient Not Found");
            return await LoadPatientFormAsync(patient);
        }

        /// <summary>
        /// Creates a new patient record or updates an existing one based on the provided patient information.
        /// </summary>
        /// <remarks>If the patient data is invalid, the method returns a BadRequest result. On successful
        /// creation or update, a success message is stored in TempData and the user is redirected to the patient list.
        /// If an exception occurs, an error message is stored and the patient form is reloaded.</remarks>
        /// <param name="patient">The patient data to create or update. Must contain valid patient information. The PatientId property
        /// determines whether a new record is created or an existing one is updated.</param>
        /// <param name="doctorUserId">The identifier of the doctor associated with the patient. If specified, the patient will be linked to this
        /// doctor. Optional.</param>
        /// <returns>An IActionResult that redirects to the patient list on success, or returns a form view with error
        /// information if the operation fails.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdatePatient(Patient patient,int? doctorUserId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = 0, message = "Datos inválidos." });

            try
            {
                if (patient.PatientId > 0)
                    await _patientService.UpdatePatientAsync(patient, doctorUserId); // ✅ pasar el valor recibido

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
        /// Asynchronously prepares and returns the view for creating or editing a patient, populating the form with
        /// relevant selection lists and user data.
        /// </summary>
        /// <param name="patient">An optional patient object to pre-populate the form fields. If null, the form will be initialized for a new
        /// patient.</param>
        /// <param name="establishmentId">An optional establishment identifier used to filter available medics and insurance companies. If not
        /// specified, the value is obtained from the current session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult that renders
        /// the patient form view with all necessary data for selection fields.</returns>
        private async Task<IActionResult> LoadPatientFormAsync(Patient patient = null, int? establishmentId = null)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            var estid = establishmentId ?? HttpContext.Session.GetInt32("UsuarioEstablecimientoId") ?? 0;
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

        /// <summary>
        /// Asynchronously loads the patient registration form (Form B) with relevant data for display, including
        /// patient details and selection lists.
        /// </summary>
        /// <remarks>The returned view model includes selection lists for gender, blood type, document
        /// type, civil status, professional training, health insurance, countries, provinces, and medical staff. The
        /// method uses session data to determine user and establishment context if parameters are not
        /// provided.</remarks>
        /// <param name="patient">The patient whose information will be pre-filled in the form. If null, the form will be initialized for a
        /// new patient.</param>
        /// <param name="establishmentId">The identifier of the establishment for which to load medical staff and related data. If null, the value is
        /// obtained from the current session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the patient registration form populated with the specified data.</returns>
        private async Task<IActionResult> LoadPatientFormBAsync(Patient patient = null, int? establishmentId = null)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            var estid = establishmentId ?? HttpContext.Session.GetInt32("UsuarioEstablecimientoId") ?? 0;
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
                    : new List<MedicDetails>()


            };

            return View(viewModel);
        }


    }
}


