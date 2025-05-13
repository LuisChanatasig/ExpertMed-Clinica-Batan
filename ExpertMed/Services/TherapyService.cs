using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class TherapyService
    {

        private readonly DbExpertmedContext _dbContext;
        private readonly ILogger<TherapyService> _logger;

        public TherapyService(DbExpertmedContext dbContext, ILogger<TherapyService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task GuardarTerapiasAsync(TherapySubmissionDto model, int creationUser)
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("therapy_type", typeof(string));
                dataTable.Columns.Add("session_date", typeof(DateTime));
                dataTable.Columns.Add("session_time", typeof(TimeSpan));
                dataTable.Columns.Add("session_count", typeof(int));
                dataTable.Columns.Add("observations", typeof(string));

                foreach (var sesion in model.Sesiones)
                {
                    if (!DateTime.TryParse(sesion.Fecha, out var fecha))
                        throw new Exception($"Fecha inválida: {sesion.Fecha}");

                    if (!TimeSpan.TryParse(sesion.Hora, out var hora))
                        throw new Exception($"Hora inválida: {sesion.Hora}");

                    dataTable.Rows.Add(
                        sesion.Tipo,
                        fecha,
                        hora,
                        sesion.SesionesCont,
                        sesion.Observaciones ?? string.Empty
                    );
                }

                var parameters = new[]
                {
            new SqlParameter("@patient_id", model.PacienteId),
            new SqlParameter("@therapist_id", model.TerapeutaId),
            new SqlParameter("@creation_user", creationUser),
            new SqlParameter("@sesiones", SqlDbType.Structured)
            {
                TypeName = "dbo.TerapiaSesionTipo",
                Value = dataTable
            }
        };

                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_Savetherapyrequests", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(parameters);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar terapias");
                throw;
            }
        }


        public async Task<List<TherapyHistoryDTO>> ObtenerHistorialTerapiasAsync(int perfilId, int usuarioId)
        {
            var listaPlano = new List<TherapyRawDTO>();

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var command = new SqlCommand("sp_HistorialTerapias", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@perfil_id", perfilId);
                command.Parameters.AddWithValue("@usuario_id", usuarioId);

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        listaPlano.Add(new TherapyRawDTO
                        {
                            CedulaPaciente = reader["CedulaPaciente"].ToString(),
                            NombrePaciente = reader["NombrePaciente"].ToString(),
                            NombreSeguro = reader["NombreSeguro"].ToString(),

                            RequestId = Convert.ToInt32(reader["RequestId"]),
                            FechaSolicitud = Convert.ToDateTime(reader["FechaSolicitud"]),
                            TerapeutaNombre = reader["TerapeutaNombre"].ToString(),

                            SessionId = Convert.ToInt32(reader["SessionId"]),
                            FechaSesion = Convert.ToDateTime(reader["FechaSesion"]),
                            HoraSesion = (TimeSpan)reader["HoraSesion"],
                            Tipo = reader["therapy_type"].ToString(),
                            Observaciones = reader["observations"].ToString(),
                            AvancePorcentaje = Convert.ToInt32(reader["AvancePorcentaje"]),
                            AvanceNotas = reader["AvanceNotas"].ToString()
                        });
                    }
                }
            }

            // Agrupar la data jerárquicamente
            var resultado = listaPlano
                .GroupBy(p => new { p.CedulaPaciente, p.NombrePaciente, p.NombreSeguro })
                .Select(g => new TherapyHistoryDTO
                {
                    CedulaPaciente = g.Key.CedulaPaciente,
                    NombrePaciente = g.Key.NombrePaciente,
                    NombreSeguro = g.Key.NombreSeguro,
                    Solicitudes = g
                        .GroupBy(s => new { s.RequestId, s.FechaSolicitud, s.TerapeutaNombre })
                        .Select(sg => new TherapyRequestDTO
                        {
                            RequestId = sg.Key.RequestId,
                            FechaSolicitud = sg.Key.FechaSolicitud,
                            TerapeutaNombre = sg.Key.TerapeutaNombre,
                            Sesiones = sg.Select(s => new TherapySessionDTO
                            {
                                SessionId = s.SessionId,
                                Fecha = s.FechaSesion,
                                Hora = s.HoraSesion,
                                Tipo = s.Tipo,
                                Observaciones = s.Observaciones,
                                AvancePorcentaje = s.AvancePorcentaje,
                                AvanceNotas = s.AvanceNotas
                            }).ToList()
                        }).ToList()
                }).ToList();

            return resultado;
        }


        public async Task ActualizarAvanceSesionAsync(int sessionId, int porcentaje, string notas, int userId)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var command = new SqlCommand("sp_UpdateAvanceSesion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@session_id", sessionId);
            command.Parameters.AddWithValue("@avance_porcentaje", porcentaje);
            command.Parameters.AddWithValue("@avance_notas", string.IsNullOrWhiteSpace(notas) ? (object)DBNull.Value : notas);
            command.Parameters.AddWithValue("@modification_user", userId);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

    }
}
