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


    }
}
