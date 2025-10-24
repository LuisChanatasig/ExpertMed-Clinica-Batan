using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class ConsultationController : Controller
    {

        private readonly AppointmentService _citaService;
        private readonly PatientService _patientService;
        private readonly SelectsService _selectService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConsultationService _consultationService;

        private readonly ILogger<ConsultationController> _logger;
        private readonly DbExpertmedContext _medical_SystemContext;

        public ConsultationController(AppointmentService citaService, PatientService patientService, IHttpContextAccessor httpContextAccessor, SelectsService selectsService, ConsultationService consultationService, ILogger<ConsultationController> logger, DbExpertmedContext medical_SystemContext)
        {
            _citaService = citaService;
            _patientService = patientService;
            _httpContextAccessor = httpContextAccessor;
            _selectService = selectsService;
            _consultationService = consultationService;
            _logger = logger;
            _medical_SystemContext = medical_SystemContext;
        }




        [HttpGet]
        public async Task<IActionResult> ConsultationList(int? patientId = null)
        {
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            var profileId = HttpContext.Session.GetInt32("PerfilId");

            if (!userId.HasValue || !profileId.HasValue)
            {
                TempData["ErrorMessage"] = "Sesión expirada. Por favor, inicie sesión nuevamente.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var consultationsGrouped = await _consultationService.GetConsultationsAsync(
                    userId.Value,
                    profileId.Value,
                    patientId
                );

                // Información adicional para la vista
                ViewBag.TotalConsultas = consultationsGrouped.Sum(g => g.TotalConsultas);
                ViewBag.TotalPacientes = consultationsGrouped.Count;
                ViewBag.ConsultasPendientes = consultationsGrouped.Sum(g => g.Consultas.Count(c => c.AppointmentStatus != 4));

                return View(consultationsGrouped);
            }
            catch (ApplicationException appEx)
            {
                TempData["ErrorMessage"] = appEx.Message;
                return View(new List<ConsultationGroupViewModel>());
            }
            catch (Exception ex)
            {
                // Log el error (usa ILogger si está disponible)
                TempData["ErrorMessage"] = "Error inesperado al cargar las consultas. Por favor, contacte al administrador.";
                return View(new List<ConsultationGroupViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> NewConsultation(int patientId)
        {
            try
            {
                // Obtener los detalles del paciente
                var patient = await _patientService.GetPatientFullByIdAsync(patientId);

                // Si el paciente no existe, devolver una respuesta de "No encontrado"
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("AppointmentList", "Appointment");
                }

                // Obtener datos adicionales para la vista
                var genderTypes = await _selectService.GetGenderTypeAsync();
                var bloodTypes = await _selectService.GetBloodTypeAsync();
                var documentTypes = await _selectService.GetDocumentTypeAsync();
                var civilTypes = await _selectService.GetCivilTypeAsync();
                var professionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync();
                var sureHealthTypes = await _selectService.GetSureHealtTypeAsync();
                var countries = await _selectService.GetAllCountriesAsync();
                var provinces = await _selectService.GetAllProvinceAsync();
                var parents = await _selectService.GetRelationshipTypeAsync();
                var allergies = await _selectService.GetAllergiesTypeAsync();
                var surgeries = await _selectService.GetSurgeriesTypeAsync();
                var familyMember = await _selectService.GetFamiliarTypeAsync();
                var diagnosis = await _selectService.GetAllDiagnosisAsync();
                var medications = await _selectService.GetAllMedicationsAsync();
                var images = await _selectService.GetAllImagesAsync();
                var laboratories = await _selectService.GetAllLaboratoriesAsync();


                // Crear el ViewModel
                var viewModel = new NewPatientViewModel
                {
                    DetailsPatient = patient,
                    GenderTypes = genderTypes,
                    BloodTypes = bloodTypes,
                    DocumentTypes = documentTypes,
                    CivilTypes = civilTypes,
                    ProfessionalTrainingTypes = professionalTrainingTypes,
                    SureHealthTypes = sureHealthTypes,
                    Countries = countries,
                    Provinces = provinces,
                    Parents = parents,
                    AllergiesTypes = allergies,
                    SurgeriesTypes = surgeries,
                    FamilyMember = familyMember,
                    Diagnoses = diagnosis,
                    Medications = medications,
                    Images = images,
                    Laboratories = laboratories


                };

                // Retornar la vista con el modelo
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unexpected error: " + ex.Message;
                return View();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consultaDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CrearConsulta([FromBody] Consulta consultaDto)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Campo = x.Key, Errores = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                _logger.LogWarning("Errores de validación: {@Errores}", errores);

                return BadRequest(new { success = false, errores });
            }

            try
            {
                // 1) Llamada al servicio, capturando el ID (nuevo o existente)
                var newId = await _consultationService.CrearConsultaAsync(
                    consultaDto.ConsultationId,                                    // InputOutput
                    consultaDto.ConsultationCreationdate ?? DateTime.Now,
                    consultaDto.ConsultationUsercreate,
                    consultaDto.ConsultationPatient,
                    consultaDto.ConsultationSpeciality ?? 0,
                    consultaDto.ConsultationHistoryclinic,
                    consultaDto.ConsultationReason,
                    consultaDto.ConsultationDisease,
                    consultaDto.ConsultationFamiliaryname,
                    consultaDto.ConsultationWarningsings,
                    consultaDto.ConsultationNonpharmacologycal,
                    consultaDto.ConsultationFamiliarytype ?? 0,
                    consultaDto.ConsultationFamiliaryphone,
                    consultaDto.ConsultationTemperature,
                    consultaDto.ConsultationRespirationrate,
                    consultaDto.ConsultationBloodpressuredAs,
                    consultaDto.ConsultationBloodpresuredDis,
                    consultaDto.ConsultationPulse,
                    consultaDto.ConsultationWeight,
                    consultaDto.ConsultationSize,
                    consultaDto.ConsultationTreatmentplan, 
                    consultaDto.ConsultationObservation,
                    consultaDto.ConsultationPersonalbackground,
                    consultaDto.ConsultationDisablilitydays ?? 0,
                    consultaDto.ConsultationEvolutionNotes,
                    consultaDto.ConsultationTherapies,
                    consultaDto.ConsultationType ?? 0,
                    consultaDto.ConsultationStatus ?? 0,
                    consultaDto.ConsultationHasdisease ?? false,
                    consultaDto.ConsultationDiseaseobservation,
                    consultaDto.ConsultationContingencytype,
                    consultaDto.ConsutationHasSymptoms,
                    consultaDto.ConsultationIsFinal,

                    // Órganos y sistemas
                    consultaDto.OrgansSystem?.OrganssystemsOrgansenses,
                    consultaDto.OrgansSystem?.OrganssystemsOrgansensesObs,
                    consultaDto.OrgansSystem?.OrganssystemsRespiratory,
                    consultaDto.OrgansSystem?.OrganssystemsRespiratoryObs,
                    consultaDto.OrgansSystem?.OrganssystemsCardiovascular,
                    consultaDto.OrgansSystem?.OrganssystemsCardiovascularObs,
                    consultaDto.OrgansSystem?.OrganssystemsDigestive,
                    consultaDto.OrgansSystem?.OrganssystemsDigestiveObs,
                    consultaDto.OrgansSystem?.OrganssystemsGenital,
                    consultaDto.OrgansSystem?.OrganssystemsGenitalObs,
                    consultaDto.OrgansSystem?.OrganssystemsUrinary,
                    consultaDto.OrgansSystem?.OrganssystemsUrinaryObs,
                    consultaDto.OrgansSystem?.OrganssystemsSkeletalM,
                    consultaDto.OrgansSystem?.OrganssystemsSkeletalMObs,
                    consultaDto.OrgansSystem?.OrganssystemsEndrocrine,
                    consultaDto.OrgansSystem?.OrganssystemsEndocrine,
                    consultaDto.OrgansSystem?.OrganssystemsLymphatic,
                    consultaDto.OrgansSystem?.OrganssystemsLymphaticObs,
                    consultaDto.OrgansSystem?.OrganssystemsNervous,
                    consultaDto.OrgansSystem?.OrganssystemsNervousObs,
                    consultaDto.OrgansSystem?.OrganssystemsSkin,
                    consultaDto.OrgansSystem?.OrganssystemsSkinObs,


                    // Examen físico
                    consultaDto.PhysicalExamination?.PhysicalexaminationHead,
                    consultaDto.PhysicalExamination?.PhysicalexaminationHeadObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationNeck,
                    consultaDto.PhysicalExamination?.PhysicalexaminationNeckObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationChest,
                    consultaDto.PhysicalExamination?.PhysicalexaminationChestObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationAbdomen,
                    consultaDto.PhysicalExamination?.PhysicalexaminationAbdomenObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationPelvis,
                    consultaDto.PhysicalExamination?.PhysicalexaminationPelvisObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationLimbs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationLimbsObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationSkinfaneras,
                    consultaDto.PhysicalExamination?.PhysicalexaminationSkinfanerasObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationEyes,
                    consultaDto.PhysicalExamination?.PhysicalexaminationEyesObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationEars,
                    consultaDto.PhysicalExamination?.PhysicalexaminationEarsObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationNose,
                    consultaDto.PhysicalExamination?.PhysicalexaminationNoseObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationMouth,
                    consultaDto.PhysicalExamination?.PhysicalexaminationMouthObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationOropharynx,
                    consultaDto.PhysicalExamination?.PhysicalexaminationOropharynxObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationAxilasmamas,
                    consultaDto.PhysicalExamination?.PhysicalexaminationAxilasmamasObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationSpine,
                    consultaDto.PhysicalExamination?.PhysicalexaminationSpineObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationIngleperine,
                    consultaDto.PhysicalExamination?.PhysicalexaminationIngleperineObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationUpperlimbs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationUpperlimbsObs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationLowerlimbs,
                    consultaDto.PhysicalExamination?.PhysicalexaminationLowerlimbsObs,


                    // Antecedentes familiares
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundHeartdisease,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundHeartdiseaseObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogHeartdisease,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDiabetes,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDiabetesObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDiabetes,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxcardiovascular,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxcardiovascularObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxcardiovascular,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundHypertension,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundHypertensionObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogHypertension,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundCancer,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundCancerObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogCancer,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundTuberculosis,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundTuberculosisObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshTuberculosis,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxmental,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxmentalObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxmental,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxinfectious,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxinfectiousObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxinfectious,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundMalformation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundMalformationObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogMalformation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundOther,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundOtherObservation,
                    consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogOther,

                    consultaDto.PersonalBackground?.PersonalBackgroundHeartdisease,
                    consultaDto.PersonalBackground?.PersonalBackgroundHeartdiseaseObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundHypertension,
                    consultaDto.PersonalBackground?.PersonalBackgroundHypertensionObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxcardiovascular,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxcardiovascularObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundEndometabolic,
                    consultaDto.PersonalBackground?.PersonalBackgroundEndometabolicObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundCancer,
                    consultaDto.PersonalBackground?.PersonalBackgroundCancerObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundTuberculosis,
                    consultaDto.PersonalBackground?.PersonalBackgroundTuberculosisObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxmental,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxmentalObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxinfectious,
                    consultaDto.PersonalBackground?.PersonalBackgroundDxinfectiousObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundMalformation,
                    consultaDto.PersonalBackground?.PersonalBackgroundMalformationObservation,
                    consultaDto.PersonalBackground?.PersonalBackgroundOther,
                    consultaDto.PersonalBackground?.PersonalBackgroundOtherObservation,

                    // TVPs
                    consultaDto.AllergiesConsultations,
                    consultaDto.SurgeriesConsultations,
                    consultaDto.MedicationsConsultations,
                    consultaDto.LaboratoriesConsultations,
                    consultaDto.ImagesConsultations,
                    consultaDto.DiagnosisConsultations,
                    consultaDto.Procedures
                );

                _logger.LogInformation("Consulta creada exitosamente. ID={ConsultationId}", newId);

                // 2) Devolvemos el ID para que el cliente lo use y no re-inserte
                return Json(new
                {
                    success = true,
                    consultationId = newId,
                    message = "Consulta creada exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la consulta");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocurrió un error en el servidor.",
                    details = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsultationDetails(int consultationId)
        {
            // Obtener los detalles de la consulta
            var consultation = _consultationService.GetConsultationDetails(consultationId);

            // Verificar si la consulta existe
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener el patientId de la consulta
            var patientId = consultation.ConsultationPatient;

            // Obtener los detalles del paciente
            var patient = await _patientService.GetPatientFullByIdAsync(patientId);

            // Si el paciente no existe, devolver una respuesta de "No encontrado"
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            // Obtener datos adicionales para la vista
            var genderTypes = await _selectService.GetGenderTypeAsync();
            var bloodTypes = await _selectService.GetBloodTypeAsync();
            var documentTypes = await _selectService.GetDocumentTypeAsync();
            var civilTypes = await _selectService.GetCivilTypeAsync();
            var professionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync();
            var sureHealthTypes = await _selectService.GetSureHealtTypeAsync();
            var countries = await _selectService.GetAllCountriesAsync();
            var provinces = await _selectService.GetAllProvinceAsync();
            var parents = await _selectService.GetRelationshipTypeAsync();
            var allergies = await _selectService.GetAllergiesTypeAsync();
            var surgeries = await _selectService.GetSurgeriesTypeAsync();
            var familyMember = await _selectService.GetFamiliarTypeAsync();
            var diagnosis = await _selectService.GetAllDiagnosisAsync();
            var medications = await _selectService.GetAllMedicationsAsync();
            var images = await _selectService.GetAllImagesAsync();
            var laboratories = await _selectService.GetAllLaboratoriesAsync();
            var doctor = await _patientService.GetDoctorsByAssistantAsync(usuarioId, perfilId);


            // Crear el ViewModel
            var viewModel = new NewPatientViewModel
            {
                DetailsPatient = patient,
                GenderTypes = genderTypes,
                BloodTypes = bloodTypes,
                DocumentTypes = documentTypes,
                CivilTypes = civilTypes,
                ProfessionalTrainingTypes = professionalTrainingTypes,
                SureHealthTypes = sureHealthTypes,
                Countries = countries,
                Provinces = provinces,
                Parents = parents,
                AllergiesTypes = allergies,
                SurgeriesTypes = surgeries,
                FamilyMember = familyMember,
                Diagnoses = diagnosis,
                Medications = medications,
                Images = images,
                Laboratories = laboratories,
                Consultation = consultation, // Agregar los detalles de la consulta al ViewModel
                Users = doctor
            };

            // Retornar la vista con el modelo
            return View(viewModel);
        }




        [HttpGet]
        public async Task<IActionResult> ConsultationUpdate(int consultationId)
        {
            // Obtener los detalles de la consulta
            var consultation = _consultationService.GetConsultationDetails(consultationId);

            // Verificar si la consulta existe
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener el patientId de la consulta
            var patientId = consultation.ConsultationPatient;

            // Obtener los detalles del paciente
            var patient = await _patientService.GetPatientFullByIdAsync(patientId);

            // Si el paciente no existe, devolver una respuesta de "No encontrado"
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
            // Obtener datos adicionales para la vista
            var genderTypes = await _selectService.GetGenderTypeAsync();
            var bloodTypes = await _selectService.GetBloodTypeAsync();
            var documentTypes = await _selectService.GetDocumentTypeAsync();
            var civilTypes = await _selectService.GetCivilTypeAsync();
            var professionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync();
            var sureHealthTypes = await _selectService.GetSureHealtTypeAsync();
            var countries = await _selectService.GetAllCountriesAsync();
            var provinces = await _selectService.GetAllProvinceAsync();
            var parents = await _selectService.GetRelationshipTypeAsync();
            var allergies = await _selectService.GetAllergiesTypeAsync();
            var surgeries = await _selectService.GetSurgeriesTypeAsync();
            var familyMember = await _selectService.GetFamiliarTypeAsync();
            var diagnosis = await _selectService.GetAllDiagnosisAsync();
            var medications = await _selectService.GetAllMedicationsAsync();
            var images = await _selectService.GetAllImagesAsync();
            var laboratories = await _selectService.GetAllLaboratoriesAsync();
            var doctor = await _patientService.GetDoctorsByAssistantAsync(usuarioId, perfilId);


            // Crear el ViewModel
            var viewModel = new NewPatientViewModel
            {
                DetailsPatient = patient,
                GenderTypes = genderTypes,
                BloodTypes = bloodTypes,
                DocumentTypes = documentTypes,
                CivilTypes = civilTypes,
                ProfessionalTrainingTypes = professionalTrainingTypes,
                SureHealthTypes = sureHealthTypes,
                Countries = countries,
                Provinces = provinces,
                Parents = parents,
                AllergiesTypes = allergies,
                SurgeriesTypes = surgeries,
                FamilyMember = familyMember,
                Diagnoses = diagnosis,
                Medications = medications,
                Images = images,
                Laboratories = laboratories,
                Consultation = consultation, // Agregar los detalles de la consulta al ViewModel
                Users = doctor
            };

            // Retornar la vista con el modelo
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConsultationFollowUp(int patientid)
        {
            // Obtener el paciente por ID
            var patient = await _patientService.GetPatientFullByIdAsync(patientid);

            // Verificar si el paciente existe primero
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener el número de documento del paciente
            var patientDocument = patient.PatientDocumentnumber;

            // Obtener los detalles de la última consulta usando el documento
            var consultation = _consultationService.GetLastConsultationDetails(patientDocument);

            // Verificar si la consulta existe
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener datos adicionales para la vista (paralelizado)
            // Obtener datos adicionales para la vista
            var genderTypes = await _selectService.GetGenderTypeAsync();
            var bloodTypes = await _selectService.GetBloodTypeAsync();
            var documentTypes = await _selectService.GetDocumentTypeAsync();
            var civilTypes = await _selectService.GetCivilTypeAsync();
            var professionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync();
            var sureHealthTypes = await _selectService.GetSureHealtTypeAsync();
            var countries = await _selectService.GetAllCountriesAsync();
            var provinces = await _selectService.GetAllProvinceAsync();
            var parents = await _selectService.GetRelationshipTypeAsync();
            var allergies = await _selectService.GetAllergiesTypeAsync();
            var surgeries = await _selectService.GetSurgeriesTypeAsync();
            var familyMember = await _selectService.GetFamiliarTypeAsync();
            var diagnosis = await _selectService.GetAllDiagnosisAsync();
            var medications = await _selectService.GetAllMedicationsAsync();
            var images = await _selectService.GetAllImagesAsync();
            var laboratories = await _selectService.GetAllLaboratoriesAsync();
            // Crear el ViewModel
            var viewModel = new NewPatientViewModel
            {
                DetailsPatient = patient,
                GenderTypes = genderTypes,
                BloodTypes = bloodTypes,
                DocumentTypes = documentTypes,
                CivilTypes = civilTypes,
                ProfessionalTrainingTypes = professionalTrainingTypes,
                SureHealthTypes = sureHealthTypes,
                Countries = countries,
                Provinces = provinces,
                Parents = parents,
                AllergiesTypes = allergies,
                SurgeriesTypes = surgeries,
                FamilyMember = familyMember,
                Diagnoses = diagnosis,
                Medications = medications,
                Images = images,
                Laboratories = laboratories,
                Consultation = consultation
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.medications_name))
            {
                return Json(new
                {
                    success = false,
                    message = "El nombre del medicamento es obligatorio"
                });
            }

            var result = await _selectService.CreateMedicationAsync(dto);

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }

            return Json(new
            {
                success = false,
                message = result.message
            });
        }
    }

}
