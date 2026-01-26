using ExpertMed.Migrations;
using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class ConsultationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ConsultationService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public ConsultationService(IHttpContextAccessor httpContextAccessor, ILogger<ConsultationService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public async Task<List<ConsultationGroupViewModel>> GetConsultationsAsync(
        int userId,
        int profileId,
        int? patientId = null,
        int page = 1,
        int pageSize = 15)
        {
            try
            {
                // Timeout extendido para evitar excepciones por rendimiento momentáneo
                _dbContext.Database.SetCommandTimeout(120);

                string sqlQuery;
                List<object> parameters = new() { userId, profileId, page, pageSize };

                if (patientId.HasValue)
                {
                    sqlQuery = "EXEC sp_ListAllConsultation @user_id = {0}, @profile_id = {1}, @patient_id = {2}, @page = {3}, @pagesize = {4}";
                    parameters.Insert(2, patientId.Value);
                }
                else
                {
                    sqlQuery = "EXEC sp_ListAllConsultation @user_id = {0}, @profile_id = {1}, @page = {2}, @pagesize = {3}";
                }

                var consultations = await _dbContext.Consultations
                    .FromSqlRaw(sqlQuery, parameters.ToArray())
                    .AsNoTracking()
                    .ToListAsync();

                if (!consultations.Any())
                    return new List<ConsultationGroupViewModel>();

                // IDs necesarios
                var patientIds = consultations.Select(c => c.ConsultationPatient).Distinct().ToList();
                var userIds = consultations.Where(c => c.ConsultationUsercreate.HasValue)
                                           .Select(c => c.ConsultationUsercreate.Value)
                                           .Distinct()
                                           .ToList();
                var specialityIds = consultations.Where(c => c.ConsultationSpeciality.HasValue)
                                                 .Select(c => c.ConsultationSpeciality.Value)
                                                 .Distinct()
                                                 .ToList();

                // Carga diccionarios
                var patients = await _dbContext.Patients
                    .Where(p => patientIds.Contains(p.PatientId))
                    .AsNoTracking()
                    .ToDictionaryAsync(p => p.PatientId);

                var users = await _dbContext.Users
                    .Where(u => userIds.Contains(u.UsersId))
                    .AsNoTracking()
                    .ToDictionaryAsync(u => u.UsersId);

                var specialities = await _dbContext.Specialities
                    .Where(s => specialityIds.Contains(s.SpecialityId))
                    .AsNoTracking()
                    .ToDictionaryAsync(s => s.SpecialityId);

                // Asignación manual
                foreach (var c in consultations)
                {
                    if (patients.TryGetValue(c.ConsultationPatient, out var pt))
                        c.ConsultationPatientNavigation = pt;

                    if (c.ConsultationUsercreate.HasValue &&
                        users.TryGetValue(c.ConsultationUsercreate.Value, out var us))
                        c.ConsultationUsercreateNavigation = us;

                    if (c.ConsultationSpeciality.HasValue &&
                        specialities.TryGetValue(c.ConsultationSpeciality.Value, out var sp))
                        c.ConsultationSpecialityNavigation = sp;
                }

                // Agrupar
                var grouped = consultations
                    .GroupBy(c => new
                    {
                        PacienteId = c.ConsultationPatient,
                        NombreCompleto = BuildPatientFullName(c.ConsultationPatientNavigation)
                    })
                    .Select(g => new ConsultationGroupViewModel
                    {
                        PacienteId = g.Key.PacienteId,
                        PacienteNombre = g.Key.NombreCompleto,
                        Consultas = g.OrderByDescending(c => c.ConsultationCreationdate).ToList(),
                        UltimaConsulta = g.Max(c => c.ConsultationCreationdate),
                        TotalConsultas = g.Count()
                    })
                    .OrderByDescending(g => g.UltimaConsulta)
                    .ToList();

                return grouped;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"Error SQL: {sqlEx.Message}");
                throw new ApplicationException($"Error al ejecutar el SP: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                throw new ApplicationException("Error al obtener consultas", ex);
            }
        }

        // Método auxiliar para construir el nombre completo del paciente
        private string BuildPatientFullName(Patient patient)
        {
            if (patient == null)
                return "Paciente no encontrado";

            var nombres = new List<string>();

            if (!string.IsNullOrWhiteSpace(patient.PatientFirstname))
                nombres.Add(patient.PatientFirstname);

            if (!string.IsNullOrWhiteSpace(patient.PatientMiddlename))
                nombres.Add(patient.PatientMiddlename);

            if (!string.IsNullOrWhiteSpace(patient.PatientFirstsurname))
                nombres.Add(patient.PatientFirstsurname);

            if (!string.IsNullOrWhiteSpace(patient.PatientSecondlastname))
                nombres.Add(patient.PatientSecondlastname);

            return nombres.Any() ? string.Join(" ", nombres) : "Sin nombre";
        }

        public async Task<int> CrearConsultaAsync(
            int? consultationId,
            DateTime consultation_creationdate,
            int? consultation_usercreate,
            int consultation_patient,
            int consultation_speciality,
            string consultation_historyclinic,
            string consultation_reason,
            string consultation_disease,
            string consultation_familiaryname,
            string consultation_warningsings,
            string consultation_nonpharmacologycal,
            int consultation_familiarytype,
            string consultation_familiaryphone,
            string consultation_temperature,
            string consultation_respirationrate,
            string consultation_bloodpressuredAS,
            string consultation_bloodpresuredDIS,
            string consultation_pulse,
            string consultation_weight,
            string consultation_size,
            string consultation_treatmentplan,
            string consultation_observation,
            string consultation_personalbackground,
            int consultation_disablilitydays,
            string consultation_evolution_notes,
            string consultation_therapies,
            int consultation_type,
            int consultation_status,
            bool consultation_hasdisease,
            string consultation_diseaseobservation,
            string consultation_contingencytype,
            bool? consultation_hassymptoms,
            bool consultation_is_final,
    // ==============================
    // NUEVOS CAMPOS CLÍNICOS (2025-11-07)
    // ==============================
    decimal? consultation_imc,
    decimal? consultation_abdominal_perimeter,
    decimal? consultation_capillary_hemoglobin,
    decimal? consultation_capillary_glucose,
    decimal? consultation_spo2,
            // Órganos y sistemas
            bool? organssystems_organsenses,
            string organssystems_organsenses_Obs,
            bool? organssystems_respiratory,
            string organssystems_respiratory_obs,
            bool? organssystems_cardiovascular,
            string organssystems_cardiovascular_obs,
            bool? organssystems_digestive,
            string organssystems_digestive_obs,
            bool? organssystems_genital,
            string organssystems_genital_obs,
            bool? organssystems_urinary,
            string organssystems_urinary_obs,
            bool? organssystems_skeletal_m,
            string organssystems_skeletal_m_obs,
            bool? organssystems_endrocrine,
            string organssystems_endocrine,
            bool? organssystems_lymphatic,
            string organssystems_lymphatic_obs,
            bool? organssystems_nervous,
            string organssystems_nervous_obs,
            bool? organssystems_skin,
            string organssystems_skin_obs,

            // Examen físico
            bool? physicalexamination_head,
            string physicalexamination_head_obs,
            bool? physicalexamination_neck,
            string physicalexamination_neck_obs,
            bool? physicalexamination_chest,
            string physicalexamination_chest_obs,
            bool? physicalexamination_abdomen,
            string physicalexamination_abdomen_obs,
            bool? physicalexamination_pelvis,
            string physicalexamination_pelvis_obs,
            bool? physicalexamination_limbs,
            string physicalexamination_limbs_obs,
            bool? physicalexamination_skinfaneras,
            string physicalexamination_skinfaneras_obs,
            bool? physicalexamination_eyes,
            string physicalexamination_eyes_obs,
            bool? physicalexamination_ears,
            string physicalexamination_ears_obs,
            bool? physicalexamination_nose,
            string physicalexamination_nose_obs,
            bool? physicalexamination_mouth,
            string physicalexamination_mouth_obs,
            bool? physicalexamination_oropharynx,
            string physicalexamination_oropharynx_obs,
            bool? physicalexamination_axilasmamas,
            string physicalexamination_axilasmamas_obs,
            bool? physicalexamination_spine,
            string physicalexamination_spine_obs,
            bool? physicalexamination_ingleperine,
            string physicalexamination_ingleperine_obs,
            bool? physicalexamination_upperlimbs,
            string physicalexamination_upperlimbs_obs,
            bool? physicalexamination_lowerlimbs,
            string physicalexamination_lowerlimbs_obs,

            // Antecedentes familiares
            bool? familiary_background_heartdisease,
            string familiary_background_heartdisease_observation,
            int? familiary_background_relatshcatalog_heartdisease,
            bool? familiary_background_diabetes,
            string familiary_background_diabetes_observation,
            int? familiary_background_relatshcatalog_diabetes,
            bool? familiary_background_dxcardiovascular,
            string familiary_background_dxcardiovascular_observation,
            int? familiary_background_relatshcatalog_dxcardiovascular,
            bool? familiary_background_hypertension,
            string familiary_background_hypertension_observation,
            int? familiary_background_relatshcatalog_hypertension,
            bool? familiary_background_cancer,
            string familiary_background_cancer_observation,
            int? familiary_background_relatshcatalog_cancer,
            bool? familiary_background_tuberculosis,
            string familiary_background_tuberculosis_observation,
            int? familiary_background_relatsh_tuberculosis,
            bool? familiary_background_dxmental,
            string familiary_background_dxmental_observation,
            int? familiary_background_relatshcatalog_dxmental,
            bool? familiary_background_dxinfectious,
            string familiary_background_dxinfectious_observation,
            int? familiary_background_relatshcatalog_dxinfectious,
            bool? familiary_background_malformation,
            string familiary_background_malformation_observation,
            int? familiary_background_relatshcatalog_malformation,
            bool? familiary_background_other,
            string familiary_background_other_observation,
            int? familiary_background_relatshcatalog_other,

            // Antecedentes personales
            bool? personal_background_heartdisease,
            string personal_background_heartdisease_observation,
            bool? personal_background_hypertension,
            string personal_background_hypertension_observation,
            bool? personal_background_dxcardiovascular,
            string personal_background_dxcardiovascular_observation,
            bool? personal_background_endometabolic,
            string personal_background_endometabolic_observation,
            bool? personal_background_cancer,
            string personal_background_cancer_observation,
            bool? personal_background_tuberculosis,
            string personal_background_tuberculosis_observation,
            bool? personal_background_dxmental,
            string personal_background_dxmental_observation,
            bool? personal_background_dxinfectious,
            string personal_background_dxinfectious_observation,
            bool? personal_background_malformation,
            string personal_background_malformation_observation,
            bool? personal_background_other,
            string personal_background_other_observation,

            // TVPs
            List<ConsultaAlergiaDTO> allergies_consultation,
            List<ConsultaCirugiaDTO> surgeries_consultation,
            List<ConsultaMedicamentoDTO> medications_consultation,
            List<ConsultaLaboratorioDTO> laboratories_consultation,
            List<ConsultaImagenDTO> images_consutlation,
            List<ConsultaDiagnosticoDTO> diagnosis_consultation,
            List<ConsultaOtroEstudioDTO> other_studies,
            List<ConsultaProcedimientoDTO> procedures
        )
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var command = new SqlCommand("sp_CreateConsultation", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // OUTPUT
            var idParam = new SqlParameter("@consultation_id", SqlDbType.Int)
            {
                Direction = ParameterDirection.InputOutput,
                Value = consultationId ?? (object)DBNull.Value
            };
            command.Parameters.Add(idParam);

            // === Parámetros principales ===
            AddSqlParameter(command, "@consultation_creationdate", consultation_creationdate);
            AddSqlParameter(command, "@consultation_usercreate", consultation_usercreate);
            AddSqlParameter(command, "@consultation_patient", consultation_patient);
            AddSqlParameter(command, "@consultation_speciality", consultation_speciality);
            AddSqlParameter(command, "@consultation_historyclinic", consultation_historyclinic);
            AddSqlParameter(command, "@consultation_reason", consultation_reason);
            AddSqlParameter(command, "@consultation_disease", consultation_disease);
            AddSqlParameter(command, "@consultation_familiaryname", consultation_familiaryname);
            AddSqlParameter(command, "@consultation_warningsings", consultation_warningsings);
            AddSqlParameter(command, "@consultation_nonpharmacologycal", consultation_nonpharmacologycal);
            AddSqlParameter(command, "@consultation_familiarytype", consultation_familiarytype);
            AddSqlParameter(command, "@consultation_familiaryphone", consultation_familiaryphone);
            AddSqlParameter(command, "@consultation_temperature", consultation_temperature);
            AddSqlParameter(command, "@consultation_respirationrate", consultation_respirationrate);
            AddSqlParameter(command, "@consultation_bloodpressuredAS", consultation_bloodpressuredAS);
            AddSqlParameter(command, "@consultation_bloodpresuredDIS", consultation_bloodpresuredDIS);
            AddSqlParameter(command, "@consultation_pulse", consultation_pulse);
            AddSqlParameter(command, "@consultation_weight", consultation_weight);
            AddSqlParameter(command, "@consultation_size", consultation_size);
            AddSqlParameter(command, "@consultation_treatmentplan", consultation_treatmentplan);
            AddSqlParameter(command, "@consultation_observation", consultation_observation);
            AddSqlParameter(command, "@consultation_personalbackground", consultation_personalbackground);
            AddSqlParameter(command, "@consultation_disablilitydays", consultation_disablilitydays);
            AddSqlParameter(command, "@consultation_evolution_notes", consultation_evolution_notes);
            AddSqlParameter(command, "@consultation_therapies", consultation_therapies);
            AddSqlParameter(command, "@consultation_type", consultation_type);
            AddSqlParameter(command, "@consultation_status", consultation_status);
            AddSqlParameter(command, "@consultation_hasdisease", consultation_hasdisease);
            AddSqlParameter(command, "@consultation_diseaseobservation", consultation_diseaseobservation);
            AddSqlParameter(command, "@consultation_contingencytype", consultation_contingencytype);
            AddSqlParameter(command, "@consultation_hassymptoms", consultation_hassymptoms);
            AddSqlParameter(command, "@consultation_is_final", consultation_is_final);

            // === Nuevos campos clínicos (2025-11-07) ===
            AddSqlParameter(command, "@consultation_imc", consultation_imc);
            AddSqlParameter(command, "@consultation_abdominal_perimeter", consultation_abdominal_perimeter);
            AddSqlParameter(command, "@consultation_capillary_hemoglobin", consultation_capillary_hemoglobin);
            AddSqlParameter(command, "@consultation_capillary_glucose", consultation_capillary_glucose);
            AddSqlParameter(command, "@consultation_spo2", consultation_spo2);
            // === Órganos y sistemas ===
            AddSqlParameter(command, "@organssystems_organsenses", organssystems_organsenses);
            AddSqlParameter(command, "@organssystems_organsenses_Obs", organssystems_organsenses_Obs);
            AddSqlParameter(command, "@organssystems_respiratory", organssystems_respiratory);
            AddSqlParameter(command, "@organssystems_respiratory_obs", organssystems_respiratory_obs);
            AddSqlParameter(command, "@organssystems_cardiovascular", organssystems_cardiovascular);
            AddSqlParameter(command, "@organssystems_cardiovascular_obs", organssystems_cardiovascular_obs);
            AddSqlParameter(command, "@organssystems_digestive", organssystems_digestive);
            AddSqlParameter(command, "@organssystems_digestive_obs", organssystems_digestive_obs);
            AddSqlParameter(command, "@organssystems_genital", organssystems_genital);
            AddSqlParameter(command, "@organssystems_genital_obs", organssystems_genital_obs);
            AddSqlParameter(command, "@organssystems_urinary", organssystems_urinary);
            AddSqlParameter(command, "@organssystems_urinary_obs", organssystems_urinary_obs);
            AddSqlParameter(command, "@organssystems_skeletal_m", organssystems_skeletal_m);
            AddSqlParameter(command, "@organssystems_skeletal_m_obs", organssystems_skeletal_m_obs);
            AddSqlParameter(command, "@organssystems_endrocrine", organssystems_endrocrine);
            AddSqlParameter(command, "@organssystems_endocrine", organssystems_endocrine);
            AddSqlParameter(command, "@organssystems_lymphatic", organssystems_lymphatic);
            AddSqlParameter(command, "@organssystems_lymphatic_obs", organssystems_lymphatic_obs);
            AddSqlParameter(command, "@organssystems_nervous", organssystems_nervous);
            AddSqlParameter(command, "@organssystems_nervous_obs", organssystems_nervous_obs);
            AddSqlParameter(command, "@organssystems_skin", organssystems_skin);
            AddSqlParameter(command, "@organssystems_skin_obs", organssystems_skin_obs);

            // === Examen físico ===
            AddSqlParameter(command, "@physicalexamination_head", physicalexamination_head);
            AddSqlParameter(command, "@physicalexamination_head_obs", physicalexamination_head_obs);
            AddSqlParameter(command, "@physicalexamination_neck", physicalexamination_neck);
            AddSqlParameter(command, "@physicalexamination_neck_obs", physicalexamination_neck_obs);
            AddSqlParameter(command, "@physicalexamination_chest", physicalexamination_chest);
            AddSqlParameter(command, "@physicalexamination_chest_obs", physicalexamination_chest_obs);
            AddSqlParameter(command, "@physicalexamination_abdomen", physicalexamination_abdomen);
            AddSqlParameter(command, "@physicalexamination_abdomen_obs", physicalexamination_abdomen_obs);
            AddSqlParameter(command, "@physicalexamination_pelvis", physicalexamination_pelvis);
            AddSqlParameter(command, "@physicalexamination_pelvis_obs", physicalexamination_pelvis_obs);
            AddSqlParameter(command, "@physicalexamination_limbs", physicalexamination_limbs);
            AddSqlParameter(command, "@physicalexamination_limbs_obs", physicalexamination_limbs_obs);
            AddSqlParameter(command, "@physicalexamination_skinfaneras", physicalexamination_skinfaneras);
            AddSqlParameter(command, "@physicalexamination_skinfaneras_obs", physicalexamination_skinfaneras_obs);
            AddSqlParameter(command, "@physicalexamination_eyes", physicalexamination_eyes);
            AddSqlParameter(command, "@physicalexamination_eyes_obs", physicalexamination_eyes_obs);
            AddSqlParameter(command, "@physicalexamination_ears", physicalexamination_ears);
            AddSqlParameter(command, "@physicalexamination_ears_obs", physicalexamination_ears_obs);
            AddSqlParameter(command, "@physicalexamination_nose", physicalexamination_nose);
            AddSqlParameter(command, "@physicalexamination_nose_obs", physicalexamination_nose_obs);
            AddSqlParameter(command, "@physicalexamination_mouth", physicalexamination_mouth);
            AddSqlParameter(command, "@physicalexamination_mouth_obs", physicalexamination_mouth_obs);
            AddSqlParameter(command, "@physicalexamination_oropharynx", physicalexamination_oropharynx);
            AddSqlParameter(command, "@physicalexamination_oropharynx_obs", physicalexamination_oropharynx_obs);
            AddSqlParameter(command, "@physicalexamination_axilasmamas", physicalexamination_axilasmamas);
            AddSqlParameter(command, "@physicalexamination_axilasmamas_obs", physicalexamination_axilasmamas_obs);
            AddSqlParameter(command, "@physicalexamination_spine", physicalexamination_spine);
            AddSqlParameter(command, "@physicalexamination_spine_obs", physicalexamination_spine_obs);
            AddSqlParameter(command, "@physicalexamination_ingleperine", physicalexamination_ingleperine);
            AddSqlParameter(command, "@physicalexamination_ingleperine_obs", physicalexamination_ingleperine_obs);
            AddSqlParameter(command, "@physicalexamination_upperlimbs", physicalexamination_upperlimbs);
            AddSqlParameter(command, "@physicalexamination_upperlimbs_obs", physicalexamination_upperlimbs_obs);
            AddSqlParameter(command, "@physicalexamination_lowerlimbs", physicalexamination_lowerlimbs);
            AddSqlParameter(command, "@physicalexamination_lowerlimbs_obs", physicalexamination_lowerlimbs_obs);

            // === Antecedentes familiares ===
            AddSqlParameter(command, "@familiary_background_heartdisease", familiary_background_heartdisease);
            AddSqlParameter(command, "@familiary_background_heartdisease_observation", familiary_background_heartdisease_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_heartdisease", familiary_background_relatshcatalog_heartdisease);
            AddSqlParameter(command, "@familiary_background_diabetes", familiary_background_diabetes);
            AddSqlParameter(command, "@familiary_background_diabetes_observation", familiary_background_diabetes_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_diabetes", familiary_background_relatshcatalog_diabetes);
            AddSqlParameter(command, "@familiary_background_dxcardiovascular", familiary_background_dxcardiovascular);
            AddSqlParameter(command, "@familiary_background_dxcardiovascular_observation", familiary_background_dxcardiovascular_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_dxcardiovascular", familiary_background_relatshcatalog_dxcardiovascular);
            AddSqlParameter(command, "@familiary_background_hypertension", familiary_background_hypertension);
            AddSqlParameter(command, "@familiary_background_hypertension_observation", familiary_background_hypertension_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_hypertension", familiary_background_relatshcatalog_hypertension);
            AddSqlParameter(command, "@familiary_background_cancer", familiary_background_cancer);
            AddSqlParameter(command, "@familiary_background_cancer_observation", familiary_background_cancer_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_cancer", familiary_background_relatshcatalog_cancer);
            AddSqlParameter(command, "@familiary_background_tuberculosis", familiary_background_tuberculosis);
            AddSqlParameter(command, "@familiary_background_tuberculosis_observation", familiary_background_tuberculosis_observation);
            AddSqlParameter(command, "@familiary_background_relatsh_tuberculosis", familiary_background_relatsh_tuberculosis);
            AddSqlParameter(command, "@familiary_background_dxmental", familiary_background_dxmental);
            AddSqlParameter(command, "@familiary_background_dxmental_observation", familiary_background_dxmental_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_dxmental", familiary_background_relatshcatalog_dxmental);
            AddSqlParameter(command, "@familiary_background_dxinfectious", familiary_background_dxinfectious);
            AddSqlParameter(command, "@familiary_background_dxinfectious_observation", familiary_background_dxinfectious_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_dxinfectious", familiary_background_relatshcatalog_dxinfectious);
            AddSqlParameter(command, "@familiary_background_malformation", familiary_background_malformation);
            AddSqlParameter(command, "@familiary_background_malformation_observation", familiary_background_malformation_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_malformation", familiary_background_relatshcatalog_malformation);
            AddSqlParameter(command, "@familiary_background_other", familiary_background_other);
            AddSqlParameter(command, "@familiary_background_other_observation", familiary_background_other_observation);
            AddSqlParameter(command, "@familiary_background_relatshcatalog_other", familiary_background_relatshcatalog_other);

            // === Antecedentes personales ===
            AddSqlParameter(command, "@personal_background_heartdisease", personal_background_heartdisease);
            AddSqlParameter(command, "@personal_background_heartdisease_observation", personal_background_heartdisease_observation);
            AddSqlParameter(command, "@personal_background_hypertension", personal_background_hypertension);
            AddSqlParameter(command, "@personal_background_hypertension_observation", personal_background_hypertension_observation);
            AddSqlParameter(command, "@personal_background_dxcardiovascular", personal_background_dxcardiovascular);
            AddSqlParameter(command, "@personal_background_dxcardiovascular_observation", personal_background_dxcardiovascular_observation);
            AddSqlParameter(command, "@personal_background_endometabolic", personal_background_endometabolic);
            AddSqlParameter(command, "@personal_background_endometabolic_observation", personal_background_endometabolic_observation);
            AddSqlParameter(command, "@personal_background_cancer", personal_background_cancer);
            AddSqlParameter(command, "@personal_background_cancer_observation", personal_background_cancer_observation);
            AddSqlParameter(command, "@personal_background_tuberculosis", personal_background_tuberculosis);
            AddSqlParameter(command, "@personal_background_tuberculosis_observation", personal_background_tuberculosis_observation);
            AddSqlParameter(command, "@personal_background_dxmental", personal_background_dxmental);
            AddSqlParameter(command, "@personal_background_dxmental_observation", personal_background_dxmental_observation);
            AddSqlParameter(command, "@personal_background_dxinfectious", personal_background_dxinfectious);
            AddSqlParameter(command, "@personal_background_dxinfectious_observation", personal_background_dxinfectious_observation);
            AddSqlParameter(command, "@personal_background_malformation", personal_background_malformation);
            AddSqlParameter(command, "@personal_background_malformation_observation", personal_background_malformation_observation);
            AddSqlParameter(command, "@personal_background_other", personal_background_other);
            AddSqlParameter(command, "@personal_background_other_observation", personal_background_other_observation);

            // === TVPs ===
            AddSqlParameter(command, "@allergies", CreateDataTable(allergies_consultation));
            AddSqlParameter(command, "@surgeries", CreateDataTable(surgeries_consultation));
            AddSqlParameter(command, "@medications", CreateDataTable(medications_consultation));
            AddSqlParameter(command, "@laboratories", CreateDataTable(laboratories_consultation));
            AddSqlParameter(command, "@images", CreateDataTable(images_consutlation));
            AddSqlParameter(command, "@diagnostics", CreateDataTable(diagnosis_consultation));
            AddSqlParameter(command, "@procedures", CreateDataTable(procedures));
            AddSqlParameter(command, "@other_studies", CreateOtherStudiesDataTable(other_studies));
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return (int)idParam.Value;
        }

        private DataTable CreateOtherStudiesDataTable(List<ConsultaOtroEstudioDTO> studies)
        {
            DataTable table = new DataTable();
            table.Columns.Add("study_name", typeof(string));
            table.Columns.Add("study_indication", typeof(string));

            if (studies != null)
            {
                foreach (var item in studies)
                {
                    table.Rows.Add(item.StudyName, item.StudyIndication);
                }
            }
            return table;
        }
        private void AddSqlParameter(SqlCommand command, string paramName, object value)
        {
            if (value == null)
            {
                command.Parameters.AddWithValue(paramName, DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue(paramName, value);
            }
        }



        private DataTable CreateDataTable<T>(List<T> list)
        {
            var table = new DataTable();
            var properties = typeof(T).GetProperties();

            // Crear columnas en el DataTable basadas en las propiedades de la clase
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Rellenar las filas del DataTable con los valores de los objetos
            foreach (var item in list)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }


        public Consulta GetConsultationDetails(int consultationId)
        {
            var consulta = new Consulta
            {
                DiagnosisConsultations = new List<ConsultaDiagnosticoDTO>(),
                AllergiesConsultations = new List<ConsultaAlergiaDTO>(),
                ImagesConsultations = new List<ConsultaImagenDTO>(),
                LaboratoriesConsultations = new List<ConsultaLaboratorioDTO>(),
                MedicationsConsultations = new List<ConsultaMedicamentoDTO>(),
                Procedures = new List<ConsultaProcedimientoDTO>(),
                SurgeriesConsultations = new List<ConsultaCirugiaDTO>(),
                OtherStudies = new List<ConsultaOtroEstudioDTO>(),
                FamiliaryBackground = new FamiliaryBackground(),
                OrgansSystem = new OrgansSystem(),
                PhysicalExamination = new PhysicalExamination(),
                PersonalBackground = new PersonalBackground()
            };

            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            connection.Open();

            using var command = new SqlCommand("sp_GetConsultationDetails", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@consultation_id", consultationId);

            using var reader = command.ExecuteReader();

            T GetValueOrDefault<T>(string col, T def = default) =>
                reader.IsDBNull(reader.GetOrdinal(col)) ? def : (T)reader.GetValue(reader.GetOrdinal(col));

            string S(string c) => GetValueOrDefault(c, "");
            bool B(string c) => GetValueOrDefault(c, false);
            int I(string c) => GetValueOrDefault(c, 0);

            if (reader.Read())
            {
                consulta.ConsultationId = I("consultation_id");
                consulta.ConsultationCreationdate = GetValueOrDefault<DateTime?>("consultation_creationdate");
                consulta.ConsultationUsercreate = GetValueOrDefault<int?>("consultation_usercreate");
                consulta.ConsultationPatient = I("consultation_patient");
                consulta.ConsultationSpeciality = GetValueOrDefault<int?>("consultation_speciality");
                consulta.ConsultationHistoryclinic = S("consultation_historyclinic");
                consulta.ConsultationSequential = GetValueOrDefault<int?>("consultation_sequential");
                consulta.ConsultationReason = S("consultation_reason");
                consulta.ConsultationDisease = S("consultation_disease");
                consulta.ConsultationFamiliaryname = S("consultation_familiaryname");
                consulta.ConsultationWarningsings = S("consultation_warningsings");
                consulta.ConsultationNonpharmacologycal = S("consultation_nonpharmacologycal");
                consulta.ConsultationFamiliarytype = GetValueOrDefault<int?>("consultation_familiarytype");
                consulta.ConsultationFamiliaryphone = S("consultation_familiaryphone");
                consulta.ConsultationTemperature = S("consultation_temperature");
                consulta.ConsultationRespirationrate = S("consultation_respirationrate");
                consulta.ConsultationBloodpressuredAs = S("consultation_bloodpressuredAS");
                consulta.ConsultationBloodpresuredDis = S("consultation_bloodpresuredDIS");
                consulta.ConsultationPulse = S("consultation_pulse");
                consulta.ConsultationWeight = S("consultation_weight");
                consulta.ConsultationSize = S("consultation_size");

                // **Campos Fisiológicos Adicionales (decimal(5, 2) NULL)**
                consulta.ConsultationImc = GetValueOrDefault<decimal?>("imc");
                consulta.ConsultationAbdominalPerimeter = GetValueOrDefault<decimal?>("abdominal_perimeter");
                consulta.ConsultationCapillaryHemoglobin = GetValueOrDefault<decimal?>("capillary_hemoglobin");
                consulta.ConsultationCapillaryGlucose = GetValueOrDefault<decimal?>("capillary_glucose");
                consulta.ConsultationSpo2 = GetValueOrDefault<decimal?>("spo2");

                consulta.ConsultationTreatmentplan = S("consultation_treatmentplan");
                consulta.ConsultationObservation = S("consultation_observation");
                consulta.ConsultationPersonalbackground = S("consultation_personalbackground");
                consulta.ConsultationDisablilitydays = GetValueOrDefault<int?>("consultation_disablilitydays");
                consulta.ConsultationEvolutionNotes = S("consultation_evolution_notes");
                consulta.ConsultationTherapies = S("consultation_therapies");
                consulta.ConsultationType = GetValueOrDefault<int?>("consultation_type");
                consulta.ConsultationStatus = GetValueOrDefault<int?>("consultation_status");
                consulta.ConsultationHasdisease = GetValueOrDefault<bool?>("consultation_hasdisease");
                consulta.ConsutationHasSymptoms = GetValueOrDefault<bool?>("consultation_hassymptoms");
                consulta.ConsultationDiseaseobservation = S("consultation_diseaseobservation");
                consulta.ConsultationContingencytype = S("consultation_contingencytype");
                consulta.UsersNames = S("users_names");
                consulta.UsersSurcenames = S("users_surcenames");
                consulta.UsersEmail = S("users_email");
                consulta.UsersPhone = S("users_phone");

                if (!reader.IsDBNull(reader.GetOrdinal("users_profilephoto")))
                {
                    var b = (byte[])reader["users_profilephoto"];
                    consulta.UsersProfilephoto = b;
                    consulta.UsersProfilephoto64 = Convert.ToBase64String(b);
                }

                consulta.UsersDocumentNumber = S("users_document_number");
                consulta.SpecialityName = S("speciality_name");
                consulta.EstablishmentName = S("establishment_name");
                consulta.EstablishmentUnicode = S("establishment_unicode");
                consulta.EstablishmentType = I("establishment_type");

                if (!reader.IsDBNull(reader.GetOrdinal("establishment_logo")))
                {
                    var l = (byte[])reader["establishment_logo"];
                    consulta.UsersEstablishmentLogo = l;
                    consulta.UsersEstablishmentLogo64 = Convert.ToBase64String(l);
                }

                consulta.EstablishmentAddress = S("establishment_address");
            }
            reader.NextResult();
            while (reader.Read())
                consulta.DiagnosisConsultations.Add(new ConsultaDiagnosticoDTO
                {
                    DiagnosisDiagnosisid = I("diagnosis_diagnosisid"),
                    DiagnosisObservation = S("diagnosis_observation"),
                    DiagnosisPresumptive = B("diagnosis_presumptive"),
                    DiagnosisDefinitive = B("diagnosis_definitive"),
                    DiagnosisStatus = I("diagnosis_status")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.AllergiesConsultations.Add(new ConsultaAlergiaDTO
                {
                    AllergiesCatalogid = I("allergies_catalogid"),
                    AllergiesObservation = S("allergies_observation"),
                    AllergiesStatus = I("allergies_status")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.ImagesConsultations.Add(new ConsultaImagenDTO
                {
                    ImagesImagesid = I("images_imagesid"),
                    ImagesAmount = S("images_amount"),
                    ImagesObservation = S("images_observation"),
                    ImagesSequential = I("images_sequential"),
                    ImagesStatus = I("images_status")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.LaboratoriesConsultations.Add(new ConsultaLaboratorioDTO
                {
                    LaboratoriesLaboratoriesid = I("laboratories_laboratoriesid"),
                    LaboratoriesAmount = S("laboratories_amount"),
                    LaboratoriesObservation = S("laboratories_observation"),
                    LaboratoriesSequential = I("laboratories_sequential"),
                    LaboratoriesStatus = I("laboratories_status")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.MedicationsConsultations.Add(new ConsultaMedicamentoDTO
                {
                    MedicationsMedicationsid = GetValueOrDefault<int?>("medications_medicationsid"),
                    MedicationsAmount = S("medications_amount"),
                    MedicationsObservation = S("medications_observation"),
                    MedicationsSequential = GetValueOrDefault<int?>("medications_sequential"),
                    MedicationsStatus = GetValueOrDefault<int?>("medications_status")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.Procedures.Add(new ConsultaProcedimientoDTO
                {
                    Procedure_Name = S("procedure_name"),
                    Procedure_Date = GetValueOrDefault<DateTime?>("procedure_date")
                });

            reader.NextResult();
            while (reader.Read())
                consulta.SurgeriesConsultations.Add(new ConsultaCirugiaDTO
                {
                    SurgeriesCatalogid = I("surgeries_catalogid"),
                    SurgeriesObservation = S("surgeries_observation"),
                    SurgeriesStatus = I("surgeries_status")
                });

            reader.NextResult();
            if (reader.Read())
            {
                consulta.FamiliaryBackground = new FamiliaryBackground
                {
                    FamiliaryBackgroundHeartdisease = B("familiary_background_heartdisease"),
                    FamiliaryBackgroundHeartdiseaseObservation = S("familiary_background_heartdisease_observation"),
                    FamiliaryBackgroundRelatshcatalogHeartdisease = I("familiary_background_relatshcatalog_heartdisease"),
                    RelatshHeartdiseaseName = S("relatsh_heartdisease_name"),

                    FamiliaryBackgroundDiabetes = B("familiary_background_diabetes"),
                    FamiliaryBackgroundDiabetesObservation = S("familiary_background_diabetes_observation"),
                    FamiliaryBackgroundRelatshcatalogDiabetes = I("familiary_background_relatshcatalog_diabetes"),
                    RelatshDiabetesName = S("relatsh_diabetes_name"),

                    FamiliaryBackgroundDxcardiovascular = B("familiary_background_dxcardiovascular"),
                    FamiliaryBackgroundDxcardiovascularObservation = S("familiary_background_dxcardiovascular_observation"),
                    FamiliaryBackgroundRelatshcatalogDxcardiovascular = I("familiary_background_relatshcatalog_dxcardiovascular"),
                    RelatshDxcardiovascularName = S("relatsh_dxcardiovascular_name"),

                    FamiliaryBackgroundHypertension = B("familiary_background_hypertension"),
                    FamiliaryBackgroundHypertensionObservation = S("familiary_background_hypertension_observation"),
                    FamiliaryBackgroundRelatshcatalogHypertension = I("familiary_background_relatshcatalog_hypertension"),
                    RelatshHypertensionName = S("relatsh_hypertension_name"),

                    FamiliaryBackgroundCancer = B("familiary_background_cancer"),
                    FamiliaryBackgroundCancerObservation = S("familiary_background_cancer_observation"),
                    FamiliaryBackgroundRelatshcatalogCancer = I("familiary_background_relatshcatalog_cancer"),
                    RelatshCancerName = S("relatsh_cancer_name"),

                    FamiliaryBackgroundTuberculosis = B("familiary_background_tuberculosis"),
                    FamiliaryBackgroundTuberculosisObservation = S("familiary_background_tuberculosis_observation"),
                    FamiliaryBackgroundRelatshTuberculosis = I("familiary_background_relatsh_tuberculosis"),
                    RelatshTuberculosisName = S("relatsh_tuberculosis_name"),

                    FamiliaryBackgroundDxmental = B("familiary_background_dxmental"),
                    FamiliaryBackgroundDxmentalObservation = S("familiary_background_dxmental_observation"),
                    FamiliaryBackgroundRelatshcatalogDxmental = I("familiary_background_relatshcatalog_dxmental"),
                    RelatshDxmentalName = S("relatsh_dxmental_name"),

                    FamiliaryBackgroundDxinfectious = B("familiary_background_dxinfectious"),
                    FamiliaryBackgroundDxinfectiousObservation = S("familiary_background_dxinfectious_observation"),
                    FamiliaryBackgroundRelatshcatalogDxinfectious = I("familiary_background_relatshcatalog_dxinfectious"),
                    RelatshDxinfectiousName = S("relatsh_dxinfectious_name"),

                    FamiliaryBackgroundMalformation = B("familiary_background_malformation"),
                    FamiliaryBackgroundMalformationObservation = S("familiary_background_malformation_observation"),
                    FamiliaryBackgroundRelatshcatalogMalformation = I("familiary_background_relatshcatalog_malformation"),
                    RelatshMalformationName = S("relatsh_malformation_name"),

                    FamiliaryBackgroundOther = B("familiary_background_other"),
                    FamiliaryBackgroundOtherObservation = S("familiary_background_other_observation"),
                    FamiliaryBackgroundRelatshcatalogOther = I("familiary_background_relatshcatalog_other"),
                    RelatshOtherName = S("relatsh_other_name")
                };
            }


            reader.NextResult();
            if (reader.Read())
            {
                consulta.OrgansSystem = new OrgansSystem
                {
                    OrganssystemsOrgansenses = B("organssystems_organsenses"),
                    OrganssystemsOrgansensesObs = S("organssystems_organsenses_Obs"),
                    OrganssystemsRespiratory = B("organssystems_respiratory"),
                    OrganssystemsRespiratoryObs = S("organssystems_respiratory_obs"),
                    OrganssystemsCardiovascular = B("organssystems_cardiovascular"),
                    OrganssystemsCardiovascularObs = S("organssystems_cardiovascular_obs"),
                    OrganssystemsDigestive = B("organssystems_digestive"),
                    OrganssystemsDigestiveObs = S("organssystems_digestive_obs"),
                    OrganssystemsGenital = B("organssystems_genital"),
                    OrganssystemsGenitalObs = S("organssystems_genital_obs"),
                    OrganssystemsUrinary = B("organssystems_urinary"),
                    OrganssystemsUrinaryObs = S("organssystems_urinary_obs"),
                    OrganssystemsSkeletalM = B("organssystems_skeletal_m"),
                    OrganssystemsSkeletalMObs = S("organssystems_skeletal_m_obs"),
                    OrganssystemsEndrocrine = B("organssystems_endrocrine"),
                    OrganssystemsEndocrine = S("organssystems_endocrine"),
                    OrganssystemsLymphatic = B("organssystems_lymphatic"),
                    OrganssystemsLymphaticObs = S("organssystems_lymphatic_obs"),
                    OrganssystemsNervous = B("organssystems_nervous"),
                    OrganssystemsNervousObs = S("organssystems_nervous_obs"),
                    OrganssystemsSkin = B("organssystems_skin"),
                    OrganssystemsSkinObs = S("organssystems_skin_obs")
                };
            }

            reader.NextResult();
            if (reader.Read())
            {
                consulta.PhysicalExamination = new PhysicalExamination
                {
                    PhysicalexaminationHead = B("physicalexamination_head"),
                    PhysicalexaminationHeadObs = S("physicalexamination_head_obs"),
                    PhysicalexaminationNeck = B("physicalexamination_neck"),
                    PhysicalexaminationNeckObs = S("physicalexamination_neck_obs"),
                    PhysicalexaminationChest = B("physicalexamination_chest"),
                    PhysicalexaminationChestObs = S("physicalexamination_chest_obs"),
                    PhysicalexaminationAbdomen = B("physicalexamination_abdomen"),
                    PhysicalexaminationAbdomenObs = S("physicalexamination_abdomen_obs"),
                    PhysicalexaminationPelvis = B("physicalexamination_pelvis"),
                    PhysicalexaminationPelvisObs = S("physicalexamination_pelvis_obs"),
                    PhysicalexaminationLimbs = B("physicalexamination_limbs"),
                    PhysicalexaminationLimbsObs = S("physicalexamination_limbs_obs"),
                    PhysicalexaminationSkinfaneras = B("physicalexamination_skinfaneras"),
                    PhysicalexaminationSkinfanerasObs = S("physicalexamination_skinfaneras_obs"),
                    PhysicalexaminationEyes = B("physicalexamination_eyes"),
                    PhysicalexaminationEyesObs = S("physicalexamination_eyes_obs"),
                    PhysicalexaminationEars = B("physicalexamination_ears"),
                    PhysicalexaminationEarsObs = S("physicalexamination_ears_obs"),
                    PhysicalexaminationNose = B("physicalexamination_nose"),
                    PhysicalexaminationNoseObs = S("physicalexamination_nose_obs"),
                    PhysicalexaminationMouth = B("physicalexamination_mouth"),
                    PhysicalexaminationMouthObs = S("physicalexamination_mouth_obs"),
                    PhysicalexaminationOropharynx = B("physicalexamination_oropharynx"),
                    PhysicalexaminationOropharynxObs = S("physicalexamination_oropharynx_obs"),
                    PhysicalexaminationAxilasmamas = B("physicalexamination_axilasmamas"),
                    PhysicalexaminationAxilasmamasObs = S("physicalexamination_axilasmamas_obs"),
                    PhysicalexaminationSpine = B("physicalexamination_spine"),
                    PhysicalexaminationSpineObs = S("physicalexamination_spine_obs"),
                    PhysicalexaminationIngleperine = B("physicalexamination_ingleperine"),
                    PhysicalexaminationIngleperineObs = S("physicalexamination_ingleperine_obs"),
                    PhysicalexaminationUpperlimbs = B("physicalexamination_upperlimbs"),
                    PhysicalexaminationUpperlimbsObs = S("physicalexamination_upperlimbs_obs"),
                    PhysicalexaminationLowerlimbs = B("physicalexamination_lowerlimbs"),
                    PhysicalexaminationLowerlimbsObs = S("physicalexamination_lowerlimbs_obs")
                };
            }

            reader.NextResult();
            if (reader.Read())
            {
                consulta.PersonalBackground = new PersonalBackground
                {
                    PersonalBackgroundHeartdisease = B("personal_background_heartdisease"),
                    PersonalBackgroundHeartdiseaseObservation = S("personal_background_heartdisease_observation"),
                    PersonalBackgroundHypertension = B("personal_background_hypertension"),
                    PersonalBackgroundHypertensionObservation = S("personal_background_hypertension_observation"),
                    PersonalBackgroundDxcardiovascular = B("personal_background_dxcardiovascular"),
                    PersonalBackgroundDxcardiovascularObservation = S("personal_background_dxcardiovascular_observation"),
                    PersonalBackgroundEndometabolic = B("personal_background_endometabolic"),
                    PersonalBackgroundEndometabolicObservation = S("personal_background_endometabolic_observation"),
                    PersonalBackgroundCancer = B("personal_background_cancer"),
                    PersonalBackgroundCancerObservation = S("personal_background_cancer_observation"),
                    PersonalBackgroundTuberculosis = B("personal_background_tuberculosis"),
                    PersonalBackgroundTuberculosisObservation = S("personal_background_tuberculosis_observation"),
                    PersonalBackgroundDxmental = B("personal_background_dxmental"),
                    PersonalBackgroundDxmentalObservation = S("personal_background_dxmental_observation"),
                    PersonalBackgroundDxinfectious = B("personal_background_dxinfectious"),
                    PersonalBackgroundDxinfectiousObservation = S("personal_background_dxinfectious_observation"),
                    PersonalBackgroundMalformation = B("personal_background_malformation"),
                    PersonalBackgroundMalformationObservation = S("personal_background_malformation_observation"),
                    PersonalBackgroundOther = B("personal_background_other"),
                    PersonalBackgroundOtherObservation = S("personal_background_other_observation")
                };
            }

            reader.NextResult();
            while (reader.Read())
            {
                consulta.OtherStudies.Add(new ConsultaOtroEstudioDTO
                {
                    // Asegúrate de que los nombres coincidan con el SELECT del SP
                    StudyName = S("study_name"),
                    StudyIndication = S("study_indication")
                });
            }
            return consulta;
        }

        public Consulta GetLastConsultationDetails(string historyClinic)
        {
            var consulta = new Consulta();

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                connection.Open();

                using (var command = new SqlCommand("GetLastConsultationByHistoryClinic", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@historyClinic", historyClinic);

                    using (var reader = command.ExecuteReader())
                    {
                        // Leer la consulta principal
                        if (reader.Read())
                        {
                            consulta.ConsultationId = reader.GetInt32(0);
                            consulta.ConsultationCreationdate = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1);
                            consulta.ConsultationUsercreate = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);
                            consulta.ConsultationPatient = reader.GetInt32(3);
                            consulta.ConsultationSpeciality = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
                            consulta.ConsultationHistoryclinic = reader.GetString(5);
                            consulta.ConsultationSequential = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6);
                            consulta.ConsultationReason = reader.IsDBNull(7) ? null : reader.GetString(7);
                            consulta.ConsultationDisease = reader.IsDBNull(8) ? null : reader.GetString(8);
                            consulta.ConsultationFamiliaryname = reader.IsDBNull(9) ? null : reader.GetString(9);
                            consulta.ConsultationWarningsings = reader.IsDBNull(10) ? null : reader.GetString(10);
                            consulta.ConsultationNonpharmacologycal = reader.IsDBNull(11) ? null : reader.GetString(11);
                            consulta.ConsultationFamiliarytype = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12);
                            consulta.ConsultationFamiliaryphone = reader.IsDBNull(13) ? null : reader.GetString(13);
                            consulta.ConsultationTemperature = reader.IsDBNull(14) ? null : reader.GetString(14);
                            consulta.ConsultationRespirationrate = reader.IsDBNull(15) ? null : reader.GetString(15);
                            consulta.ConsultationBloodpressuredAs = reader.IsDBNull(16) ? null : reader.GetString(16);
                            consulta.ConsultationBloodpresuredDis = reader.IsDBNull(17) ? null : reader.GetString(17);
                            consulta.ConsultationPulse = reader.GetString(18);
                            consulta.ConsultationWeight = reader.GetString(19);
                            consulta.ConsultationSize = reader.GetString(20);
                            consulta.ConsultationTreatmentplan = reader.IsDBNull(21) ? null : reader.GetString(21);
                            consulta.ConsultationObservation = reader.IsDBNull(22) ? null : reader.GetString(22);
                            consulta.ConsultationPersonalbackground = reader.IsDBNull(23) ? null : reader.GetString(23);
                            consulta.ConsultationDisablilitydays = reader.IsDBNull(24) ? (int?)null : reader.GetInt32(24);
                            consulta.ConsultationEvolutionNotes = reader.IsDBNull(25) ? null : reader.GetString(25);
                            consulta.ConsultationTherapies = reader.IsDBNull(26) ? null : reader.GetString(26);

                            consulta.ConsultationType = reader.IsDBNull(27) ? (int?)null : reader.GetInt32(27);
                            consulta.ConsultationStatus = reader.IsDBNull(28) ? (int?)null : reader.GetInt32(28);
                            consulta.UsersNames = reader.IsDBNull(29) ? null : reader.GetString(29);
                            consulta.UsersSurcenames = reader.IsDBNull(30) ? null : reader.GetString(30);
                            consulta.UsersEmail = reader.IsDBNull(31) ? null : reader.GetString(31);
                            consulta.UsersPhone = reader.IsDBNull(32) ? null : reader.GetString(32);


                            // Leer la imagen de perfil (columna varbinary en índice 33)
                            if (!reader.IsDBNull(33))
                            {
                                // Opción A: Asignarla directamente como byte[]
                                byte[] profilePhotoBytes = (byte[])reader[33];
                                consulta.UsersProfilephoto = profilePhotoBytes;

                                // Opción B: Convertir a Base64 para insertar en el src de un <img>
                                consulta.UsersProfilephoto64 = Convert.ToBase64String(profilePhotoBytes);
                            }
                            else
                            {
                                consulta.UsersProfilephoto = null;
                                consulta.UsersProfilephoto64 = null;
                            }

                            // La especialidad se encuentra en el índice 34
                            consulta.SpecialityName = reader.IsDBNull(34) ? null : reader.GetString(34);
                        }

                        // Leer los diagnósticos
                        reader.NextResult();
                        consulta.DiagnosisConsultations = new List<ConsultaDiagnosticoDTO>();

                        while (reader.Read())
                        {
                            Console.WriteLine($"Columna 1 (DiagnosisDiagnosisid): {reader[1]}");
                            Console.WriteLine($"Columna 2 (DiagnosisObservation): {reader[2]}");
                            Console.WriteLine($"Columna 3 (DiagnosisPresumptive): {reader[3]}");
                            Console.WriteLine($"Columna 4 (DiagnosisDefinitive): {reader[4]}");
                            Console.WriteLine($"Columna 6 (DiagnosisStatus): {reader[6]}");

                            consulta.DiagnosisConsultations.Add(new ConsultaDiagnosticoDTO
                            {
                                DiagnosisDiagnosisid = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                DiagnosisObservation = reader.IsDBNull(2) ? null : reader.GetString(2),
                                DiagnosisPresumptive = reader.IsDBNull(3) ? (bool?)null : reader.GetBoolean(3),
                                DiagnosisDefinitive = reader.IsDBNull(4) ? (bool?)null : reader.GetBoolean(4),
                                DiagnosisStatus = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6)
                            });
                        }

                        // Leer las alergias
                        reader.NextResult();
                        consulta.AllergiesConsultations = new List<ConsultaAlergiaDTO>();
                        while (reader.Read())
                        {
                            consulta.AllergiesConsultations.Add(new ConsultaAlergiaDTO
                            {
                                AllergiesCatalogid = reader.GetInt32(1),
                                AllergiesObservation = reader.IsDBNull(2) ? null : reader.GetString(2),
                                AllergiesStatus = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                            });
                        }

                        // Leer las imágenes
                        reader.NextResult();
                        consulta.ImagesConsultations = new List<ConsultaImagenDTO>();
                        while (reader.Read())
                        {
                            consulta.ImagesConsultations.Add(new ConsultaImagenDTO
                            {
                                ImagesImagesid = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                ImagesAmount = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ImagesObservation = reader.IsDBNull(3) ? null : reader.GetString(3),
                                ImagesSequential = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                ImagesStatus = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
                            });
                        }

                        // Leer los laboratorios
                        reader.NextResult();
                        consulta.LaboratoriesConsultations = new List<ConsultaLaboratorioDTO>();
                        while (reader.Read())
                        {
                            consulta.LaboratoriesConsultations.Add(new ConsultaLaboratorioDTO
                            {
                                LaboratoriesLaboratoriesid = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                LaboratoriesAmount = reader.IsDBNull(2) ? null : reader.GetString(2),
                                LaboratoriesObservation = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LaboratoriesSequential = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                LaboratoriesStatus = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
                            });
                        }

                        // Leer los medicamentos
                        reader.NextResult();
                        consulta.MedicationsConsultations = new List<ConsultaMedicamentoDTO>();
                        while (reader.Read())
                        {
                            consulta.MedicationsConsultations.Add(new ConsultaMedicamentoDTO
                            {
                                MedicationsMedicationsid = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                MedicationsAmount = reader.IsDBNull(2) ? null : reader.GetString(2),
                                MedicationsObservation = reader.IsDBNull(3) ? null : reader.GetString(3),
                                MedicationsSequential = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                MedicationsStatus = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5)
                            });
                        }

                        // Leer las cirugías
                        reader.NextResult();
                        consulta.SurgeriesConsultations = new List<ConsultaCirugiaDTO>();
                        while (reader.Read())
                        {
                            consulta.SurgeriesConsultations.Add(new ConsultaCirugiaDTO
                            {
                                SurgeriesCatalogid = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                SurgeriesObservation = reader.IsDBNull(3) ? null : reader.GetString(3),
                                SurgeriesStatus = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4)
                            });
                        }

                        // Leer los antecedentes familiares
                        reader.NextResult();
                        if (reader.Read())
                        {
                            try
                            {
                                // Imprimir valores crudos desde la base de datos para depuración
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.WriteLine($"Columna {i} ({reader.GetName(i)}): {reader.GetValue(i)}");
                                }

                                consulta.FamiliaryBackground = new FamiliaryBackground
                                {
                                    // Mapea las propiedades de FamiliaryBackground
                                    FamiliaryBackgroundHeartdisease = reader.IsDBNull(0) ? false : Convert.ToBoolean(reader.GetValue(0)),
                                    FamiliaryBackgroundHeartdiseaseObservation = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    FamiliaryBackgroundRelatshcatalogHeartdisease = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),

                                    FamiliaryBackgroundDiabetes = reader.IsDBNull(3) ? false : Convert.ToBoolean(reader.GetValue(3)),
                                    FamiliaryBackgroundDiabetesObservation = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    FamiliaryBackgroundRelatshcatalogDiabetes = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),

                                    FamiliaryBackgroundDxcardiovascular = reader.IsDBNull(6) ? false : Convert.ToBoolean(reader.GetValue(6)),
                                    FamiliaryBackgroundDxcardiovascularObservation = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    FamiliaryBackgroundRelatshcatalogDxcardiovascular = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),

                                    FamiliaryBackgroundHypertension = reader.IsDBNull(9) ? false : Convert.ToBoolean(reader.GetValue(9)),
                                    FamiliaryBackgroundHypertensionObservation = reader.IsDBNull(10) ? null : reader.GetString(10),
                                    FamiliaryBackgroundRelatshcatalogHypertension = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),

                                    FamiliaryBackgroundCancer = reader.IsDBNull(12) ? false : Convert.ToBoolean(reader.GetValue(12)),
                                    FamiliaryBackgroundCancerObservation = reader.IsDBNull(13) ? null : reader.GetString(13),
                                    FamiliaryBackgroundRelatshcatalogCancer = reader.IsDBNull(14) ? (int?)null : reader.GetInt32(14),

                                    FamiliaryBackgroundTuberculosis = reader.IsDBNull(15) ? false : Convert.ToBoolean(reader.GetValue(15)),
                                    FamiliaryBackgroundTuberculosisObservation = reader.IsDBNull(16) ? null : reader.GetString(16),
                                    FamiliaryBackgroundRelatshTuberculosis = reader.IsDBNull(17) ? (int?)null : reader.GetInt32(17),

                                    FamiliaryBackgroundDxmental = reader.IsDBNull(18) ? false : Convert.ToBoolean(reader.GetValue(18)),
                                    FamiliaryBackgroundDxmentalObservation = reader.IsDBNull(19) ? null : reader.GetString(19),
                                    FamiliaryBackgroundRelatshcatalogDxmental = reader.IsDBNull(20) ? (int?)null : reader.GetInt32(20),

                                    FamiliaryBackgroundDxinfectious = reader.IsDBNull(21) ? false : Convert.ToBoolean(reader.GetValue(21)),
                                    FamiliaryBackgroundDxinfectiousObservation = reader.IsDBNull(22) ? null : reader.GetString(22),
                                    FamiliaryBackgroundRelatshcatalogDxinfectious = reader.IsDBNull(23) ? (int?)null : reader.GetInt32(23),

                                    FamiliaryBackgroundMalformation = reader.IsDBNull(24) ? false : Convert.ToBoolean(reader.GetValue(24)),
                                    FamiliaryBackgroundMalformationObservation = reader.IsDBNull(25) ? null : reader.GetString(25),
                                    FamiliaryBackgroundRelatshcatalogMalformation = reader.IsDBNull(26) ? (int?)null : reader.GetInt32(26),

                                    FamiliaryBackgroundOther = reader.IsDBNull(27) ? false : Convert.ToBoolean(reader.GetValue(27)),
                                    FamiliaryBackgroundOtherObservation = reader.IsDBNull(28) ? null : reader.GetString(28),
                                    FamiliaryBackgroundRelatshcatalogOther = reader.IsDBNull(29) ? (int?)null : reader.GetInt32(29),
                                };
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al mapear FamiliaryBackground: {ex.Message}");
                            }
                        }

                        // Leer los sistemas de órganos
                        reader.NextResult();
                        if (reader.Read())
                        {

                            try
                            {
                                // Imprimir valores crudos desde la base de datos para depuración
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.WriteLine($"Columna {i} ({reader.GetName(i)}): {reader.GetValue(i)}");
                                }

                                consulta.OrgansSystem = new OrgansSystem
                                {

                                    OrganssystemsOrgansenses = reader.IsDBNull(0) ? false : Convert.ToBoolean(reader.GetValue(0)),
                                    OrganssystemsOrgansensesObs = reader.IsDBNull(1) ? null : reader.GetString(1),

                                    OrganssystemsRespiratory = reader.IsDBNull(2) ? false : Convert.ToBoolean(reader.GetValue(2)),
                                    OrganssystemsRespiratoryObs = reader.IsDBNull(3) ? null : reader.GetString(3),

                                    OrganssystemsCardiovascular = reader.IsDBNull(4) ? false : Convert.ToBoolean(reader.GetValue(4)),
                                    OrganssystemsCardiovascularObs = reader.IsDBNull(5) ? null : reader.GetString(5),

                                    OrganssystemsDigestive = reader.IsDBNull(6) ? false : Convert.ToBoolean(reader.GetValue(6)),
                                    OrganssystemsDigestiveObs = reader.IsDBNull(7) ? null : reader.GetString(7),

                                    OrganssystemsGenital = reader.IsDBNull(8) ? false : Convert.ToBoolean(reader.GetValue(8)),
                                    OrganssystemsGenitalObs = reader.IsDBNull(9) ? null : reader.GetString(9),

                                    OrganssystemsUrinary = reader.IsDBNull(10) ? false : Convert.ToBoolean(reader.GetValue(10)),
                                    OrganssystemsUrinaryObs = reader.IsDBNull(11) ? null : reader.GetString(11),

                                    OrganssystemsSkeletalM = reader.IsDBNull(12) ? false : Convert.ToBoolean(reader.GetValue(12)),
                                    OrganssystemsSkeletalMObs = reader.IsDBNull(13) ? null : reader.GetString(13),

                                    OrganssystemsEndrocrine = reader.IsDBNull(14) ? false : Convert.ToBoolean(reader.GetValue(14)),
                                    OrganssystemsEndocrine = reader.IsDBNull(15) ? null : reader.GetString(15),

                                    OrganssystemsLymphatic = reader.IsDBNull(16) ? false : Convert.ToBoolean(reader.GetValue(16)),
                                    OrganssystemsLymphaticObs = reader.IsDBNull(17) ? null : reader.GetString(17),

                                    OrganssystemsNervous = reader.IsDBNull(18) ? false : Convert.ToBoolean(reader.GetValue(18)),
                                    OrganssystemsNervousObs = reader.IsDBNull(19) ? null : reader.GetString(19),

                                };

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al mapear OrgansSystems: {ex.Message}");
                            }
                        }
                        // Leer el examen físico
                        reader.NextResult();
                        if (reader.Read())
                        {

                            try
                            {
                                // Imprimir valores crudos desde la base de datos para depuración
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.WriteLine($"Columna {i} ({reader.GetName(i)}): {reader.GetValue(i)}");
                                }
                                consulta.PhysicalExamination = new PhysicalExamination
                                {
                                    // Mapea las propiedades de PhysicalExamination
                                    PhysicalexaminationHead = reader.IsDBNull(0) ? false : Convert.ToBoolean(reader.GetValue(0)),
                                    PhysicalexaminationHeadObs = reader.IsDBNull(1) ? null : reader.GetString(1),

                                    PhysicalexaminationNeck = reader.IsDBNull(2) ? false : Convert.ToBoolean(reader.GetValue(2)),
                                    PhysicalexaminationNeckObs = reader.IsDBNull(3) ? null : reader.GetString(3),

                                    PhysicalexaminationChest = reader.IsDBNull(4) ? false : Convert.ToBoolean(reader.GetValue(4)),
                                    PhysicalexaminationChestObs = reader.IsDBNull(5) ? null : reader.GetString(5),

                                    PhysicalexaminationAbdomen = reader.IsDBNull(6) ? false : Convert.ToBoolean(reader.GetValue(6)),
                                    PhysicalexaminationAbdomenObs = reader.IsDBNull(7) ? null : reader.GetString(7),

                                    PhysicalexaminationPelvis = reader.IsDBNull(8) ? false : Convert.ToBoolean(reader.GetValue(8)),
                                    PhysicalexaminationPelvisObs = reader.IsDBNull(9) ? null : reader.GetString(9),

                                    PhysicalexaminationLimbs = reader.IsDBNull(10) ? false : Convert.ToBoolean(reader.GetValue(10)),
                                    PhysicalexaminationLimbsObs = reader.IsDBNull(11) ? null : reader.GetString(11),
                                };

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al mapear Physical: {ex.Message}");
                            }

                        }
                    }
                }
            }

            return consulta;
        }

    }
}
