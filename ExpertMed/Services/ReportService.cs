using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class ReportService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportService> _logger; // Corregí el tipo de ILogger
        private readonly DbExpertmedContext _dbContext;

        public ReportService(IHttpContextAccessor httpContextAccessor, ILogger<ReportService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResumenConsultasDto> GetResumenConsultasAsync(DateTime? fechaDesde, DateTime? fechaHasta, int perfilId, int usuarioId)
        {
            var resumen = new ResumenConsultasDto();

            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                using var command = new SqlCommand("sp_ReporteResumenConsultas", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@fechaDesde", (object?)fechaDesde ?? DBNull.Value);
                command.Parameters.AddWithValue("@fechaHasta", (object?)fechaHasta ?? DBNull.Value);
                command.Parameters.AddWithValue("@perfilId", perfilId);
                command.Parameters.AddWithValue("@usuarioId", usuarioId);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                // ---------------------------------------------------------
                // 1. LECTURA DE KPIs (Índices actualizados según el nuevo SP)
                // ---------------------------------------------------------
                if (await reader.ReadAsync())
                {
                    resumen.Kpi = new ResumenConsultasDto.DashboardKpi
                    {
                        TotalCitas = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        TotalConsultas = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        // NUEVOS CAMPOS:
                        TotalAsistidos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                        TotalNoAsistidos = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        // DESPLAZADOS:
                        TotalPagadas = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        TotalPacientesHistorico = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                    };
                }

                // ---------------------------------------------------------
                // 2. LECTURA DE EVOLUCIÓN DIARIA
                // ---------------------------------------------------------
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resumen.EvolucionDiaria.Add(new ResumenConsultasDto.DashboardEvolutionItem
                        {
                            Fecha = reader.GetDateTime(0),
                            CantidadCitas = reader.GetInt32(1)
                        });
                    }
                }

                // ---------------------------------------------------------
                // 3. LECTURA DE ESTADO DE CITAS
                // ---------------------------------------------------------
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resumen.EstadoCitas.Add(new ResumenConsultasDto.DashboardStatusItem
                        {
                            Estado = reader.GetString(0),
                            Cantidad = reader.GetInt32(1)
                        });
                    }
                }

                // ---------------------------------------------------------
                // 4. LECTURA DE RANKING MÉDICOS
                // ---------------------------------------------------------
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resumen.RankingMedicos.Add(new ResumenConsultasDto.DashboardDoctorItem
                        {
                            Medico = reader.GetString(0),
                            ConsultasRealizadas = reader.GetInt32(1)
                        });
                    }
                }

                // ---------------------------------------------------------
                // 5. LECTURA DE PACIENTES POR SEGURO
                // ---------------------------------------------------------
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resumen.PacientesPorSeguro.Add(new ResumenConsultasDto.DashboardInsuranceItem
                        {
                            Seguro = reader.GetString(0),
                            CantidadPacientesUnicos = reader.GetInt32(1)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el reporte de dashboard (sp_ReporteResumenConsultas).");
                throw;
            }

            return resumen;
        }
    }
}