using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class EstablishmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PatientService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public EstablishmentService(IHttpContextAccessor httpContextAccessor, ILogger<PatientService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Servicio que consume el sp para crear el establecimiento
        /// </summary>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="emissionPoint"></param>
        /// <param name="pointOfSale"></param>
        /// <param name="creationDate"></param>
        /// <param name="modificationDate"></param>
        /// <param name="sequentialBilling"></param>
        /// <param name="logo"></param>
        /// <returns></returns>
        public async Task<string> CreateEstablishmentAsync(
        string name,
        string address = null,
        string emissionPoint = null,
        string pointOfSale = null,
        DateTime? creationDate = null,
        DateTime? modificationDate = null,
        int? sequentialBilling = null,
        byte[] logo = null)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_dbContext.Database.GetConnectionString());
                using SqlCommand cmd = new SqlCommand("sp_CreateEstablishment", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@establishment_name", name);

                cmd.Parameters.AddWithValue("@establishment_address", (object?)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_emissionpoint", (object?)emissionPoint ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_pointofsale", (object?)pointOfSale ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_creationdate", (object?)creationDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_modificationdate", (object?)modificationDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_sequential_billing", (object?)sequentialBilling ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@establishment_logo", (object?)logo ?? DBNull.Value);

                await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                // Leer PRINT del SP si existe (lo retorna como InfoMessage)
                string result = "Establecimiento creado correctamente.";

                conn.InfoMessage += (sender, e) =>
                {
                    result = e.Message;
                };

                return result;
            }
            catch (SqlException ex)
            {
                return $"SQL Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"General Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Servicio para listar los establecimientos
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<EstablishmentDto>> ListEstablishmentsAsync()
        {
            var result = new List<EstablishmentDto>();

            try
            {
                using SqlConnection conn = new SqlConnection(_dbContext.Database.GetConnectionString());
                using SqlCommand cmd = new SqlCommand("sp_ListEstablishment", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await conn.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var establishment = new EstablishmentDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader["Name"]?.ToString(),
                        Address = reader["Address"]?.ToString(),
                        EmissionPoint = reader["EmissionPoint"]?.ToString(),
                        PointOfSale = reader["PointOfSale"]?.ToString(),
                        CreationDate = reader["CreationDate"] as DateTime?,
                        ModificationDate = reader["ModificationDate"] as DateTime?,
                        SequentialBilling = reader["SequentialBilling"] as int?,
                        Logo = reader["Logo"] as byte[]
                    };

                    result.Add(establishment);
                }
            }
            catch (Exception ex)
            {
                // Puedes loguearlo o propagarlo
                throw new Exception("Error al listar establecimientos: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Servicio para obtener el establecimiento por el id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<EstablishmentDto> GetEstablishmentByIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                using var command = new SqlCommand("sp_GetEstablishmentById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@EstablishmentId", id);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return null; // También podrías lanzar una excepción si prefieres

                EstablishmentDto establishment = null;

                if (await reader.ReadAsync())
                {
                    establishment = new EstablishmentDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("establishment_id")),
                        Name = reader.GetString(reader.GetOrdinal("establishment_name")),
                        Address = reader.IsDBNull(reader.GetOrdinal("establishment_address")) ? null : reader.GetString(reader.GetOrdinal("establishment_address")),
                        EmissionPoint = reader.IsDBNull(reader.GetOrdinal("establishment_emissionpoint")) ? null : reader.GetString(reader.GetOrdinal("establishment_emissionpoint")),
                        PointOfSale = reader.IsDBNull(reader.GetOrdinal("establishment_pointofsale")) ? null : reader.GetString(reader.GetOrdinal("establishment_pointofsale")),
                        CreationDate = reader.IsDBNull(reader.GetOrdinal("establishment_creationdate")) ? null : reader.GetDateTime(reader.GetOrdinal("establishment_creationdate")),
                        ModificationDate = reader.IsDBNull(reader.GetOrdinal("establishment_modificationdate")) ? null : reader.GetDateTime(reader.GetOrdinal("establishment_modificationdate")),
                        SequentialBilling = reader.IsDBNull(reader.GetOrdinal("establishment_sequential_billing")) ? null : reader.GetInt32(reader.GetOrdinal("establishment_sequential_billing")),
                        Logo = reader.IsDBNull(reader.GetOrdinal("establishment_logo")) ? null : (byte[])reader["establishment_logo"]
                    };
                }

                return establishment;
            }
            catch (SqlException ex)
            {
                // Captura errores lanzados con RAISERROR en el SP
                throw new ApplicationException($"Error de base de datos: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Otros errores generales
                throw new ApplicationException("Ocurrió un error al obtener el establecimiento.", ex);
            }
        }

        /// <summary>
        /// Servicio para actualizar el establecimiento
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string> UpdateEstablishmentAsync(EstablishmentDto dto)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var command = new SqlCommand("sp_UpdateEstablishment", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@EstablishmentId", dto.Id);
            command.Parameters.AddWithValue("@Name", dto.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object?)dto.Address ?? DBNull.Value);
            command.Parameters.AddWithValue("@EmissionPoint", (object?)dto.EmissionPoint ?? DBNull.Value);
            command.Parameters.AddWithValue("@PointOfSale", (object?)dto.PointOfSale ?? DBNull.Value);
            command.Parameters.AddWithValue("@SequentialBilling", (object?)dto.SequentialBilling ?? DBNull.Value);

            if (dto.Logo != null && dto.Logo.Length > 0)
                command.Parameters.AddWithValue("@Logo", dto.Logo);
            else
                command.Parameters.AddWithValue("@Logo", DBNull.Value);

            await connection.OpenAsync();

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                if (reader.Read())
                {
                    // Recupera el mensaje del SP
                    return reader["Message"]?.ToString() ?? "Actualización completada.";
                }

                return "Actualización completada sin mensaje.";
            }
            catch (SqlException ex)
            {
                // Loggear si querés
                throw new ApplicationException($"Error al actualizar el establecimiento: {ex.Message}", ex);
            }
        }


    }
}
