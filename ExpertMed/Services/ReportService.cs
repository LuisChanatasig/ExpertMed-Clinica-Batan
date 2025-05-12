using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class ReportService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PatientService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public ReportService(IHttpContextAccessor httpContextAccessor, ILogger<PatientService> logger, DbExpertmedContext dbContext)
        {

            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

        }

        public async Task<ResumenConsultasDto> GetResumenConsultasAsync(
          DateTime? fechaDesde, DateTime? fechaHasta, int perfilId, int usuarioId)
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

                // 1. Ocupación por consultorio
                while (await reader.ReadAsync())
                {
                    resumen.OcupacionConsultorios.Add(new ResumenConsultasDto.OcupacionConsultorioItem
                    {
                        Consultorio = reader.GetString(0),
                        MedicosAsignados = reader.GetInt32(1)
                    });
                }

                // 2. Médico con mayor índice
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    resumen.MedicoTop = new ResumenConsultasDto.MedicoTopItem
                    {
                        Medico = reader.GetString(0),
                        TotalConsultas = reader.GetInt32(1)
                    };
                }

                // 3. Tipo de paciente
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        resumen.TiposPacientes.Add(new ResumenConsultasDto.TipoPacienteItem
                        {
                            TipoPaciente = reader.GetString(0),
                            Total = reader.GetInt32(1)
                        });
                    }
                }

                // 4. Nombre del establecimiento (solo si perfil 4)
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    resumen.NombreEstablecimiento = reader.GetString(0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el resumen de consultas.");
                throw;
            }

            return resumen;
        }



    }
}
