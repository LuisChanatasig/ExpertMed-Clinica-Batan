using Azure.Core;
using ExpertMed.Models;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

namespace ExpertMed.Services
{
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public UserService(IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public List<Users> GetAllUsers(int usuarioId, int perfilId)
        {
            var users = new List<Users>();

            using (SqlConnection connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                using (SqlCommand command = new SqlCommand("sp_ListAllUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@usuarioId", usuarioId);
                    command.Parameters.AddWithValue("@perfilId", perfilId);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new Users
                            {
                                UserId = reader.GetInt32(reader.GetOrdinal("users_id")),
                                DocumentNumber = reader.GetString(reader.GetOrdinal("users_document_number")),
                                Names = reader.GetString(reader.GetOrdinal("users_names")),
                                Surnames = reader.GetString(reader.GetOrdinal("users_surcenames")),
                                Phone = reader.GetString(reader.GetOrdinal("users_phone")),
                                Email = reader.GetString(reader.GetOrdinal("users_email")),
                                CreationDate = reader.GetDateTime(reader.GetOrdinal("users_creationdate")),

                                ModificationDate = reader.IsDBNull(reader.GetOrdinal("users_modificationdate"))
                                    ? DateTime.MinValue
                                    : reader.GetDateTime(reader.GetOrdinal("users_modificationdate")),

                                Address = reader.GetString(reader.GetOrdinal("users_address")),

                                ProfilePhoto = reader.IsDBNull(reader.GetOrdinal("users_profilephoto"))
                                    ? string.Empty
                                    : reader["users_profilephoto"] as string,

                                ProfilePhoto64 = reader.IsDBNull(reader.GetOrdinal("users_profilephoto64"))
                                    ? string.Empty
                                    : reader["users_profilephoto64"] as string,

                                SenecytCode = reader.IsDBNull(reader.GetOrdinal("users_senecytcode"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("users_senecytcode")),

                                XKeyTaxo = reader.IsDBNull(reader.GetOrdinal("users_xkeytaxo"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("users_xkeytaxo")),

                                XPassTaxo = reader.IsDBNull(reader.GetOrdinal("users_xpasstaxo"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("users_xpasstaxo")),

                                Login = reader.GetString(reader.GetOrdinal("users_login")),
                                Password = reader.GetString(reader.GetOrdinal("users_password")),
                                Status = reader.GetInt32(reader.GetOrdinal("users_status")),
                                ProfileId = reader.GetInt32(reader.GetOrdinal("users_profileid")),

                                EstablishmentName = reader.IsDBNull(reader.GetOrdinal("establishment_name"))
                                    ? "Sin establecimiento"
                                    : reader.GetString(reader.GetOrdinal("establishment_name")),

                                EstablishmentAddress = reader.IsDBNull(reader.GetOrdinal("establishment_address"))
                                    ? "Sin dirección"
                                    : reader.GetString(reader.GetOrdinal("establishment_address")),

                                VatPercentageId = reader.IsDBNull(reader.GetOrdinal("users_vatpercentageid"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("users_vatpercentageid")),

                                SpecialityId = reader.IsDBNull(reader.GetOrdinal("users_specialityid"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("users_specialityid")),

                                CountryId = reader.IsDBNull(reader.GetOrdinal("users_countryid"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("users_countryid")),

                                Description = reader.IsDBNull(reader.GetOrdinal("users_description"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("users_description")),

                                ProfileName = reader.IsDBNull(reader.GetOrdinal("profile_name"))
                                    ? "Sin perfil"
                                    : reader.GetString(reader.GetOrdinal("profile_name")),

                                SpecialityName = reader.IsDBNull(reader.GetOrdinal("speciality_name"))
                                    ? "Sin especialidad"
                                    : reader.GetString(reader.GetOrdinal("speciality_name")),

                                CountryName = reader.IsDBNull(reader.GetOrdinal("country_name"))
                                    ? "Sin país"
                                    : reader.GetString(reader.GetOrdinal("country_name")),

                                VatBillingPercentage = reader.IsDBNull(reader.GetOrdinal("vatbilling_percentage"))
                                    ? "0%"
                                    : reader.GetString(reader.GetOrdinal("vatbilling_percentage"))
                            };

                            users.Add(user);
                        }
                    }
                }
            }

            return users;
        }


        // Método para activar o desactivar al usuario
        public async Task<(bool success, string message)> DesactiveOrActiveUserAsync(int userId, int status)
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
                        command.CommandText = "SP_DesactiveOrActiveUser";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros del procedimiento almacenado
                        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
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
                _logger.LogError(ex, "Error al activar/desactivar el usuario.");
                return (false, "Ocurrió un error al procesar la solicitud.");
            }

        }

        //CREAR UN NUEVO USUARIO
        public async Task<int> CreateUserAsync(UserViewModel usuario, List<int>? associatedDoctorIds = null)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
            using var command = new SqlCommand("sp_CreateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetros personales
            command.Parameters.Add(new SqlParameter("@ProfilePhoto", SqlDbType.VarBinary) { Value = usuario.UserProfilephoto ?? (object)DBNull.Value });
            command.Parameters.AddWithValue("@ProfileId", usuario.UserProfileid);
            command.Parameters.AddWithValue("@DocumentNumber", usuario.UserDocumentNumber);
            command.Parameters.AddWithValue("@Names", usuario.UserNames);
            command.Parameters.AddWithValue("@Surnames", usuario.UserSurnames);
            command.Parameters.AddWithValue("@Address", usuario.UserAddress);
            command.Parameters.AddWithValue("@SenecytCode", usuario.UserSenecytcode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", usuario.UserPhone);
            command.Parameters.AddWithValue("@Email", usuario.UserEmail);
            command.Parameters.AddWithValue("@SpecialtyId", usuario.UserSpecialtyid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CountryId", usuario.UserCountryid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Login", usuario.UserLogin);
            command.Parameters.AddWithValue("@Password", usuario.UserPassword);

            // Parámetros taxo
            command.Parameters.AddWithValue("@EstablishmentId", usuario.UserEstablishmentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@VatPercentageId", usuario.UserVatpercentageid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@XKeyTaxo", usuario.UserXkeytaxo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@XPassTaxo", usuario.UserXpasstaxo ?? (object)DBNull.Value);

            // Horario
            command.Parameters.AddWithValue("@StartTime", usuario.StartTime == TimeOnly.MinValue ? (object)DBNull.Value : DateTime.Today.Add(usuario.StartTime.ToTimeSpan()));
            command.Parameters.AddWithValue("@EndTime", usuario.EndTime == TimeOnly.MinValue ? (object)DBNull.Value : DateTime.Today.Add(usuario.EndTime.ToTimeSpan()));
            command.Parameters.AddWithValue("@AppointmentInterval", usuario.AppointmentInterval);
            command.Parameters.AddWithValue("@Description", usuario.UserDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@WorkingDays", usuario.WorksDays ?? (object)DBNull.Value);

            // Asistente - Médicos
            command.Parameters.AddWithValue("@DoctorIds", associatedDoctorIds != null && associatedDoctorIds.Any()
                ? string.Join(",", associatedDoctorIds)
                : (object)DBNull.Value);

            // Archivos adicionales
            command.Parameters.AddWithValue("@CompanyLogo", usuario.CompanyLogoBytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CompanyLogoFileName", usuario.CompanyLogoFileName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CompanyLogoContentType", usuario.CompanyLogoContentType ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@CertificateP12", usuario.CertificateP12Bytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CertificateP12FileName", usuario.CertificateP12FileName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CertificateP12ContentType", usuario.CertificateP12ContentType ?? (object)DBNull.Value);

            // TVP de consultorios
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));

            if (usuario.SelectedOfficeIds != null)
            {
                foreach (var id in usuario.SelectedOfficeIds)
                {
                    table.Rows.Add(id);
                }
            }

            var consultoriosParam = new SqlParameter("@ConsultoriosIds", SqlDbType.Structured)
            {
                TypeName = "dbo.IdList",
                Value = table
            };
            command.Parameters.Add(consultoriosParam);

            // Usuario que asigna
            command.Parameters.AddWithValue("@AssignedBy", usuario.AssignedBy);

            try
            {
                await connection.OpenAsync();

                string jsonResult = null;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        jsonResult = reader.GetString(0);
                }

                if (string.IsNullOrEmpty(jsonResult))
                    throw new Exception("No se obtuvo ningún resultado del procedimiento almacenado.");

                using var document = JsonDocument.Parse(jsonResult);
                var root = document.RootElement;

                if (root.TryGetProperty("success", out var success) && success.GetInt32() == 1)
                {
                    if (root.TryGetProperty("userId", out var userId))
                        return userId.GetInt32();
                    throw new Exception("No se recibió el campo 'userId'.");
                }

                var errorMessage = root.TryGetProperty("message", out var message)
                    ? message.GetString()
                    : "Error al crear el usuario.";
                throw new Exception(errorMessage);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }



        // Método para obtener un usuario por su ID
        public async Task<UserWithDetails> GetUserDetailsAsync(int userId)
        {
            UserWithDetails userDetails = null;
            var doctors = new List<DoctorDto>();
            var consultorios = new List<MedicalOfficeDto>();
            var files = new List<UserFileDto>();

            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var command = new SqlCommand("sp_ListUserById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@user_id", userId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                // 🧾 Primer result set: datos del usuario
                if (await reader.ReadAsync())
                {
                    userDetails = new UserWithDetails
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("users_id")),
                        DocumentNumber = reader.GetString(reader.GetOrdinal("users_document_number")),
                        Names = reader.GetString(reader.GetOrdinal("users_names")),
                        Surnames = reader.GetString(reader.GetOrdinal("users_surcenames")),
                        Phone = reader.GetString(reader.GetOrdinal("users_phone")),
                        Email = reader.GetString(reader.GetOrdinal("users_email")),
                        CreationDate = reader.GetDateTime(reader.GetOrdinal("users_creationdate")),
                        ModificationDate = reader.IsDBNull(reader.GetOrdinal("users_modificationdate"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("users_modificationdate")),
                        Address = reader.GetString(reader.GetOrdinal("users_address")),
                        ProfilePhoto = reader["users_profilephoto"] as byte[],
                        ProfilePhoto64 = reader["users_profilephoto"] != DBNull.Value
                            ? "data:image/png;base64," + Convert.ToBase64String((byte[])reader["users_profilephoto"])
                            : "assets/images/users/UsersIcon",
                        SenecytCode = reader["users_senecytcode"] as string,
                        XKeyTaxo = reader["users_xkeytaxo"] as string,
                        XPassTaxo = reader["users_xpasstaxo"] as string,
                        Login = reader.GetString(reader.GetOrdinal("users_login")),
                        Status = reader.GetInt32(reader.GetOrdinal("users_status")),
                        ProfileId = reader.GetInt32(reader.GetOrdinal("users_profileid")),
                        UserCountryid = reader.IsDBNull(reader.GetOrdinal("users_countryid")) ? null : reader.GetInt32(reader.GetOrdinal("users_countryid")),
                        UserDescription = reader.IsDBNull(reader.GetOrdinal("users_description"))
                            ? "Sin especificar"
                            : reader.GetString(reader.GetOrdinal("users_description")),
                        ProfileName = reader.IsDBNull(reader.GetOrdinal("profile_name"))
                            ? "Sin perfil"
                            : reader.GetString(reader.GetOrdinal("profile_name")),
                        UserEstablishmentid = reader.IsDBNull(reader.GetOrdinal("user_establishment_id")) ? null : reader.GetInt32(reader.GetOrdinal("user_establishment_id")),
                        SpecialtyName = reader.IsDBNull(reader.GetOrdinal("speciality_name"))
                            ? "Sin especialidad"
                            : reader.GetString(reader.GetOrdinal("speciality_name")),
                        CountryName = reader.IsDBNull(reader.GetOrdinal("country_name"))
                            ? "Sin país"
                            : reader.GetString(reader.GetOrdinal("country_name")),
                        StartTime = reader.GetTimeSpan(reader.GetOrdinal("start_time")),
                        EndTime = reader.GetTimeSpan(reader.GetOrdinal("end_time")),
                        AppointmentInterval = reader.GetInt32(reader.GetOrdinal("appointment_interval"))
                    };
                }

                // 🗂 Segundo result set: archivos (logo, certificado, etc.)
                if (await reader.NextResultAsync() && reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        files.Add(new UserFileDto
                        {
                            FileType = reader.GetString(reader.GetOrdinal("file_type")),
                            FileName = reader.GetString(reader.GetOrdinal("file_name")),
                            FileContent = reader["file_content"] as byte[],
                            ContentType = reader.GetString(reader.GetOrdinal("content_type"))
                        });
                    }
                }

                // 🏥 Tercer result set: consultorios
                if (await reader.NextResultAsync() && reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        consultorios.Add(new MedicalOfficeDto
                        {
                            OfficeId = reader.GetInt32(reader.GetOrdinal("medicaloffice_id")),
                            OfficeName = reader.IsDBNull(reader.GetOrdinal("medicaloffice_name"))
                                ? "Sin nombre"
                                : reader.GetString(reader.GetOrdinal("medicaloffice_name")),
                            OfficeLocation = reader.IsDBNull(reader.GetOrdinal("medicaloffice_location"))
                                ? "Sin ubicación"
                                : reader.GetString(reader.GetOrdinal("medicaloffice_location"))
                        });
                    }
                }

                // 👨‍⚕️ Cuarto result set: médicos asociados
                if (userDetails != null && userDetails.ProfileName.Equals("Asistente", StringComparison.OrdinalIgnoreCase))
                {
                    if (await reader.NextResultAsync() && reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            doctors.Add(new DoctorDto
                            {
                                DoctorId = reader.GetInt32(reader.GetOrdinal("doctor_id")),
                                DoctorNames = reader.GetString(reader.GetOrdinal("doctor_names")),
                                DoctorSurnames = reader.GetString(reader.GetOrdinal("doctor_surnames")),
                                DoctorSpecialtyId = reader.GetInt32(reader.GetOrdinal("doctor_specialtyid")),
                                DoctorSpecialtyName = reader.GetString(reader.GetOrdinal("doctor_specialty_name"))
                            });
                        }
                    }
                }

                // 🧩 Asignar listas completadas
                if (userDetails != null)
                {
                    userDetails.Doctors = doctors;
                    userDetails.MedicalOffices = consultorios;
                    userDetails.UserFiles = files;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los detalles del usuario", ex);
            }

            return userDetails;
        }

        //Metodo para actualizar un usuario


        public async Task UpdateUserAsync(int userId, UserViewModel usuario, List<int>? associatedDoctorIds = null)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
            using var command = new SqlCommand("SP_UpdateUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetro obligatorio
            command.Parameters.AddWithValue("@UserId", userId);

            // Datos personales
            command.Parameters.Add(new SqlParameter("@ProfilePhoto", SqlDbType.VarBinary) { Value = usuario.UserProfilephoto ?? (object)DBNull.Value });
            command.Parameters.AddWithValue("@ProfileId", usuario.UserProfileid);
            command.Parameters.AddWithValue("@DocumentNumber", usuario.UserDocumentNumber);
            command.Parameters.AddWithValue("@Names", usuario.UserNames);
            command.Parameters.AddWithValue("@Surnames", usuario.UserSurnames);
            command.Parameters.AddWithValue("@Address", usuario.UserAddress);
            command.Parameters.AddWithValue("@SenecytCode", usuario.UserSenecytcode ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Phone", usuario.UserPhone);
            command.Parameters.AddWithValue("@Email", usuario.UserEmail);
            command.Parameters.AddWithValue("@SpecialtyId", usuario.UserSpecialtyid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CountryId", usuario.UserCountryid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Login", usuario.UserLogin);
            command.Parameters.AddWithValue("@Password", usuario.UserPassword);

            // Datos fiscales (Taxo)
            command.Parameters.AddWithValue("@EstablishmentId", usuario.UserEstablishmentId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@VatPercentageId", usuario.UserVatpercentageid ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@XKeyTaxo", usuario.UserXkeytaxo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@XPassTaxo", usuario.UserXpasstaxo ?? (object)DBNull.Value);

            // Horario y descripción
            command.Parameters.AddWithValue("@StartTime", usuario.StartTime == TimeOnly.MinValue ? (object)DBNull.Value : DateTime.Today.Add(usuario.StartTime.ToTimeSpan()));
            command.Parameters.AddWithValue("@EndTime", usuario.EndTime == TimeOnly.MinValue ? (object)DBNull.Value : DateTime.Today.Add(usuario.EndTime.ToTimeSpan()));
            command.Parameters.AddWithValue("@AppointmentInterval", usuario.AppointmentInterval);
            command.Parameters.AddWithValue("@Description", usuario.UserDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@WorkingDays", usuario.WorksDays ?? (object)DBNull.Value);

            // Relación asistente-médico
            command.Parameters.AddWithValue("@DoctorIds", associatedDoctorIds != null && associatedDoctorIds.Any()
                ? string.Join(",", associatedDoctorIds)
                : (object)DBNull.Value);

            // Archivos
            command.Parameters.AddWithValue("@CompanyLogo", usuario.CompanyLogoBytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CompanyLogoFileName", usuario.CompanyLogoFileName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CompanyLogoContentType", usuario.CompanyLogoContentType ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@CertificateP12", usuario.CertificateP12Bytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CertificateP12FileName", usuario.CertificateP12FileName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CertificateP12ContentType", usuario.CertificateP12ContentType ?? (object)DBNull.Value);

            // TVP: consultorios
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));

            if (usuario.SelectedOfficeIds != null)
            {
                foreach (var id in usuario.SelectedOfficeIds)
                    table.Rows.Add(id);
            }

            var consultoriosParam = new SqlParameter("@ConsultoriosIds", SqlDbType.Structured)
            {
                TypeName = "dbo.IdList",
                Value = table
            };
            command.Parameters.Add(consultoriosParam);

            // Usuario que asigna
            command.Parameters.AddWithValue("@AssignedBy", usuario.AssignedBy);

            try
            {
                await connection.OpenAsync();

                string jsonResult = null;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        jsonResult = reader.GetString(0);
                }

                if (string.IsNullOrEmpty(jsonResult))
                    throw new Exception("No se obtuvo ningún resultado del procedimiento almacenado.");

                using var document = JsonDocument.Parse(jsonResult);
                var root = document.RootElement;

                if (root.TryGetProperty("success", out var success) && success.GetInt32() == 1)
                {
                    // Todo bien
                    return;
                }

                var errorMessage = root.TryGetProperty("message", out var message)
                    ? message.GetString()
                    : "Error al actualizar el usuario.";
                throw new Exception(errorMessage);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }
        }



    }
}
