using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Numerics;
using System.Text.Json;

namespace ExpertMed.Services
{
    public class PatientService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PatientService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public PatientService(IHttpContextAccessor httpContextAccessor, ILogger<PatientService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Servicio para cargar los medicos
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<User>> GetDoctorsByAssistantAsync(int userId, int userProfile)
        {
            var doctors = new List<User>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                using (var command = new SqlCommand("GetDoctorsByAssistant", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@AssistantUserId", userId);
                    command.Parameters.AddWithValue("@UserProfile", userProfile);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var doctor = new User
                            {
                                UsersId = reader.GetInt32(reader.GetOrdinal("users_id")),
                                UsersNames = reader.IsDBNull(reader.GetOrdinal("users_names")) ? null : reader.GetString(reader.GetOrdinal("users_names")),
                                UsersSurcenames = reader.IsDBNull(reader.GetOrdinal("users_surcenames")) ? null : reader.GetString(reader.GetOrdinal("users_surcenames")),

                                // Agrega aquí los campos adicionales que quieras mapear
                                // Ejemplo:
                                // UserEmail = reader.IsDBNull(reader.GetOrdinal("user_email")) ? null : reader.GetString(reader.GetOrdinal("user_email")),
                            };

                            doctors.Add(doctor);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception($"Error al obtener los médicos para el usuario {userId}: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener los médicos asociados: {ex.Message}", ex);
            }

            return doctors;
        }



        //Método para obtener todos los pacientes, el administrador o perfil 1 tiene todos los pacientes,
        //el perfil 2 tiene solo los pacientes de el
        // Método para obtener todos los pacientes

        public async Task<List<PatientDTO>> GetAllPatientsAsync(int userProfile, int? userId = null)
        {
            try
            {
                if (userId == null)
                    throw new ArgumentException("El ID del usuario no puede ser nulo.", nameof(userId));

                var parameters = new[]
                {
            new SqlParameter("@UserProfile", userProfile),
            new SqlParameter("@UserID", userId.Value)
        };

                var patients = await _dbContext.Set<PatientDTO>()
                    .FromSqlRaw("EXEC sp_ListAllPatients @UserProfile, @UserID", parameters)
                    .ToListAsync();

                return patients;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Error al ejecutar el procedimiento almacenado en la base de datos.");
                throw;
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Error de argumento en el método GetAllPatientsAsync.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los pacientes.");
                throw;
            }
        }


        // Método para crear un nuevo paciente
        public async Task<int> CreatePatientAsync(Patient patient, int? doctorUserId = null)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString))
            using (var command = new SqlCommand("sp_CreatePatient", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros desde el modelo Patient
                command.Parameters.AddWithValue("@patient_creationuser", patient.PatientCreationuser);
                command.Parameters.AddWithValue("@patient_modificationuser", patient.PatientModificationuser);
                command.Parameters.AddWithValue("@patient_documenttype", patient.PatientDocumenttype);
                command.Parameters.AddWithValue("@patient_documentnumber", patient.PatientDocumentnumber);
                command.Parameters.AddWithValue("@patient_firstname", patient.PatientFirstname);
                command.Parameters.AddWithValue("@patient_middlename", patient.PatientMiddlename ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_firstsurname", patient.PatientFirstsurname);
                command.Parameters.AddWithValue("@patient_secondlastname", patient.PatientSecondlastname ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_gender", patient.PatientGender);
                command.Parameters.AddWithValue("@patient_birthdate", patient.PatientBirthdate);
                command.Parameters.AddWithValue("@patient_age", patient.PatientAge);
                command.Parameters.AddWithValue("@patient_bloodtype", patient.PatientBloodtype);
                command.Parameters.AddWithValue("@patient_donor", patient.PatientDonor ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_maritalstatus", patient.PatientMaritalstatus);
                command.Parameters.AddWithValue("@patient_vocational_training", patient.PatientVocationalTraining);
                command.Parameters.AddWithValue("@patient_landline_phone", patient.PatientLandlinePhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_cellular_phone", patient.PatientCellularPhone ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_email", patient.PatientEmail ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_nationality", patient.PatientNationality);
                command.Parameters.AddWithValue("@patient_province", patient.PatientProvince);
                command.Parameters.AddWithValue("@patient_address", patient.PatientAddress);
                command.Parameters.AddWithValue("@patient_ocupation", patient.PatientOcupation ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_company", patient.PatientCompany ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@patient_healt_insurance", patient.PatientHealtInsurance);
                command.Parameters.AddWithValue("@patient_status", patient.PatientStatus);

                // ✅ Nuevo parámetro: código de autorización
                command.Parameters.AddWithValue("@patient_insurance_authorization_code",
                    string.IsNullOrWhiteSpace(patient.PatientInsuranceAuthorizationCode)
                        ? (object)DBNull.Value
                        : patient.PatientInsuranceAuthorizationCode);

                // Parámetro opcional: doctor_userid
                command.Parameters.AddWithValue("@doctor_userid", doctorUserId.HasValue ? doctorUserId.Value : (object)DBNull.Value);

                try
                {
                    await connection.OpenAsync();

                    string jsonResult = null;
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            jsonResult = reader.GetString(0);
                        }
                    }

                    if (string.IsNullOrEmpty(jsonResult))
                    {
                        throw new Exception("Error inesperado: No se obtuvo ningún resultado del procedimiento almacenado.");
                    }

                    using (JsonDocument document = JsonDocument.Parse(jsonResult))
                    {
                        var root = document.RootElement;

                        if (root.TryGetProperty("success", out var success) && success.GetInt32() == 1)
                        {
                            if (root.TryGetProperty("patientId", out var patientId))
                            {
                                return patientId.GetInt32();
                            }
                            else
                            {
                                throw new Exception("El campo 'patientId' no se encuentra en el resultado.");
                            }
                        }
                        else
                        {
                            string errorMessage = root.TryGetProperty("message", out var message)
                                ? message.GetString()
                                : "Error al crear el paciente.";
                            throw new Exception(errorMessage);
                        }
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }
            }
        }


        // Método para activar o desactivar al usuario
        public async Task<(bool success, string message)> DesactiveOrActivePatientAsync(int patientId, int status)
        {
            try
            {
                // Crear la conexión
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    // Crear el comando para ejecutar el SP
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_DesactiveOrActivePatient";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros del procedimiento almacenado
                        command.Parameters.Add(new SqlParameter("@PatientId", SqlDbType.Int) { Value = patientId });
                        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = status });

                        // Ejecutar el comando y obtener la respuesta
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                string success = reader["success"].ToString();
                                string message = reader["message"].ToString();

                                if (success == "true")
                                {
                                    return (true, message);
                                }
                                else
                                {
                                    return (false, message);
                                }
                            }
                            else
                            {
                                return (false, "No se recibió respuesta válida del procedimiento.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar/desactivar el paciente.");
                return (false, "Ocurrió un error al procesar la solicitud.");
            }

        }


        public async Task<int> UpdatePatientAsync(Patient patient, int? doctorUserId = null)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString))
            {
                using (var command = new SqlCommand("sp_UpdatePatient", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Adding parameters from Patient object
                    command.Parameters.AddWithValue("@patient_id", patient.PatientId);
                    command.Parameters.AddWithValue("@patient_modificationuser", patient.PatientModificationuser);
                    command.Parameters.AddWithValue("@patient_documenttype", patient.PatientDocumenttype);
                    command.Parameters.AddWithValue("@patient_documentnumber", patient.PatientDocumentnumber);
                    command.Parameters.AddWithValue("@patient_firstname", patient.PatientFirstname);
                    command.Parameters.AddWithValue("@patient_middlename", patient.PatientMiddlename ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_firstsurname", patient.PatientFirstsurname);
                    command.Parameters.AddWithValue("@patient_secondlastname", patient.PatientSecondlastname ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_gender", patient.PatientGender);
                    command.Parameters.AddWithValue("@patient_birthdate", patient.PatientBirthdate);
                    command.Parameters.AddWithValue("@patient_age", patient.PatientAge);
                    command.Parameters.AddWithValue("@patient_bloodtype", patient.PatientBloodtype);
                    command.Parameters.AddWithValue("@patient_donor", patient.PatientDonor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_maritalstatus", patient.PatientMaritalstatus);
                    command.Parameters.AddWithValue("@patient_vocational_training", patient.PatientVocationalTraining);
                    command.Parameters.AddWithValue("@patient_landline_phone", patient.PatientLandlinePhone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_cellular_phone", patient.PatientCellularPhone ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_email", patient.PatientEmail ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_nationality", patient.PatientNationality);
                    command.Parameters.AddWithValue("@patient_province", patient.PatientProvince);
                    command.Parameters.AddWithValue("@patient_address", patient.PatientAddress);
                    command.Parameters.AddWithValue("@patient_ocupation", patient.PatientOcupation ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_company", patient.PatientCompany ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_health_insurance", patient.PatientHealtInsurance);
                    //command.Parameters.AddWithValue("@patient_code", patient.PatientCode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_insurance_authorization_code", patient.PatientInsuranceAuthorizationCode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@patient_status", patient.PatientStatus);

                    // Si doctor_userid está presente, agregar el parámetro
                    if (doctorUserId.HasValue)
                    {
                        command.Parameters.AddWithValue("@doctor_userid", doctorUserId.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@doctor_userid", DBNull.Value);
                    }

                    try
                    {
                        await connection.OpenAsync();

                        // Execute the stored procedure
                        string jsonResult = null;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                jsonResult = reader.GetString(0);
                            }
                        }

                        if (string.IsNullOrEmpty(jsonResult))
                        {
                            throw new Exception("Error inesperado: No se obtuvo ningún resultado del procedimiento almacenado.");
                        }

                        // Deserialize the JSON result
                        using (JsonDocument document = JsonDocument.Parse(jsonResult))
                        {
                            var root = document.RootElement;

                            // Validate the result
                            if (root.TryGetProperty("success", out var success) && success.GetInt32() == 1)
                            {
                                if (root.TryGetProperty("patientId", out var patientId))
                                {
                                    return patientId.GetInt32();
                                }
                                else
                                {
                                    throw new Exception("El campo 'patientId' no se encuentra en el resultado.");
                                }
                            }
                            else
                            {
                                string errorMessage = root.TryGetProperty("message", out var message)
                                    ? message.GetString()
                                    : "Error al actualizar el paciente.";
                                throw new Exception(errorMessage);
                            }
                        }
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            await connection.CloseAsync();
                        }
                    }
                }
            }
        }


        public async Task<DetailsPatientConsult> GetPatientDataByIdAsync(int patientId)
        {
            DetailsPatientConsult patient = null;
            var doctors = new List<DoctorPatient>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetPatientById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro del ID del paciente
                        command.Parameters.Add(new SqlParameter("@PatientId", SqlDbType.Int) { Value = patientId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Leer los datos del paciente
                            if (await reader.ReadAsync())
                            {
                                patient = new DetailsPatientConsult
                                {
                                    PatientId = reader.GetInt32(reader.GetOrdinal("patient_id")),
                                    PatientCreationdate = reader.GetDateTime(reader.GetOrdinal("patient_creationdate")),
                                    PatientModificationdate = reader.GetDateTime(reader.GetOrdinal("patient_modificationdate")),
                                    PatientCreationuser = reader.GetInt32(reader.GetOrdinal("patient_creationuser")),
                                    PatientModificationuser = reader.GetInt32(reader.GetOrdinal("patient_modificationuser")),
                                    PatientDocumenttype = reader.GetInt32(reader.GetOrdinal("patient_documenttype")),
                                    PatientDocumentnumber = reader.IsDBNull(reader.GetOrdinal("patient_documentnumber")) ? null : reader.GetString(reader.GetOrdinal("patient_documentnumber")),
                                    PatientFirstname = reader.IsDBNull(reader.GetOrdinal("patient_firstname")) ? null : reader.GetString(reader.GetOrdinal("patient_firstname")),
                                    PatientMiddlename = reader.IsDBNull(reader.GetOrdinal("patient_middlename")) ? null : reader.GetString(reader.GetOrdinal("patient_middlename")),
                                    PatientFirstsurname = reader.IsDBNull(reader.GetOrdinal("patient_firstsurname")) ? null : reader.GetString(reader.GetOrdinal("patient_firstsurname")),
                                    PatientSecondlastname = reader.IsDBNull(reader.GetOrdinal("patient_secondlastname")) ? null : reader.GetString(reader.GetOrdinal("patient_secondlastname")),
                                    PatientGender = reader.IsDBNull(reader.GetOrdinal("patient_gender")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_gender")),
                                    PatientBirthdate = reader.IsDBNull(reader.GetOrdinal("patient_birthdate")) ? (DateOnly?)null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("patient_birthdate"))),
                                    PatientAge = reader.GetInt32(reader.GetOrdinal("patient_age")),
                                    PatientBloodtype = reader.IsDBNull(reader.GetOrdinal("patient_bloodtype")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_bloodtype")),
                                    PatientDonor = reader.IsDBNull(reader.GetOrdinal("patient_donor")) ? null : reader.GetString(reader.GetOrdinal("patient_donor")),
                                    PatientMaritalstatus = reader.IsDBNull(reader.GetOrdinal("patient_maritalstatus")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_maritalstatus")),
                                    PatientVocationalTraining = reader.IsDBNull(reader.GetOrdinal("patient_vocational_training")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_vocational_training")),
                                    PatientLandlinePhone = reader.IsDBNull(reader.GetOrdinal("patient_landline_phone")) ? null : reader.GetString(reader.GetOrdinal("patient_landline_phone")),
                                    PatientCellularPhone = reader.IsDBNull(reader.GetOrdinal("patient_cellular_phone")) ? null : reader.GetString(reader.GetOrdinal("patient_cellular_phone")),
                                    PatientEmail = reader.IsDBNull(reader.GetOrdinal("patient_email")) ? null : reader.GetString(reader.GetOrdinal("patient_email")),
                                    PatientNationality = reader.IsDBNull(reader.GetOrdinal("patient_nationality")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_nationality")),
                                    PatientProvince = reader.IsDBNull(reader.GetOrdinal("patient_province")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_province")),
                                    PatientAddress = reader.IsDBNull(reader.GetOrdinal("patient_address")) ? null : reader.GetString(reader.GetOrdinal("patient_address")),
                                    PatientOcupation = reader.IsDBNull(reader.GetOrdinal("patient_ocupation")) ? null : reader.GetString(reader.GetOrdinal("patient_ocupation")),
                                    PatientCompany = reader.IsDBNull(reader.GetOrdinal("patient_company")) ? null : reader.GetString(reader.GetOrdinal("patient_company")),
                                    PatientHealthInsurance = reader.IsDBNull(reader.GetOrdinal("patient_healt_insurance")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_healt_insurance")),
                                    PatientCode = reader.IsDBNull(reader.GetOrdinal("patient_code")) ? null : reader.GetString(reader.GetOrdinal("patient_code")),
                                    PatientInsuranceAuthorizationCode = reader.IsDBNull(reader.GetOrdinal("patient_insurance_authorization_code")) ? null : reader.GetString(reader.GetOrdinal("patient_insurance_authorization_code")),
                                    PatientStatus = reader.GetInt32(reader.GetOrdinal("patient_status"))
                                };
                            }

                            // Leer los médicos asociados al paciente
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var doctor = new DoctorPatient
                                    {
                                        DoctorUserid = reader.GetInt32(reader.GetOrdinal("doctor_userid")),
                                        RelationshipStatus = reader.GetInt32(reader.GetOrdinal("relationship_status"))
                                    };
                                    doctors.Add(doctor);
                                }
                            }
                        }
                    }
                }

                // Asignar la lista de médicos al paciente
                if (patient != null)
                {
                    patient.Doctors = doctors;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching patient data: {ex.Message}");
                // Manejo de errores (puedes agregar más log o re-throw la excepción si es necesario)
            }

            return patient;
        }

        public async Task<Patient> GetPatientDetailsAsync(int patientId)
        {
            Patient patientDetails = null;

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                try
                {
                    // Abrir la conexión
                    await connection.OpenAsync();

                    // Configurar el comando para ejecutar el procedimiento almacenado
                    using (var command = new SqlCommand("sp_GetPatientById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", patientId);

                        // Ejecutar el comando y leer los resultados
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Mapear los datos del paciente
                                patientDetails = new Patient
                                {
                                    PatientId = reader.GetInt32(reader.GetOrdinal("patient_id")),
                                    PatientCreationdate = reader.IsDBNull(reader.GetOrdinal("patient_creationdate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("patient_creationdate")),
                                    PatientModificationdate = reader.IsDBNull(reader.GetOrdinal("patient_modificationdate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("patient_modificationdate")),
                                    PatientCreationuser = reader.IsDBNull(reader.GetOrdinal("patient_creationuser"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_creationuser")),
                                    PatientModificationuser = reader.IsDBNull(reader.GetOrdinal("patient_modificationuser"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_modificationuser")),
                                    PatientDocumenttype = reader.IsDBNull(reader.GetOrdinal("patient_documenttype"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_documenttype")),
                                    PatientDocumentnumber = reader.GetString(reader.GetOrdinal("patient_documentnumber")),
                                    PatientFirstname = reader.GetString(reader.GetOrdinal("patient_firstname")),
                                    PatientMiddlename = reader.IsDBNull(reader.GetOrdinal("patient_middlename"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_middlename")),
                                    PatientFirstsurname = reader.GetString(reader.GetOrdinal("patient_firstsurname")),
                                    PatientSecondlastname = reader.IsDBNull(reader.GetOrdinal("patient_secondlastname"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_secondlastname")),
                                    PatientGender = reader.IsDBNull(reader.GetOrdinal("patient_gender"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_gender")),
                                    PatientBirthdate = reader.IsDBNull(reader.GetOrdinal("patient_birthdate"))
                                        ? (DateOnly?)null
                                        : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("patient_birthdate"))),
                                    PatientAge = reader.IsDBNull(reader.GetOrdinal("patient_age"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_age")),
                                    PatientBloodtype = reader.IsDBNull(reader.GetOrdinal("patient_bloodtype"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_bloodtype")),
                                    PatientDonor = reader.IsDBNull(reader.GetOrdinal("patient_donor"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_donor")),
                                    PatientMaritalstatus = reader.IsDBNull(reader.GetOrdinal("patient_maritalstatus"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_maritalstatus")),
                                    PatientVocationalTraining = reader.IsDBNull(reader.GetOrdinal("patient_vocational_training"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_vocational_training")),
                                    PatientLandlinePhone = reader.IsDBNull(reader.GetOrdinal("patient_landline_phone"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_landline_phone")),
                                    PatientCellularPhone = reader.IsDBNull(reader.GetOrdinal("patient_cellular_phone"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_cellular_phone")),
                                    PatientEmail = reader.GetString(reader.GetOrdinal("patient_email")),
                                    PatientNationality = reader.IsDBNull(reader.GetOrdinal("patient_nationality"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_nationality")),
                                    PatientProvince = reader.IsDBNull(reader.GetOrdinal("patient_province"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_province")),
                                    PatientAddress = reader.IsDBNull(reader.GetOrdinal("patient_address"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_address")),
                                    PatientOcupation = reader.IsDBNull(reader.GetOrdinal("patient_ocupation"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_ocupation")),
                                    PatientCompany = reader.IsDBNull(reader.GetOrdinal("patient_company"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("patient_company")),
                                    PatientHealtInsurance = reader.IsDBNull(reader.GetOrdinal("patient_healt_insurance"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("patient_healt_insurance")),
                                    PatientCode = reader.GetString(reader.GetOrdinal("patient_code")),
                                    PatientInsuranceAuthorizationCode = reader.IsDBNull(reader.GetOrdinal("patient_insurance_authorization_code"))
    ? null
    : reader.GetString(reader.GetOrdinal("patient_insurance_authorization_code")),
                                    PatientStatus = reader.GetInt32(reader.GetOrdinal("patient_status"))
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Manejo de errores, loguear el error si es necesario
                    throw new Exception("Error al obtener los detalles del paciente", ex);
                }
            }

            return patientDetails;
        }


        //Traes todos los daros del paciente 
        public async Task<DetailsPatientConsult> GetPatientFullByIdAsync(int patientId)
        {
            DetailsPatientConsult patient = null;

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetPatientFullData", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro del ID del paciente
                        command.Parameters.Add(new SqlParameter("@PatientId", SqlDbType.Int) { Value = patientId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Mapear los datos del lector a un objeto de DetailsPatientConsult
                                patient = new DetailsPatientConsult
                                {
                                    PatientId = reader.GetInt32(reader.GetOrdinal("patient_id")),
                                    PatientCreationdate = reader.GetDateTime(reader.GetOrdinal("patient_creationdate")),
                                    PatientModificationdate = reader.GetDateTime(reader.GetOrdinal("patient_modificationdate")),
                                    PatientCreationuser = reader.GetInt32(reader.GetOrdinal("patient_creationuser")),
                                    PatientModificationuser = reader.GetInt32(reader.GetOrdinal("patient_modificationuser")),
                                    PatientDocumenttype = reader.GetInt32(reader.GetOrdinal("patient_documenttype")),
                                    PatientDocumentnumber = reader.IsDBNull(reader.GetOrdinal("patient_documentnumber")) ? null : reader.GetString(reader.GetOrdinal("patient_documentnumber")),
                                    PatientFirstname = reader.IsDBNull(reader.GetOrdinal("patient_firstname")) ? null : reader.GetString(reader.GetOrdinal("patient_firstname")),
                                    PatientMiddlename = reader.IsDBNull(reader.GetOrdinal("patient_middlename")) ? null : reader.GetString(reader.GetOrdinal("patient_middlename")),
                                    PatientFirstsurname = reader.IsDBNull(reader.GetOrdinal("patient_firstsurname")) ? null : reader.GetString(reader.GetOrdinal("patient_firstsurname")),
                                    PatientSecondlastname = reader.IsDBNull(reader.GetOrdinal("patient_secondlastname")) ? null : reader.GetString(reader.GetOrdinal("patient_secondlastname")),
                                    PatientGender = reader.IsDBNull(reader.GetOrdinal("patient_gender")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_gender")),
                                    PatientGenderName = reader.IsDBNull(reader.GetOrdinal("patient_gender_name")) ? null : reader.GetString(reader.GetOrdinal("patient_gender_name")),
                                    PatientBirthdate = reader.IsDBNull(reader.GetOrdinal("patient_birthdate")) ? (DateOnly?)null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("patient_birthdate"))),
                                    PatientAge = reader.GetInt32(reader.GetOrdinal("patient_age")),
                                    PatientBloodtype = reader.IsDBNull(reader.GetOrdinal("patient_bloodtype")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_bloodtype")),
                                    PatientBloodtypeName = reader.IsDBNull(reader.GetOrdinal("patient_bloodtype_name")) ? null : reader.GetString(reader.GetOrdinal("patient_bloodtype_name")),
                                    PatientDonor = reader.IsDBNull(reader.GetOrdinal("patient_donor")) ? null : reader.GetString(reader.GetOrdinal("patient_donor")),
                                    PatientMaritalstatus = reader.IsDBNull(reader.GetOrdinal("patient_maritalstatus")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_maritalstatus")),
                                    PatientMaritalstatusName = reader.IsDBNull(reader.GetOrdinal("patient_maritalstatus_name")) ? null : reader.GetString(reader.GetOrdinal("patient_maritalstatus_name")),
                                    PatientVocationalTraining = reader.IsDBNull(reader.GetOrdinal("patient_vocational_training")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_vocational_training")),
                                    PatientVocationalTrainingName = reader.IsDBNull(reader.GetOrdinal("patient_vocational_training_name")) ? null : reader.GetString(reader.GetOrdinal("patient_vocational_training_name")),
                                    PatientLandlinePhone = reader.IsDBNull(reader.GetOrdinal("patient_landline_phone")) ? null : reader.GetString(reader.GetOrdinal("patient_landline_phone")),
                                    PatientCellularPhone = reader.IsDBNull(reader.GetOrdinal("patient_cellular_phone")) ? null : reader.GetString(reader.GetOrdinal("patient_cellular_phone")),
                                    PatientEmail = reader.IsDBNull(reader.GetOrdinal("patient_email")) ? null : reader.GetString(reader.GetOrdinal("patient_email")),
                                    PatientNationality = reader.IsDBNull(reader.GetOrdinal("patient_nationality")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_nationality")),
                                    PatientNationalityName = reader.IsDBNull(reader.GetOrdinal("patient_nationality_name")) ? null : reader.GetString(reader.GetOrdinal("patient_nationality_name")),
                                    PatientProvince = reader.IsDBNull(reader.GetOrdinal("patient_province")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_province")),
                                    PatientProvinceName = reader.IsDBNull(reader.GetOrdinal("patient_province_name")) ? null : reader.GetString(reader.GetOrdinal("patient_province_name")),
                                    PatientAddress = reader.IsDBNull(reader.GetOrdinal("patient_address")) ? null : reader.GetString(reader.GetOrdinal("patient_address")),
                                    PatientOcupation = reader.IsDBNull(reader.GetOrdinal("patient_ocupation")) ? null : reader.GetString(reader.GetOrdinal("patient_ocupation")),
                                    PatientCompany = reader.IsDBNull(reader.GetOrdinal("patient_company")) ? null : reader.GetString(reader.GetOrdinal("patient_company")),
                                    PatientHealthInsurance = reader.IsDBNull(reader.GetOrdinal("patient_healt_insurance")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("patient_healt_insurance")),
                                    PatientHealthInsuranceName = reader.GetString(reader.GetOrdinal("patient_health_insurance_name")),
                                    PatientCode = reader.IsDBNull(reader.GetOrdinal("patient_code")) ? null : reader.GetString(reader.GetOrdinal("patient_code")),
                                    PatientStatus = reader.GetInt32(reader.GetOrdinal("patient_status")),
                                    // Signos vitales (pueden ser null si no hay registro)
                                    Temperature = reader.IsDBNull(reader.GetOrdinal("temperature")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("temperature")),
                                    RespiratoryRate = reader.IsDBNull(reader.GetOrdinal("respiratory_rate")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("respiratory_rate")),
                                    BloodPressureAS = reader.IsDBNull(reader.GetOrdinal("blood_pressureAS")) ? null : reader.GetString(reader.GetOrdinal("blood_pressureAS")),
                                    BloodPressureDIS = reader.IsDBNull(reader.GetOrdinal("blood_pressureDIS")) ? null : reader.GetString(reader.GetOrdinal("blood_pressureDIS")),
                                    Pulse = reader.IsDBNull(reader.GetOrdinal("pulse")) ? null : reader.GetString(reader.GetOrdinal("pulse")),
                                    Weight = reader.IsDBNull(reader.GetOrdinal("weight")) ? null : reader.GetString(reader.GetOrdinal("weight")),
                                    Size = reader.IsDBNull(reader.GetOrdinal("size")) ? null : reader.GetString(reader.GetOrdinal("size")),
                                    VitalCreatedAt = reader.IsDBNull(reader.GetOrdinal("vital_created_at")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("vital_created_at")),
                                    VitalCreatedBy = reader.IsDBNull(reader.GetOrdinal("vital_created_by")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("vital_created_by")),

                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching patient data: {ex.Message}");
                // Manejo de errores (puedes agregar más log o re-throw la excepción si es necesario)
            }

            return patient;
        }

        public async Task<Patient> GetPatientDataByDocumentNumberAsync(string documentNumber)
        {
            Patient patient = null;

            using (SqlConnection conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPatientByCedula", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@cedula", documentNumber);

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            patient = new Patient
                            {
                                PatientId = reader["patient_id"] != DBNull.Value ? Convert.ToInt32(reader["patient_id"]) : 0,
                                PatientCreationdate = reader["patient_creationdate"] != DBNull.Value ? Convert.ToDateTime(reader["patient_creationdate"]) : DateTime.MinValue,
                                PatientModificationdate = reader["patient_modificationdate"] != DBNull.Value ? Convert.ToDateTime(reader["patient_modificationdate"]) : DateTime.MinValue,
                                PatientCreationuser = reader["patient_creationuser"] != DBNull.Value ? Convert.ToInt32(reader["patient_creationuser"]) : (int?)null,
                                PatientModificationuser = reader["patient_modificationuser"] != DBNull.Value ? Convert.ToInt32(reader["patient_modificationuser"]) : (int?)null,
                                PatientDocumenttype = reader["patient_documenttype"] != DBNull.Value ? Convert.ToInt32(reader["patient_documenttype"]) : 0,
                                PatientDocumentnumber = reader["patient_documentnumber"]?.ToString(),
                                PatientFirstname = reader["patient_firstname"]?.ToString(),
                                PatientMiddlename = reader["patient_middlename"]?.ToString(),
                                PatientSecondlastname = reader["patient_secondlastname"]?.ToString(),
                                PatientGender = reader["patient_gender"] != DBNull.Value ? Convert.ToInt32(reader["patient_gender"]) : 0,
                                PatientBirthdate = reader["patient_birthdate"] != DBNull.Value
                            ? DateOnly.FromDateTime(Convert.ToDateTime(reader["patient_birthdate"]))
                            : null,
                                PatientAge = reader["patient_age"] != DBNull.Value ? Convert.ToInt32(reader["patient_age"]) : (int?)null,
                                PatientBloodtype = reader["patient_bloodtype"] != DBNull.Value ? Convert.ToInt32(reader["patient_bloodtype"]) : 0,
                                PatientDonor = reader["patient_donor"]?.ToString(),
                                PatientMaritalstatus = reader["patient_maritalstatus"] != DBNull.Value ? Convert.ToInt32(reader["patient_maritalstatus"]) : 0,
                                PatientVocationalTraining = reader["patient_vocational_training"] != DBNull.Value ? Convert.ToInt32(reader["patient_vocational_training"]) : 0,
                                PatientLandlinePhone = reader["patient_landline_phone"]?.ToString(),
                                PatientEmail = reader["patient_email"]?.ToString(),
                                PatientNationality = reader["patient_nationality"] != DBNull.Value ? Convert.ToInt32(reader["patient_nationality"]) : 0,
                                PatientProvince = reader["patient_province"] != DBNull.Value ? Convert.ToInt32(reader["patient_province"]) : 0,
                                PatientAddress = reader["patient_address"]?.ToString(),
                                PatientOcupation = reader["patient_ocupation"]?.ToString(),
                                PatientCompany = reader["patient_company"]?.ToString(),
                                PatientHealtInsurance = reader["patient_healt_insurance"] != DBNull.Value ? Convert.ToInt32(reader["patient_healt_insurance"]) : 0,
                                PatientCode = reader["patient_code"]?.ToString(),
                                PatientStatus = reader["patient_status"] != DBNull.Value ? Convert.ToInt32(reader["patient_status"]) : 0,
                                PatientCellularPhone = reader["patient_cellular_phone"]?.ToString(),
                                PatientFirstsurname = reader["patient_firstsurname"]?.ToString()
                            };
                        }
                    }
                }
            }

            return patient;
        }


    }
}
