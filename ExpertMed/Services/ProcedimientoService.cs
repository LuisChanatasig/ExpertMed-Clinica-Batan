using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class ProcedimientoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ProcedimientoService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public ProcedimientoService(IHttpContextAccessor httpContextAccessor, ILogger<ProcedimientoService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<List<ProcedimientoDto>> BuscarProcedimientosAsync(string termino, int insuranceCompanyId)
        {
            try
            {
                // Validación inicial
                if (string.IsNullOrWhiteSpace(termino) || insuranceCompanyId <= 0)
                    return new List<ProcedimientoDto>();

                var result = new List<ProcedimientoDto>();

                // Parámetros para el SP
                var parametros = new[]
                {
                new SqlParameter("@termino", SqlDbType.NVarChar, 255) { Value = termino },
                new SqlParameter("@insuranceCompanyId", SqlDbType.Int) { Value = insuranceCompanyId }
            };

                // Ejecutar el stored procedure usando FromSqlRaw
                var procedimientos = await _dbContext.Set<ProcedimientoDto>()
                    .FromSqlRaw("EXEC sp_BuscarProcedimientos @termino, @insuranceCompanyId", parametros)
                    .ToListAsync();

                return procedimientos;
            }
            catch (Exception ex)
            {
                // Log del error (usar tu sistema de logging preferido)
                throw new ApplicationException($"Error al buscar procedimientos: {ex.Message}", ex);
            }
        }

        // Alternativa usando ADO.NET directo para mayor control
        public async Task<List<ProcedimientoDto>> BuscarProcedimientosAdoAsync(string termino, int insuranceCompanyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || insuranceCompanyId <= 0)
                    return new List<ProcedimientoDto>();

                var result = new List<ProcedimientoDto>();

                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "sp_BuscarProcedimientos";
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros
                        var param1 = new SqlParameter("@termino", SqlDbType.NVarChar, 255) { Value = termino };
                        var param2 = new SqlParameter("@insuranceCompanyId", SqlDbType.Int) { Value = insuranceCompanyId };

                        command.Parameters.Add(param1);
                        command.Parameters.Add(param2);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new ProcedimientoDto
                                {
                                    Id = reader.GetInt32("id"),
                                    Text = reader.GetString("text"),
                                    Precio = reader.GetDecimal("precio"),
                                    PrecioAseguradora = reader.GetDecimal("precio_aseguradora")
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al buscar procedimientos: {ex.Message}", ex);
            }
        }
    }
}
