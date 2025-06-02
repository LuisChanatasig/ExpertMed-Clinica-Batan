using ExpertMed.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class SelectsService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SelectsService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public SelectsService(IHttpContextAccessor httpContextAccessor, ILogger<SelectsService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

        }

        // Método para obtener todos los perfiles
        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllProfiles
                var profiles = await _dbContext.Profiles
                    .FromSqlRaw("EXEC sp_ListAllProfiles")
                    .ToListAsync();

                return profiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los perfiles.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }

        // Método para obtener todas las especialidades
        public async Task<List<Speciality>> GetAllSpecialtiesAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var specialties = await _dbContext.Specialities
                    .FromSqlRaw("EXEC sp_ListAllSpecialities")
                    .ToListAsync();

                return specialties;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las especialidades.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }

        // Método para obtener todas las Nacionalidades
        public async Task<List<Country>> GetAllCountriesAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var countries = await _dbContext.Countries
                    .FromSqlRaw("EXEC sp_ListAllCountries")
                    .ToListAsync();

                return countries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las nacionalidades.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }
        public async Task<List<Establishment>> GetAllEstablishmentAsync(int userProfile, int userId)
        {
            try
            {
                var establishments = await _dbContext.Establishments
                    .FromSqlRaw("EXEC sp_ListAllEstablishment @UserProfile = {0}, @UserId = {1}", userProfile, userId)
                    .ToListAsync();

                return establishments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los establecimientos.");
                throw;
            }
        }

        // Método para obtener todas los porcentajes de iva
        public async Task<List<VatBilling>> GetAllVatPercentageAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var pencentage = await _dbContext.VatBillings
                    .FromSqlRaw("EXEC sp_ListAllPercentage")
                    .ToListAsync();

                return pencentage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los porcentajes.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }

        /// <summary>
        /// Método para obtener todos los Medicos
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<User>> GetAllMedicsAsync(int userProfile, int userId)
        {
            try
            {
                var medics = await _dbContext.Users
                    .FromSqlRaw("EXEC sp_ListAllMedics @UserProfile = {0}, @UserId = {1}", userProfile, userId)
                    .ToListAsync();


                return medics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los médicos.");
                throw;
            }
        }

        // Método para obtener todos los Medicos y sus caractaristicas
        public async Task<List<MedicDetails>> GetAllMedicsDetailsAsync(int establecimientoId)
        {
            var results = new List<MedicDetails>();

            try
            {
                using var conn = _dbContext.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_ListAllMedicsAndDetails";
                cmd.CommandType = CommandType.StoredProcedure;

                var param = cmd.CreateParameter();
                param.ParameterName = "@establecimientoId";
                param.Value = establecimientoId;
                param.DbType = DbType.Int32;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var dto = new MedicDetails
                    {
                        UsersId = reader.GetInt32(reader.GetOrdinal("users_id")),
                        UsersNames = reader.GetString(reader.GetOrdinal("users_names")),
                        UsersSurcenames = reader.GetString(reader.GetOrdinal("users_surcenames")),
                        SpecialityName = reader.GetString(reader.GetOrdinal("SpecialityName")),
                        UsersEstablishmentId = reader.GetInt32(reader.GetOrdinal("establishment_id")),
                        UsersEstablishmentName = reader.GetString(reader.GetOrdinal("establishment_name"))
                    };

                    results.Add(dto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los Médicos.");
                throw;
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Province>> GetAllProvinceAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var province = await _dbContext.Provinces
                    .FromSqlRaw("EXEC sp_ListAllProvinces")
                    .ToListAsync();

                return province;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las Provincias.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Diagnosis>> GetAllDiagnosisAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var diagnoses = await _dbContext.Diagnoses
                    .FromSqlRaw("EXEC sp_ListAllDiagnosis")
                    .ToListAsync();

                return diagnoses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los diagnosticos.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Medication>> GetAllMedicationsAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var medications = await _dbContext.Medications
                    .FromSqlRaw("EXEC sp_ListAllMedications")
                    .ToListAsync();

                return medications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los medicamentos.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Image>> GetAllImagesAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var images = await _dbContext.Images
                    .FromSqlRaw("EXEC sp_ListAllImages")
                    .ToListAsync();

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los imagenes.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Laboratory>> GetAllLaboratoriesAsync()
        {
            try
            {
                // Ejecuta el procedimiento almacenado sp_ListAllSpecialities
                var laboratories = await _dbContext.Laboratories
                    .FromSqlRaw("EXEC sp_ListAllLaboratories")
                    .ToListAsync();

                return laboratories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los imagenes.");
                throw; // O manejar el error de forma más específica si es necesario
            }
        }


        public async Task<List<InsuranceCompanyDto>> GetInsuranceByEstablishmentAsync(int estid)
        {
            try
            {
                var result = new List<InsuranceCompanyDto>();

                var connection = _dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_GetInsuranceByEstablishment";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@establishment_id";
                    param.Value = estid;
                    command.Parameters.Add(param);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new InsuranceCompanyDto
                            {
                                InsuranceCompanyId = reader.GetInt32(0),
                                InsuranceCompanyName = reader.GetString(1)
                            });
                        }
                    }

                    await connection.CloseAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener aseguradoras por establecimiento.");
                throw;
            }
        }



        //Metodo para obtener los tipos de genero de la tabla catalogo
        public async Task<List<Catalog>> GetGenderTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "GENERO")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de sangre de la tabla catalogo
        public async Task<List<Catalog>> GetBloodTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "TIPO DE SANGRE")
                .ToListAsync();
        }

        //Metodo para obtener los tipos de documentos de la tabla catalogo
        public async Task<List<Catalog>> GetDocumentTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "TIPO DOCUMENTO")
                .ToListAsync();
        }

        //Metodo para obtener los tipos de estado civil de la tabla catalogo
        public async Task<List<Catalog>> GetCivilTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "ESTADO CIVIL")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de formacion de la tabla catalogo
        public async Task<List<Catalog>> GetProfessionaltrainingTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "FORMACION PROFESIONAL")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de seguros de salud de la tabla catalogo
        public async Task<List<Catalog>> GetSureHealtTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "SEGUROS DE SALUD")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de Parentesco de la tabla catalogo
        public async Task<List<Catalog>> GetRelationshipTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "PARENTESCO")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de Antedecentes familiares de la tabla catalogo
        public async Task<List<Catalog>> GetFamiliarTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "ANTECEDENTES FAMILIARES")
                .ToListAsync();
        }

        //Metodo para obtener los tipos de Alergias de la tabla catalogo
        public async Task<List<Catalog>> GetAllergiesTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "ALERGIAS")
                .ToListAsync();
        }
        //Metodo para obtener los tipos de Cirugias de la tabla catalogo
        public async Task<List<Catalog>> GetSurgeriesTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "CIRUGIAS")
                .ToListAsync();
        }
    }
}
