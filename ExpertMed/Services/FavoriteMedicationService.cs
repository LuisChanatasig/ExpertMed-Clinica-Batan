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

        public async Task<(bool isSuccess, string message)> AddFavoriteMedicationAsync(
        int doctorUserId,
        int medicationId,
        string amount,
        string observation = null)
        {
            using (var connection = new SqlConnection(_connectionString.Database.GetConnectionString()))
            using (var command = new SqlCommand("sp_AddFavoriteMedication", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@doctor_userid", doctorUserId);
                command.Parameters.AddWithValue("@medications_medicationsid", medicationId);
                command.Parameters.AddWithValue("@medications_amount", amount);
                command.Parameters.AddWithValue("@medications_observation", (object?)observation ?? DBNull.Value);

                var messageParam = new SqlParameter("@message", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(messageParam);

                var returnParam = new SqlParameter
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var isSuccess = (int)returnParam.Value == 0;
                var message = messageParam.Value.ToString();

                return (isSuccess, message);
            }
        }
    }
}
