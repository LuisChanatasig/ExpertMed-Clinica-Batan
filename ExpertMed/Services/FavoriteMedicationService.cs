using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class FavoriteMedicationService
    {
        private readonly DbExpertmedContext _connectionString;

        private readonly ILogger<FavoriteMedicationService> _logger;
        public FavoriteMedicationService(DbExpertmedContext connectionString, ILogger<FavoriteMedicationService> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        /// <summary>
        /// Save or update a favorite medication for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="medicationId"></param>
        /// <param name="cantidad"></param>
        /// <param name="observacion"></param>
        /// <returns></returns>
        public async Task AddOrUpdateFavoriteAsync(int userId, int medicationId, string cantidad, string observacion)
        {
            using var conn = new SqlConnection(_connectionString.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_AddOrUpdateFavoriteMedication", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@users_id", userId);
            cmd.Parameters.AddWithValue("@medications_id", medicationId);
            cmd.Parameters.AddWithValue("@cantidad", cantidad ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@observacion", observacion ?? (object)DBNull.Value);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Remove a favorite medication for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="medicationId"></param>
        /// <returns></returns>
        public async Task RemoveFavoriteAsync(int userId, int medicationId)
        {
            using var conn = new SqlConnection(_connectionString.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_RemoveFavoriteMedication", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@users_id", userId);
            cmd.Parameters.AddWithValue("@medications_id", medicationId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Get a list of favorite medications for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<FavoriteMedicationViewModel>> GetFavoritesAsync(int userId)
        {
            var list = new List<FavoriteMedicationViewModel>();

            using var conn = new SqlConnection(_connectionString.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetFavoriteMedications", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@users_id", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new FavoriteMedicationViewModel
                {
                    FavoriteId = reader.GetInt32(reader.GetOrdinal("favorite_id")),
                    MedicationId = reader.GetInt32(reader.GetOrdinal("medications_id")),
                    MedicationName = reader.GetString(reader.GetOrdinal("medications_name")),
                    Cantidad = reader.IsDBNull(reader.GetOrdinal("cantidad")) ? null : reader.GetString(reader.GetOrdinal("cantidad")),
                    Observacion = reader.IsDBNull(reader.GetOrdinal("observacion")) ? null : reader.GetString(reader.GetOrdinal("observacion")),
                    CreationDate = reader.GetDateTime(reader.GetOrdinal("creation_date"))
                });
            }

            return list;
        }
    }
}
