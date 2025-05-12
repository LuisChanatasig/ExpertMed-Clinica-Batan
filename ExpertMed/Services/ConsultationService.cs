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


        public async Task<List<Consultation>> GetConsultationsAsync(int userId, int profileId)
        {
            try
            {
                var consultations = await _dbContext.Consultations
                    .FromSqlRaw("EXEC sp_ListAllConsultation @user_id = {0}, @profile_id = {1}", userId, profileId)
                    .ToListAsync();
                // Cargar relaciones necesarias explícitamente
                foreach (var consultation in consultations)
                {
                    // Esto es útil si necesitas cargar relaciones adicionales como PatientNationalityNavigation o PatientCreationuserNavigation
                    // Usamos 'LoadAsync' solo cuando sea necesario, pero si tienes muchas relaciones o muchos pacientes, puede afectar el rendimiento
                    await _dbContext.Entry(consultation)
                        .Reference(p => p.ConsultationPatientNavigation)
                        .LoadAsync();

                    await _dbContext.Entry(consultation)
                        .Reference(p => p.ConsultationUsercreateNavigation)
                        .LoadAsync();

                    await _dbContext.Entry(consultation)
                        .Reference(p => p.ConsultationSpecialityNavigation)
                        .LoadAsync();
                }
                return consultations;
            }
            catch (Exception ex)
            {
                // Manejo de errores (log, excepción personalizada, etc.)
                throw new ApplicationException("Error al obtener las consultas", ex);
            }
        }



        public async Task CrearConsultaAsync(
         DateTime consultation_creationdate,
         int? consultation_usercreate,
         int consultation_sequential,
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
         // Parámetros para órganos y sistemas
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
         // Parámetros para examen físico
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
         // Parámetros para antecedentes familiares
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
         // Tablas relacionadas
         List<ConsultaAlergiaDTO> allergies_consultation,
         List<ConsultaCirugiaDTO> surgeries_consultation,
         List<ConsultaMedicamentoDTO> medications_consultation,
         List<ConsultaLaboratorioDTO> laboratories_consultation,
         List<ConsultaImagenDTO> images_consutlation,
         List<ConsultaDiagnosticoDTO> diagnosis_consultation)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                using (var command = new SqlCommand("sp_CreateConsultation", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetros de consulta
                    AddSqlParameter(command, "@consultation_creationdate", DateTime.Today);
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

                    // Parámetros de órganos y sistemas
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

                    // Parámetros de examen físico
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

                    // Parámetros de antecedentes familiares
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
                    AddSqlParameter(command, "familiary_background_relatshcatalog_malformation", familiary_background_relatshcatalog_malformation);
                    AddSqlParameter(command, "@familiary_background_other", familiary_background_other);
                    AddSqlParameter(command, "@familiary_background_other_observation", familiary_background_other_observation);
                    AddSqlParameter(command, "@familiary_background_relatshcatalog_other", familiary_background_relatshcatalog_other);

                    // Tablas relacionadas (se inicializan con CreateDataTable)
                    AddSqlParameter(command, "@allergies", CreateDataTable(allergies_consultation));
                    AddSqlParameter(command, "@surgeries", CreateDataTable(surgeries_consultation));
                    AddSqlParameter(command, "@medications", CreateDataTable(medications_consultation));
                    AddSqlParameter(command, "@laboratories", CreateDataTable(laboratories_consultation));
                    AddSqlParameter(command, "@images", CreateDataTable(images_consutlation));
                    AddSqlParameter(command, "@diagnostics", CreateDataTable(diagnosis_consultation));

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
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
            var consulta = new Consulta();

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                connection.Open();

                using (var command = new SqlCommand("sp_GetConsultationDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@consultation_id", consultationId);

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
                            consulta.EstablishmentName = reader.IsDBNull(35) ? null : reader.GetString(35);

                        }

                        // Leer los diagnósticos
                        reader.NextResult();
                        consulta.DiagnosisConsultations = new List<ConsultaDiagnosticoDTO>();
                        while (reader.Read())
                        {
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
