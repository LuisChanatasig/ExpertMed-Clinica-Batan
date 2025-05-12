using ExpertMed.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class AuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }



        public async Task<User> ValidateCredentialsAsync(string usersLogin, string usersPassword)
        {
            if (string.IsNullOrWhiteSpace(usersLogin) || string.IsNullOrWhiteSpace(usersPassword))
            {
                throw new ArgumentException("El nombre de usuario y la contraseña no pueden estar vacíos.");
            }
            User user = null;

            using (SqlConnection connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (SqlCommand command = new SqlCommand("sp_ValidateCredentials", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@users_login", usersLogin);
                command.Parameters.AddWithValue("@users_password", usersPassword);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        // Validar el campo IsValid
                        bool isValid = reader["IsValid"] != DBNull.Value && Convert.ToBoolean(reader["IsValid"]);

                        if (!isValid)
                        {
                            string errorMessage = reader["Message"] != DBNull.Value ? Convert.ToString(reader["Message"]) : "Credenciales no válidas.";
                            throw new UnauthorizedAccessException(errorMessage);
                        }

                        // Procesar información del usuario si es válido
                        user = new User
                        {
                            UsersId = GetValueOrDefault<int>(reader, "users_id"),
                            UsersSpecialityid = GetValueOrDefault<int>(reader, "speciality_id"),
                            UsersDocumentNumber = GetValueOrDefault<string>(reader, "users_document_number"),
                            UsersNames = GetValueOrDefault<string>(reader, "users_names"),
                            UsersSurcenames = GetValueOrDefault<string>(reader, "users_surcenames"),
                            UsersLogin = GetValueOrDefault<string>(reader, "users_login"),
                            UsersAddress = GetValueOrDefault<string>(reader, "users_address"),
                            UsersPhone = GetValueOrDefault<string>(reader, "users_phone"),
                            UsersEmail = GetValueOrDefault<string>(reader, "users_email"),
                            UsersProfile = new Profile
                            {
                                ProfileId = GetValueOrDefault<int>(reader, "profile_id"),
                                ProfileDescription = GetValueOrDefault<string>(reader, "profile_name")
                            },
                            UsersSpeciality = new Speciality
                            {
                                SpecialityId = GetValueOrDefault<int>(reader, "speciality_id"),
                                SpecialityName = GetValueOrDefault<string>(reader, "speciality_name")
                            },
                            UserEstablishment = new Establishment
                            {
                                EstablishmentId = GetValueOrDefault<int>(reader, "establishment_id"),
                                EstablishmentAddress = GetValueOrDefault<string>(reader, "establishment_address")
                            },
                            UsersProfilephoto = reader.IsDBNull(reader.GetOrdinal("user_profilephoto")) ? null : (byte[])reader["user_profilephoto"],
                            UsersDescription = GetValueOrDefault<string>(reader, "users_description")
                        };

                        // Manejo de imágenes nulas
                        user.UsersProfilephoto ??= new byte[0];

                        // Guardar detalles en la sesión
                        var session = _httpContextAccessor.HttpContext.Session;
                        session.SetString("UsuarioNombre", user.UsersNames);
                        session.SetInt32("UsuarioId", user.UsersId);
                        session.SetString("UsuarioApellido", user.UsersSurcenames);
                        session.SetString("UsuarioDescripcion", user.UsersDescription ?? "No description");
                        session.SetInt32("UsuarioEspecialidadId", user.UsersSpecialityid ?? 0);
                        session.SetString("UsuarioEspecialidad", user.UsersSpeciality?.SpecialityName ?? "No specialty");
                        session.SetString("UsuarioEstablecimiento", user.UserEstablishment.EstablishmentName ?? "No establish");
                        session.SetString("UsuarioEstablecimientoDireccion", user.UserEstablishment.EstablishmentAddress ?? "No Address");
                        session.SetString("UsuarioDireccion", user.UsersAddress ?? "No address");
                        session.SetString("UsuarioTelefono", user.UsersPhone ?? "No phone number");
                        session.SetString("UsuarioEmail", user.UsersEmail ?? "No email");
                        session.SetString("UsuarioFotoPerfil", ConvertToBase64(user.UsersProfilephoto)); // Imagen en Base64

                        if (user.UsersProfile != null)
                        {
                            session.SetInt32("PerfilId", user.UsersProfile.ProfileId);
                        }

                        return user;
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Credenciales no válidas o usuario inactivo.");
                    }

                    //if(user?.UsersProfileid == 3)
                    //{
                    //    user.Ass


                    //}
                }
            }
        }



        private static T GetValueOrDefault<T>(IDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? default : (T)reader.GetValue(ordinal);
        }

        private static string ConvertToBase64(byte[] data)
        {
            return data != null && data.Length > 0 ? Convert.ToBase64String(data) : string.Empty;
        }


    }
}

//dotnet ef dbcontext scaffold "Server=localhost;Database=DB_EXPERTMED;User Id=sa;Password=1717;" Microsoft.EntityFrameworkCore.SqlServer -o Models -c MyDbContext  --force
