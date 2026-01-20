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
   /// Asynchronously retrieves a list of doctors associated with the specified assistant user.
   /// </summary>
   /// <param name="userId">The unique identifier of the assistant user whose associated doctors are to be retrieved.</param>
   /// <param name="userProfile">The profile type of the assistant user. This value is used to filter the doctors returned by the query.</param>
   /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="User"/> objects
   /// representing the doctors associated with the specified assistant. If no doctors are found, the list will be
   /// empty.</returns>
   /// <exception cref="Exception">Thrown when an error occurs while retrieving the doctors from the database.</exception>
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
                                SpecialityName = reader.IsDBNull(reader.GetOrdinal("speciality_name")) ? null : reader.GetString(reader.GetOrdinal("speciality_name"))
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


        /// <summary>
        /// Asynchronously retrieves a list of patients accessible to the specified user profile and user ID.
        /// </summary>
        /// <param name="userProfile">The profile type of the user requesting patient data. Determines the scope of patients returned based on
        /// user permissions.</param>
        /// <param name="userId">The unique identifier of the user whose accessible patients are to be retrieved. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of PatientDTO objects
        /// representing the patients accessible to the specified user.</returns>

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


        /// <summary>
        /// Creates a new patient record in the database asynchronously and returns the unique identifier of the created
        /// patient.
        /// </summary>
        /// <remarks>This method executes the 'sp_CreatePatient' stored procedure to insert a new patient
        /// record. The operation is performed asynchronously and requires a valid database connection. All mandatory
        /// patient fields must be provided in the <paramref name="patient"/> parameter. If the creation fails, an
        /// exception is thrown containing the error message returned by the stored procedure.</remarks>
        /// <param name="patient">The patient information to be stored. All required patient fields must be populated; optional fields may be
        /// null.</param>
        /// <param name="doctorUserId">The user ID of the doctor associated with the patient, if applicable. If not specified, the patient will be
        /// created without a doctor association.</param>
        /// <returns>The unique identifier of the newly created patient.</returns>
        /// <exception cref="Exception">Thrown if the stored procedure does not return a result, if the result does not contain a patient ID, or if
        /// an error occurs during patient creation.</exception>
        public async Task<PatientCreateResponse> CreatePatientAsync(
            Patient patient,
            int? doctorUserId = null,
            bool skipSignatureInsert = false)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            var cs = _dbContext.Database.GetDbConnection().ConnectionString;
            await using var connection = new SqlConnection(cs);
            await using var command = new SqlCommand("dbo.sp_CreatePatient", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ------------------------------------------------------------------
            // Auditoría y Perfil (obligatorio)
            // ------------------------------------------------------------------
            command.Parameters.AddWithValue("@patient_creationuser", patient.PatientCreationuser);
            command.Parameters.AddWithValue("@creationuser_profileid", patient.CreationUserProfileId);
            command.Parameters.AddWithValue("@patient_modificationuser", patient.PatientModificationuser);

            // doctor_userid (opcional)
            command.Parameters.AddWithValue("@doctor_userid",
                doctorUserId.HasValue ? doctorUserId.Value : (object)DBNull.Value);

            // ------------------------------------------------------------------
            // Datos obligatorios
            // ------------------------------------------------------------------
            command.Parameters.AddWithValue("@patient_firstname", patient.PatientFirstname ?? "");
            command.Parameters.AddWithValue("@patient_firstsurname", patient.PatientFirstsurname ?? "");
            command.Parameters.AddWithValue("@patient_cellular_phone",
                string.IsNullOrWhiteSpace(patient.PatientCellularPhone) ? (object)DBNull.Value : patient.PatientCellularPhone);

            // ------------------------------------------------------------------
            // Firma (directa vs QR)
            // - Directa: se manda firma para que SP inserte en patient_signatures.
            // - QR: se manda NULL y luego se consume token con sp_signature_consume_to_patient.
            // ------------------------------------------------------------------
            if (skipSignatureInsert)
            {
                command.Parameters.AddWithValue("@patient_signature", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@patient_signature",
                    string.IsNullOrWhiteSpace(patient.PatientSignature) ? (object)DBNull.Value : patient.PatientSignature);
            }

            // IP para trazabilidad
            var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            command.Parameters.AddWithValue("@ip_address", string.IsNullOrWhiteSpace(remoteIpAddress) ? "Unknown" : remoteIpAddress);

            // ------------------------------------------------------------------
            // Datos opcionales / defaults (según tu SP)
            // ------------------------------------------------------------------
            command.Parameters.AddWithValue("@patient_documenttype", patient.PatientDocumenttype == 0 ? (object)DBNull.Value : patient.PatientDocumenttype);
            command.Parameters.AddWithValue("@patient_documentnumber", patient.PatientDocumentnumber ?? "0000000000");

            command.Parameters.AddWithValue("@patient_middlename", string.IsNullOrWhiteSpace(patient.PatientMiddlename) ? (object)DBNull.Value : patient.PatientMiddlename);
            command.Parameters.AddWithValue("@patient_secondlastname", string.IsNullOrWhiteSpace(patient.PatientSecondlastname) ? (object)DBNull.Value : patient.PatientSecondlastname);

            command.Parameters.AddWithValue("@patient_gender", patient.PatientGender == 0 ? (object)DBNull.Value : patient.PatientGender);
            command.Parameters.AddWithValue("@patient_birthdate", patient.PatientBirthdate == default ? (object)DBNull.Value : patient.PatientBirthdate);
            command.Parameters.AddWithValue("@patient_age", patient.PatientAge == 0 ? (object)DBNull.Value : patient.PatientAge);

            command.Parameters.AddWithValue("@patient_bloodtype", patient.PatientBloodtype == 0 ? (object)DBNull.Value : patient.PatientBloodtype);
            command.Parameters.AddWithValue("@patient_donor", string.IsNullOrWhiteSpace(patient.PatientDonor) ? (object)DBNull.Value : patient.PatientDonor);

            command.Parameters.AddWithValue("@patient_maritalstatus", patient.PatientMaritalstatus == 0 ? (object)DBNull.Value : patient.PatientMaritalstatus);
            command.Parameters.AddWithValue("@patient_vocational_training", patient.PatientVocationalTraining == 0 ? (object)DBNull.Value : patient.PatientVocationalTraining);

            command.Parameters.AddWithValue("@patient_landline_phone", string.IsNullOrWhiteSpace(patient.PatientLandlinePhone) ? (object)DBNull.Value : patient.PatientLandlinePhone);
            command.Parameters.AddWithValue("@patient_email", string.IsNullOrWhiteSpace(patient.PatientEmail) ? (object)DBNull.Value : patient.PatientEmail);

            command.Parameters.AddWithValue("@patient_nationality", patient.PatientNationality == 0 ? (object)DBNull.Value : patient.PatientNationality);
            command.Parameters.AddWithValue("@patient_province", patient.PatientProvince == 0 ? (object)DBNull.Value : patient.PatientProvince);

            command.Parameters.AddWithValue("@patient_address", string.IsNullOrWhiteSpace(patient.PatientAddress) ? "S/N" : patient.PatientAddress);
            command.Parameters.AddWithValue("@patient_ocupation", string.IsNullOrWhiteSpace(patient.PatientOcupation) ? (object)DBNull.Value : patient.PatientOcupation);
            command.Parameters.AddWithValue("@patient_company", string.IsNullOrWhiteSpace(patient.PatientCompany) ? (object)DBNull.Value : patient.PatientCompany);

            command.Parameters.AddWithValue("@patient_status", 1);

            // ------------------------------------------------------------------
            // Execute + parse JSON result
            // ------------------------------------------------------------------
            try
            {
                await connection.OpenAsync();

                string? jsonResult = null;
                await using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (await reader.ReadAsync())
                    {
                        jsonResult = reader.GetString(0);
                    }
                }

                if (string.IsNullOrWhiteSpace(jsonResult))
                    throw new Exception("Error: No se recibió respuesta del servidor de datos.");

                using var document = JsonDocument.Parse(jsonResult);
                var root = document.RootElement;

                var isSuccess = root.TryGetProperty("success", out var successProp) && successProp.GetInt32() == 1;

                if (!isSuccess)
                {
                    var msg = root.TryGetProperty("message", out var messageProp)
                        ? messageProp.GetString()
                        : "Falla en el registro.";
                    throw new Exception(msg);
                }

                return new PatientCreateResponse
                {
                    Success = true,
                    Message = root.TryGetProperty("message", out var m) ? m.GetString() : "Éxito",
                    PatientId = root.TryGetProperty("patientId", out var id) ? id.GetInt32() : 0,
                    PatientCode = root.TryGetProperty("patientCode", out var pc) ? pc.GetString() : null,
                    SecurityToken = root.TryGetProperty("securityToken", out var st) ? st.GetString() : "",
                    SignatureData = root.TryGetProperty("signatureData", out var sd) ? sd.GetString() : null,
                    SignedAt = root.TryGetProperty("signedAt", out var sa) ? sa.GetString() : null
                };
            }
            catch (SqlException ex)
            {
                // Aquí entran también los RAISERROR del SP
                throw new Exception("Error de Base de Datos: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }

        /// <summary>
        /// Activates or deactivates a patient record asynchronously based on the specified status.
        /// </summary>
        /// <remarks>This method executes a stored procedure to update the patient's active status in the
        /// database. The operation is performed asynchronously. If the procedure does not return a valid response or an
        /// error occurs, the method returns a failure result with an appropriate message.</remarks>
        /// <param name="patientId">The unique identifier of the patient whose status will be updated.</param>
        /// <param name="status">The status to apply to the patient. Use 1 to activate or 0 to deactivate the patient.</param>
        /// <returns>A tuple containing a Boolean value indicating whether the operation was successful, and a message describing
        /// the result.</returns>
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

        /// <summary>
        /// Asynchronously updates the details of an existing patient in the database using the provided patient
        /// information.
        /// </summary>
        /// <remarks>This method executes a stored procedure to update patient data. The operation is
        /// performed asynchronously and requires a valid database connection. Ensure that the patient object contains
        /// all necessary information before calling this method.</remarks>
        /// <param name="patient">The patient entity containing updated information to be saved. All required fields must be populated; cannot
        /// be null.</param>
        /// <param name="doctorUserId">The user ID of the doctor associated with the update operation. If null, no doctor will be linked to the
        /// update.</param>
        /// <returns>The unique identifier of the updated patient if the operation succeeds.</returns>
        /// <exception cref="Exception">Thrown if the update operation fails or if the stored procedure does not return a valid result.</exception>
        public async Task<int> UpdatePatientAsync(Patient patient, int? doctorUserId = null)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString))
            using (var command = new SqlCommand("sp_UpdatePatient", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

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
                command.Parameters.AddWithValue("@patient_status", patient.PatientStatus);

                // Ya no se usa aseguradora ni código en esta etapa
                command.Parameters.AddWithValue("@doctor_userid", doctorUserId ?? (object)DBNull.Value);

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
                        throw new Exception("Error inesperado: No se obtuvo ningún resultado del procedimiento almacenado.");

                    using var document = JsonDocument.Parse(jsonResult);
                    var root = document.RootElement;

                    if (root.TryGetProperty("success", out var success) && success.GetInt32() == 1)
                    {
                        if (root.TryGetProperty("patientId", out var patientId))
                            return patientId.GetInt32();
                        else
                            throw new Exception("El campo 'patientId' no se encuentra en el resultado.");
                    }
                    else
                    {
                        string errorMessage = root.TryGetProperty("message", out var message)
                            ? message.GetString()
                            : "Error al actualizar el paciente.";
                        throw new Exception(errorMessage);
                    }
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        await connection.CloseAsync();
                }
            }
        }


        /// <summary>
        /// Asynchronously retrieves detailed patient information and associated doctors for the specified patient
        /// identifier.
        /// </summary>
        /// <remarks>The returned patient data includes personal information and a collection of
        /// associated doctors. If the patient does not exist, the method returns <see langword="null"/>. This method
        /// performs a database query and may be subject to network or database latency.</remarks>
        /// <param name="patientId">The unique identifier of the patient whose data is to be retrieved. Must be a valid patient ID.</param>
        /// <returns>A <see cref="DetailsPatientConsult"/> object containing the patient's details and a list of associated
        /// doctors, or <see langword="null"/> if no patient is found with the specified ID.</returns>
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

        /// <summary>
        /// Asynchronously retrieves detailed information for a patient by their unique identifier.
        /// </summary>
        /// <remarks>This method executes the stored procedure 'sp_GetPatientById' to obtain patient
        /// information. The returned <see cref="Patient"/> object includes all available fields from the database. If
        /// no patient is found with the specified identifier, the method returns <see langword="null"/>. This method is
        /// not thread-safe and should not be called concurrently on the same instance.</remarks>
        /// <param name="patientId">The unique identifier of the patient whose details are to be retrieved. Must correspond to an existing
        /// patient record.</param>
        /// <returns>A <see cref="Patient"/> object containing the patient's details if found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while retrieving patient details from the database.</exception>
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


        /// <summary>
        /// Retrieves detailed patient information, including personal and medical data, for the specified patient
        /// identifier.
        /// </summary>
        /// <remarks>The returned data includes both demographic and clinical information for the patient.
        /// If no patient exists with the specified identifier, the method returns <see langword="null"/>. This method
        /// performs asynchronous database access and should be awaited.</remarks>
        /// <param name="patientId">The unique identifier of the patient whose full details are to be retrieved. Must correspond to an existing
        /// patient record.</param>
        /// <returns>A <see cref="DetailsPatientConsult"/> object containing the patient's complete details if found; otherwise,
        /// <see langword="null"/>.</returns>
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
                                    PatientDocumentnumber = GetNullableString(reader, "patient_documentnumber"),
                                    PatientFirstname = GetNullableString(reader, "patient_firstname"),
                                    PatientMiddlename = GetNullableString(reader, "patient_middlename"),
                                    PatientFirstsurname = GetNullableString(reader, "patient_firstsurname"),
                                    PatientSecondlastname = GetNullableString(reader, "patient_secondlastname"),
                                    PatientGender = GetNullable<int>(reader, "patient_gender"),
                                    PatientGenderName = GetNullableString(reader, "patient_gender_name"),
                                    PatientBirthdate = reader.IsDBNull(reader.GetOrdinal("patient_birthdate"))
    ? (DateOnly?)null
    : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("patient_birthdate"))),
                                    PatientAge = (int)GetNullable<int>(reader, "patient_age"),
                                    PatientBloodtype = GetNullable<int>(reader, "patient_bloodtype"),
                                    PatientBloodtypeName = GetNullableString(reader, "patient_bloodtype_name"),
                                    PatientDonor = GetNullableString(reader, "patient_donor"),
                                    PatientMaritalstatus = GetNullable<int>(reader, "patient_maritalstatus"),
                                    PatientMaritalstatusName = GetNullableString(reader, "patient_maritalstatus_name"),
                                    PatientVocationalTraining = GetNullable<int>(reader, "patient_vocational_training"),
                                    PatientVocationalTrainingName = GetNullableString(reader, "patient_vocational_training_name"),
                                    PatientLandlinePhone = GetNullableString(reader, "patient_landline_phone"),
                                    PatientCellularPhone = GetNullableString(reader, "patient_cellular_phone"),
                                    PatientEmail = GetNullableString(reader, "patient_email"),
                                    PatientNationality = GetNullable<int>(reader, "patient_nationality"),
                                    PatientNationalityName = GetNullableString(reader, "patient_nationality_name"),
                                    PatientProvince = GetNullable<int>(reader, "patient_province"),
                                    PatientProvinceName = GetNullableString(reader, "patient_province_name"),
                                    PatientAddress = GetNullableString(reader, "patient_address"),
                                    PatientOcupation = GetNullableString(reader, "patient_ocupation"),
                                    PatientCompany = GetNullableString(reader, "patient_company"),
                                    PatientHealthInsurance = GetNullable<int>(reader, "patient_healt_insurance"),
                                    PatientHealthInsuranceName = GetNullableString(reader, "patient_health_insurance_name"),
                                    PatientCode = GetNullableString(reader, "patient_code"),
                                    PatientStatus = reader.GetInt32(reader.GetOrdinal("patient_status")),

                                    Temperature = GetNullable<decimal>(reader, "temperature"),
                                    RespiratoryRate = GetNullable<int>(reader, "respiratory_rate"),
                                    BloodPressureAS = GetNullableString(reader, "blood_pressureAS"),
                                    BloodPressureDIS = GetNullableString(reader, "blood_pressureDIS"),
                                    Pulse = GetNullableString(reader, "pulse"),
                                    Weight = GetNullableString(reader, "weight"),
                                    Size = GetNullableString(reader, "size"),
                                    VitalCreatedAt = GetNullable<DateTime>(reader, "vital_created_at"),
                                    VitalCreatedBy = GetNullable<int>(reader, "vital_created_by"),

                                    // ✅ Nuevos campos
                                    Imc = GetNullable<decimal>(reader, "imc"),
                                    AbdominalPerimeter = GetNullable<decimal>(reader, "abdominal_perimeter"),
                                    CapillaryHemoglobin = GetNullable<decimal>(reader, "capillary_hemoglobin"),
                                    CapillaryGlucose = GetNullable<decimal>(reader, "capillary_glucose"),
                                    Spo2 = GetNullable<decimal>(reader, "spo2"),
                                    // ✅ NUEVOS CAMPOS MAREADOS DESDE EL SP ACTUALIZADO
                                    LastPersonalBackground = GetNullableString(reader, "last_personal_background"),
                                    LastAllergiesIds = GetNullableString(reader, "last_allergies_ids"),
                                    LastSurgeriesIds = GetNullableString(reader, "last_surgeries_ids")

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
        /// <summary>
        /// Retrieves the value of the specified column as a nullable value type from the current row of the provided
        /// <see cref="SqlDataReader"/>.
        /// </summary>
        /// <remarks>If the specified column contains a database null (DBNull), the method returns <see
        /// langword="null"/>. Otherwise, it returns the value converted to type <typeparamref name="T"/>.</remarks>
        /// <typeparam name="T">The value type to retrieve from the data reader. Must be a struct.</typeparam>
        /// <param name="reader">The <see cref="SqlDataReader"/> instance to read the value from. Must not be null.</param>
        /// <param name="column">The name of the column to retrieve the value from. Must not be null or empty.</param>
        /// <returns>A nullable value of type <typeparamref name="T"/> containing the column value if it is not database null;
        /// otherwise, <see langword="null"/>.</returns>
        private static T? GetNullable<T>(SqlDataReader reader, string column) where T : struct
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index) ? (T?)null : reader.GetFieldValue<T>(index);
        }

        /// <summary>
        /// Retrieves the value of the specified column as a string, or returns null if the column contains a database
        /// null value.
        /// </summary>
        /// <param name="reader">The SqlDataReader instance from which to retrieve the column value. Must not be null and must be positioned
        /// on a valid record.</param>
        /// <param name="column">The name of the column to retrieve. Must correspond to a valid column in the current result set.</param>
        /// <returns>A string containing the value of the specified column, or null if the column value is DBNull.</returns>
        private static string? GetNullableString(SqlDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        /// <summary>
        /// Asynchronously retrieves patient data based on the specified document number.
        /// </summary>
        /// <remarks>This method executes the 'sp_GetPatientByCedula' stored procedure to obtain patient
        /// information. The operation is performed asynchronously and does not modify patient data.</remarks>
        /// <param name="documentNumber">The document number used to identify the patient. Cannot be null or empty.</param>
        /// <returns>A <see cref="Patient"/> object containing the patient's data if found; otherwise, <c>null</c>.</returns>
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
