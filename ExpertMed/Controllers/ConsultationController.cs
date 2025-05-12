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
        public async Task<IActionResult> ConsultationList()
        {
            // Obtén el ID del usuario y el perfil desde la sesión
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            var profileId = HttpContext.Session.GetInt32("PerfilId");

            // Obtén las consultas del servicio
            var consultations = await _consultationService.GetConsultationsAsync(userId.Value, profileId.Value);

            // Pasa las consultas a la vista
            return View(consultations);
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
                await _consultationService.CrearConsultaAsync(
                consultaDto.ConsultationCreationdate ?? DateTime.Now, // Add this line to provide a default value
                consultaDto.ConsultationUsercreate,
                consultaDto.ConsultationSequential ?? 0, // Add this line to provide a default value
                consultaDto.ConsultationPatient,
                consultaDto.ConsultationSpeciality ?? 0, // Add this line to provide a default value
                consultaDto.ConsultationHistoryclinic,
                consultaDto.ConsultationReason,
                consultaDto.ConsultationDisease,
                consultaDto.ConsultationFamiliaryname,
                consultaDto.ConsultationWarningsings,
                consultaDto.ConsultationNonpharmacologycal,
                consultaDto.ConsultationFamiliarytype ?? 0, // Add this line to provide a default value
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
                consultaDto.ConsultationTherapies,// Add this line to provide a default value
                consultaDto.ConsultationType ?? 0, // Add this line to provide a default value
                consultaDto.ConsultationStatus ?? 0, // Add this line to provide a default value
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
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundHeartdisease,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundHeartdiseaseObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogHeartdisease ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDiabetes,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDiabetesObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDiabetes ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxcardiovascular,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxcardiovascularObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxcardiovascular ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundHypertension,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundHypertensionObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogHypertension ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundCancer,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundCancerObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogCancer ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundTuberculosis,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundTuberculosisObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshTuberculosis ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxmental,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxmentalObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxmental ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxinfectious,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundDxinfectiousObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogDxinfectious ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundMalformation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundMalformationObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogMalformation ?? null, // Add this line to provide a default value
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundOther,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundOtherObservation,
                consultaDto.FamiliaryBackground?.FamiliaryBackgroundRelatshcatalogOther ?? null, // Add this line to provide a default value
                consultaDto.AllergiesConsultations,
                consultaDto.SurgeriesConsultations,
                consultaDto.MedicationsConsultations,
                consultaDto.LaboratoriesConsultations,
                consultaDto.ImagesConsultations,
                consultaDto.DiagnosisConsultations // Add this line to provide a default value
 );

                _logger.LogInformation("Consulta creada exitosamente.");

                return Json(new { success = true, message = "Consulta creada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la consulta");

                return StatusCode(500, new { success = false, message = "Ocurrió un error en el servidor.", details = ex.Message });
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
    }

}
