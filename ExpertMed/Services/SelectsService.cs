using ExpertMed.Models;
using Microsoft.Data.SqlClient;
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

                    // --- LEER LA FOTO ---
                    int photoIndex = reader.GetOrdinal("users_profilephoto");
                    if (!reader.IsDBNull(photoIndex))
                    {
                        // Esto devuelve un byte[] completo si el proveedor lo soporta
                        var photoBytes = (byte[])reader.GetValue(photoIndex);
                        dto.UsersProfilephoto = photoBytes;
                        dto.UsersProfilephoto64 = Convert.ToBase64String(photoBytes);
                    }
                    else
                    {
                        dto.UsersProfilephoto = null;
                        dto.UsersProfilephoto64 = null;
                    }

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

        /// <summary>
        /// Metodo para obtener los tipos de formacion de la tabla catalogo
        /// </summary>
        /// <returns></returns>
        public async Task<List<Catalog>> GetProfessionaltrainingTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "FORMACION PROFESIONAL")
                .ToListAsync();
        }

        ///Metodo para obtener los tipos de seguros de salud de la tabla catalogo
        public async Task<List<Catalog>> GetSureHealtTypeAsync()
        {
            /// Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "SEGUROS DE SALUD")
                .ToListAsync();
        }

        ///Metodo para obtener los tipos de Parentesco de la tabla catalogo
        public async Task<List<Catalog>> GetRelationshipTypeAsync()
        {
            // Asumiendo que _dbContext es tu contexto de base de datos inyectado
            return await _dbContext.Catalogs
                .Where(c => c.CatalogCategory == "PARENTESCO")
                .ToListAsync();
        }

        ///Metodo para obtener los tipos de Antedecentes familiares de la tabla catalogo
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

        // En tu SelectService o el servicio correspondiente
        // Agregar al final de la clase SelectsService, antes de la última llave

        /// <summary>
        /// Crea un nuevo medicamento en el sistema
        /// </summary>
        /// <summary>
        /// Crea un nuevo medicamento en el sistema
        /// </summary>
        public async Task<(bool success, string message, MedicationDto data)> CreateMedicationAsync(CreateMedicationDto dto)
        {
            try
            {
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_medications_insert";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        var paramName = command.CreateParameter();
                        paramName.ParameterName = "@medications_name";
                        paramName.Value = dto.medications_name ?? (object)DBNull.Value;
                        command.Parameters.Add(paramName);

                        var paramDesc = command.CreateParameter();
                        paramDesc.ParameterName = "@medications_description";
                        paramDesc.Value = dto.medications_description ?? (object)DBNull.Value;
                        command.Parameters.Add(paramDesc);

                        var paramConc = command.CreateParameter();
                        paramConc.ParameterName = "@medications_concentration";
                        paramConc.Value = dto.medications_concentration ?? (object)DBNull.Value;
                        command.Parameters.Add(paramConc);

                        var paramCie = command.CreateParameter();
                        paramCie.ParameterName = "@medications_cie10";
                        paramCie.Value = dto.medications_cie10 ?? (object)DBNull.Value;
                        command.Parameters.Add(paramCie);

                        var paramStatus = command.CreateParameter();
                        paramStatus.ParameterName = "@medications_status";
                        paramStatus.Value = dto.medications_status ?? 1;
                        command.Parameters.Add(paramStatus);

                        // Parámetro de salida
                        var paramId = command.CreateParameter();
                        paramId.ParameterName = "@medications_id";
                        paramId.DbType = DbType.Int32;
                        paramId.Direction = ParameterDirection.Output;
                        command.Parameters.Add(paramId);

                        // Ejecutar
                        await command.ExecuteNonQueryAsync();

                        // Obtener el ID generado
                        var newId = (int)paramId.Value;

                        // Consultar el medicamento recién creado
                        var newMedication = await _dbContext.Medications
                            .FromSqlRaw("SELECT * FROM medications WHERE medications_id = {0}", newId)
                            .FirstOrDefaultAsync();

                        if (newMedication == null)
                        {
                            return (false, "No se pudo recuperar el medicamento creado", null);
                        }

                        var result = new MedicationDto
                        {
                            medications_id = newMedication.MedicationsId,
                            medications_name = newMedication.MedicationsName,
                            medications_description = newMedication.MedicationsDescription,
                            medications_category = newMedication.MedicationsCategory,
                            medications_distinctive = newMedication.MedicationsDistinctive,
                            medications_concentration = newMedication.MedicationsConcentration,
                            medications_cie10 = newMedication.MedicationsCie10,
                            medications_status = newMedication.MedicationsStatus ?? 1
                        };

                        return (true, "Medicamento creado exitosamente", result);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear medicamento");

                if (ex.Message.Contains("Ya existe un medicamento"))
                {
                    return (false, ex.Message, null);
                }

                return (false, "Error al crear el medicamento: " + ex.Message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear medicamento");
                return (false, "Error interno: " + ex.Message, null);
            }
        }


        /// <summary>
        /// Crea un nuevo registro de laboratorio en el sistema mediante Store Procedure
        /// </summary>
        public async Task<(bool success, string message, LaboratoryDto data)> CreateLaboratoryAsync(CreateLaboratoryDto dto)
        {
            try
            {
                // Usamos la conexión del contexto de Entity Framework
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_laboratories_insert";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        command.Parameters.Add(new SqlParameter("@laboratories_name", dto.laboratories_name ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@laboratories_description", dto.laboratories_description ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@laboratories_category", dto.laboratories_category ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@laboratories_cie10", dto.laboratories_cie10 ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@laboratories_status", dto.laboratories_status ?? 1));

                        // Parámetro de salida: @laboratories_id
                        var paramId = new SqlParameter
                        {
                            ParameterName = "@laboratories_id",
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramId);

                        // Ejecutar SP
                        await command.ExecuteNonQueryAsync();

                        // Obtener el ID generado por el SP
                        var newId = (int)paramId.Value;

                        // Consultar el registro recién creado para retornar el objeto completo (Efecto Refresco)
                        // Nota: Usamos la tabla laboratories generada por EF en el contexto
                        var newLab = await _dbContext.Laboratories
                            .FromSqlRaw("SELECT * FROM laboratories WHERE laboratories_id = {0}", newId)
                            .FirstOrDefaultAsync();

                        if (newLab == null)
                        {
                            return (false, "No se pudo recuperar el registro de laboratorio creado", null);
                        }

                        var result = new LaboratoryDto
                        {
                            laboratories_id = newLab.LaboratoriesId,
                            laboratories_name = newLab.LaboratoriesName,
                            laboratories_description = newLab.LaboratoriesDescription,
                            laboratories_category = newLab.LaboratoriesCategory,
                            laboratories_cie10 = newLab.LaboratoriesCie10,
                            laboratories_status = newLab.LaboratoriesStatus ?? 1
                        };

                        return (true, "Laboratorio registrado exitosamente", result);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear registro de laboratorio");

                // Capturamos el RAISERROR del Store Procedure
                if (ex.Message.Contains("Ya existe un examen"))
                {
                    return (false, ex.Message, null);
                }

                return (false, "Error de base de datos: " + ex.Message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear laboratorio");
                return (false, "Error interno: " + ex.Message, null);
            }
        }

        /// <summary>
        /// Crea un nuevo diagnóstico CIE-10 en el sistema mediante Store Procedure
        /// </summary>
        public async Task<(bool success, string message, DiagnosisDto data)> CreateDiagnosisAsync(CreateDiagnosisDto dto)
        {
            try
            {
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_diagnosis_insert";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        command.Parameters.Add(new SqlParameter("@diagnosis_name", dto.diagnosis_name ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@diagnosis_description", dto.diagnosis_description ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@diagnosis_category", dto.diagnosis_category ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@diagnosis_cie10", dto.diagnosis_cie10 ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@diagnosis_status", dto.diagnosis_status ?? 1));

                        // Parámetro de salida: @diagnosis_id
                        var paramId = new SqlParameter
                        {
                            ParameterName = "@diagnosis_id",
                            DbType = DbType.Int32,
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramId);

                        // Ejecutar SP
                        await command.ExecuteNonQueryAsync();
                        var newId = (int)paramId.Value;

                        // Recuperar el registro fresco para confirmar la carga
                        var newDiag = await _dbContext.Diagnoses
                            .FromSqlRaw("SELECT * FROM diagnosis WHERE diagnosis_id = {0}", newId)
                            .FirstOrDefaultAsync();

                        if (newDiag == null) return (false, "No se pudo recuperar el diagnóstico creado", null);

                        var result = new DiagnosisDto
                        {
                            diagnosis_id = newDiag.DiagnosisId,
                            diagnosis_name = newDiag.DiagnosisName,
                            diagnosis_description = newDiag.DiagnosisDescription,
                            diagnosis_category = newDiag.DiagnosisCategory,
                            diagnosis_cie10 = newDiag.DiagnosisCie10,
                            diagnosis_status = newDiag.DiagnosisStatus ?? 1
                        };

                        return (true, "Diagnóstico registrado exitosamente", result);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL al crear diagnóstico");
                // Captura el mensaje de duplicado del RAISERROR del SP
                if (ex.Message.Contains("Ya existe un diagnóstico")) return (false, ex.Message, null);
                return (false, "Error de base de datos: " + ex.Message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear diagnóstico");
                return (false, "Error interno: " + ex.Message, null);
            }
        }


        /// <summary>
        /// Inserta un nuevo estudio de imagenología mediante Store Procedure
        /// </summary>
        public async Task<(bool success, string message, ImageResponseDto data)> CreateImageAsync(CreateImageDto dto)
        {
            try
            {
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_images_insert";
                        command.CommandType = CommandType.StoredProcedure;

                        // Definición de parámetros
                        command.Parameters.Add(new SqlParameter("@images_name", dto.images_name ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@images_description", dto.images_description ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@images_category", dto.images_category ?? "IMAGEN"));
                        command.Parameters.Add(new SqlParameter("@images_cie10", dto.images_cie10 ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@images_status", dto.images_status ?? 1));

                        // Parámetro de salida para recuperar el ID insertado
                        var outputId = new SqlParameter
                        {
                            ParameterName = "@images_id",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputId);

                        await command.ExecuteNonQueryAsync();

                        int newId = (int)outputId.Value;

                        // Recuperamos el objeto recién creado para enviarlo al frontend
                        var result = new ImageResponseDto
                        {
                            images_id = newId,
                            images_name = dto.images_name,
                            images_description = dto.images_description,
                            images_category = dto.images_category ?? "IMAGEN",
                            images_cie10 = dto.images_cie10,
                            images_status = dto.images_status ?? 1
                        };

                        return (true, "Estudio de imagen registrado correctamente", result);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Captura el RAISERROR del SP (duplicados o falta de nombre)
                return (false, ex.Message, null);
            }
            catch (Exception ex)
            {
                return (false, "Error interno del servidor: " + ex.Message, null);
            }
        }


        /// <summary>
        /// Inserta un nuevo item en la tabla catalogs mediante Store Procedure.
        /// </summary>
        public async Task<(bool success, string message, CatalogResponseDto data)> CreateCatalogItemAsync(CreateCatalogDto dto)
        {
            try
            {
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_InsertCatalogItem";
                        command.CommandType = CommandType.StoredProcedure;

                        // Normalización previa: Eliminar espacios innecesarios
                        command.Parameters.Add(new SqlParameter("@Name", dto.CatalogName?.Trim() ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Category", dto.CatalogCategory?.Trim() ?? (object)DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Status", dto.CategoryStatus ?? 1));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var result = new CatalogResponseDto
                                {
                                    // CAMBIO: Usar Convert.ToInt32 para manejar el Decimal que devuelve SQL
                                    CatalogId = Convert.ToInt32(reader["catalog_id"]),
                                    CatalogName = reader.GetString(reader.GetOrdinal("catalog_name")),
                                    CatalogCategory = reader.GetString(reader.GetOrdinal("catalog_category")),
                                    CategoryStatus = dto.CategoryStatus ?? 1
                                };

                                return (true, "Registro de catálogo creado exitosamente", result);
                            }
                        }

                        return (false, "Error al procesar el retorno del procedimiento", null);
                    }
                }
            }
            catch (SqlException ex)
            {
                // Captura el RAISERROR del SP (duplicados por espacios o mayúsculas)
                return (false, ex.Message, null);
            }
            catch (Exception ex)
            {
                return (false, "Error crítico: " + ex.Message, null);
            }
        }


    }
}
