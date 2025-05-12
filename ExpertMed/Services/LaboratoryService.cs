using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class LaboratoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppointmentService> _logger;
        private readonly DbExpertmedContext _dbContext;

        public LaboratoryService(IHttpContextAccessor httpContextAccessor, ILogger<AppointmentService> logger, DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

        }

        /// <summary>
        /// Obtner todas las solicitudes de laboratorio
        /// </summary>
        /// <param name="perfil"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>

        public async Task<List<PendingLabRequest>> GetAllLabRequestsAsync(int perfil, int usuarioId)
        {
            var result = new List<PendingLabRequest>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ListarTodosLaboratorios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Perfil", SqlDbType.Int) { Value = perfil });
                        command.Parameters.Add(new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = usuarioId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var request = new PendingLabRequest
                                {
                                    LaboratoriesId = reader.GetInt32(reader.GetOrdinal("laboratories_id")),
                                    LaboratoriesConsultationId = reader.GetInt32(reader.GetOrdinal("laboratories_consultationid")),
                                    LaboratoriesSequential = reader.GetInt32(reader.GetOrdinal("laboratories_sequential")),
                                    LaboratoriesAmount = reader["laboratories_amount"]?.ToString(),
                                    LaboratoriesObservation = reader["laboratories_observation"]?.ToString(),
                                    LaboratoriesStatus = reader.GetInt32(reader.GetOrdinal("laboratories_status")),
                                    EstadoTexto = reader["estado_texto"]?.ToString(),

                                    NombreExamen = reader["nombre_examen"]?.ToString(),
                                    FechaSolicitud = reader.GetDateTime(reader.GetOrdinal("fecha_solicitud")),

                                    NombrePaciente = reader["nombre_paciente"]?.ToString(),
                                    CedulaPaciente = reader["cedula_paciente"]?.ToString(),
                                    MedicoId = reader.GetInt32(reader.GetOrdinal("medico_id")),
                                    NombreMedico = reader["nombre_medico"]?.ToString(),
                                    CodigoPaciente = reader["codigo_paciente"]?.ToString(),
                                    NombreSeguro = reader["nombre_seguro"]?.ToString()
                                };

                                result.Add(request);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las solicitudes de laboratorio pendientes");
                throw;
            }

            return result;
        }


        /// <summary>
        /// Servicio para listar los laboratorios pendientes
        /// </summary>
        /// <param name="perfil"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task<List<PendingLabRequest>> GetPendingLabRequestsAsync(int perfil, int usuarioId)
        {
            var result = new List<PendingLabRequest>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ListarLaboratoriosPendientes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Perfil", SqlDbType.Int) { Value = perfil });
                        command.Parameters.Add(new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = usuarioId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new PendingLabRequest
                                {
                                    LaboratoriesId = reader.GetInt32(reader.GetOrdinal("laboratories_id")),
                                    LaboratoriesConsultationId = reader.GetInt32(reader.GetOrdinal("laboratories_consultationid")),
                                    LaboratoriesSequential = reader.GetInt32(reader.GetOrdinal("laboratories_sequential")),
                                    LaboratoriesAmount = reader["laboratories_amount"]?.ToString(),
                                    LaboratoriesObservation = reader["laboratories_observation"]?.ToString(),
                                    LaboratoriesStatus = reader.GetInt32(reader.GetOrdinal("laboratories_status")),
                                    EstadoTexto = reader["estado_texto"]?.ToString(),

                                    NombreExamen = reader["nombre_examen"]?.ToString(),
                                    FechaSolicitud = reader.GetDateTime(reader.GetOrdinal("fecha_solicitud")),

                                    NombrePaciente = reader["nombre_paciente"]?.ToString(),
                                    CedulaPaciente = reader["cedula_paciente"]?.ToString(),
                                    MedicoId = reader.GetInt32(reader.GetOrdinal("medico_id")),

                                    NombreMedico = reader["nombre_medico"]?.ToString(),
                                    CodigoPaciente = reader["codigo_paciente"]?.ToString(),
                                    NombreSeguro = reader["nombre_seguro"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las solicitudes de laboratorio pendientes");
                throw;
            }

            return result;
        }

        /// <summary>
        /// Servicio para  obtener el historico de los laboratorios
        /// </summary>
        /// <param name="perfil"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task<List<PendingLabRequest>> GetLabHistoryAsync(int perfil, int usuarioId)
        {
            var historial = new List<PendingLabRequest>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ListHistoryLaboratory", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Perfil", SqlDbType.Int) { Value = perfil });
                        command.Parameters.Add(new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = usuarioId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                historial.Add(new PendingLabRequest
                                {
                                    LaboratoriesId = reader.GetInt32(reader.GetOrdinal("laboratories_id")),
                                    LaboratoriesSequential = reader.GetInt32(reader.GetOrdinal("laboratories_sequential")),
                                    LaboratoriesAmount = reader["laboratories_amount"]?.ToString(),
                                    LaboratoriesObservation = reader["laboratories_observation"]?.ToString(),
                                    LaboratoriesStatus = reader.GetInt32(reader.GetOrdinal("laboratories_status")),
                                    EstadoTexto = reader["estado_texto"]?.ToString(),
                                    LaboratoriesConsultationId = reader.GetInt32(reader.GetOrdinal("laboratories_consultationid")),

                                    NombreExamen = reader["nombre_examen"]?.ToString(),
                                    FechaSolicitud = reader.GetDateTime(reader.GetOrdinal("fecha_solicitud")),

                                    NombrePaciente = reader["nombre_paciente"]?.ToString(),
                                    CedulaPaciente = reader["cedula_paciente"]?.ToString(),
                                    CodigoPaciente = reader["codigo_paciente"]?.ToString(),
                                    NombreSeguro = reader["nombre_seguro"]?.ToString(),
                                    NombreMedico = reader["nombre_medico"]?.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de laboratorios");
                throw;
            }

            return historial;
        }

        /// <summary>
        /// Servicio para cambiar el estado
        /// </summary>
        /// <param name="laboratorioId"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>
        public async Task ChangeStatusLaboratory(int laboratorioId, int nuevoEstado)
        {
            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ChangeStatusLaboratory", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LaboratorioId", laboratorioId);
                    command.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Servicio para insertar resultados de laboratorio
        /// </summary>
        /// <param name="resultados"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>

        public async Task InsertarResultadosLaboratorioAsync(List<ResultadoLaboratorioInput> resultados, int usuarioId)
        {
            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    foreach (var resultado in resultados)
                    {
                        byte[]? archivoBytes = null;

                        if (resultado.Archivo != null && resultado.Archivo.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await resultado.Archivo.CopyToAsync(memoryStream);
                            archivoBytes = memoryStream.ToArray();
                        }

                        using (var command = new SqlCommand("sp_InsertLaboratoryResult", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@LaboratoryId", resultado.LaboratoryId);
                            command.Parameters.AddWithValue("@Resultado", resultado.Resultado ?? string.Empty);
                            command.Parameters.AddWithValue("@Archivo", (object?)archivoBytes ?? DBNull.Value);
                            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar resultados de laboratorio.");
                throw;
            }
        }

        /// <summary>
        /// Servicio para descargar los resultados
        /// </summary>
        /// <param name="laboratoryId"></param>
        /// <returns></returns>
        public async Task<(byte[] FileData, string FileName)> GetResultadoArchivoPorLaboratoryIdAsync(int laboratoryId)
        {
            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetLaboratoryResultFile", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@LaboratoryId", laboratoryId);

                        using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                        {
                            if (await reader.ReadAsync())
                            {
                                var fileData = (byte[])reader["FileData"];
                                var fileName = $"resultado_{laboratoryId}.pdf"; // Puedes ajustar la extensión según tu lógica

                                return (fileData, fileName);
                            }
                            else
                            {
                                throw new Exception("No se encontró el resultado para el laboratorio especificado.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el archivo del laboratorio con ID {laboratoryId}");
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaDesde"></param>
        /// <param name="fechaHasta"></param>
        /// <param name="medicoId"></param>
        /// <param name="seguroId"></param>
        /// <returns></returns>


        public async Task<List<ReporteLaboratorioDto>> GetReporteResultadosAsync(
    DateTime? fechaDesde,
    DateTime? fechaHasta,
    int? medicoId,
    int? seguroId,
    int perfilId,
    int usuarioId)
        {
            var lista = new List<ReporteLaboratorioDto>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                using (var command = new SqlCommand("sp_ReporteResultadosLaboratorio", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@FechaDesde", (object?)fechaDesde ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FechaHasta", (object?)fechaHasta ?? DBNull.Value);
                    command.Parameters.AddWithValue("@MedicoId", (object?)medicoId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SeguroId", (object?)seguroId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PerfilId", perfilId);
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    await connection.OpenAsync();
                    using var reader = await command.ExecuteReaderAsync();

                    // Saltar los 4 primeros bloques de resultados (gráficos)
                    for (int i = 0; i < 4; i++)
                    {
                        if (!await reader.NextResultAsync())
                        {
                            _logger.LogWarning("No se encontraron suficientes bloques de resultados en el SP.");
                            return lista;
                        }
                    }

                    // Leer bloque 5 (detalle tabular)
                    while (await reader.ReadAsync())
                    {
                        var item = new ReporteLaboratorioDto
                        {
                            LaboratoryId = reader.GetInt32(reader.GetOrdinal("laboratories_id")),
                            ConsultationId = reader.GetInt32(reader.GetOrdinal("ConsultationId")),
                            FechaConsulta = reader.GetDateTime(reader.GetOrdinal("FechaConsulta")),
                            MedicoId = reader.GetInt32(reader.GetOrdinal("MedicoId")),
                            NombreMedico = reader.GetString(reader.GetOrdinal("NombreMedico")),
                            PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                            NombrePaciente = reader.GetString(reader.GetOrdinal("NombrePaciente")),
                            SeguroId = reader.IsDBNull(reader.GetOrdinal("SeguroId")) ? null : reader.GetInt32(reader.GetOrdinal("SeguroId")),
                            NombreSeguro = reader.IsDBNull(reader.GetOrdinal("NombreSeguro")) ? null : reader.GetString(reader.GetOrdinal("NombreSeguro")),
                            Resultado = reader.IsDBNull(reader.GetOrdinal("Resultado")) ? null : reader.GetString(reader.GetOrdinal("Resultado")),
                            FechaResultado = reader.IsDBNull(reader.GetOrdinal("FechaResultado")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaResultado"))
                        };

                        lista.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el reporte de laboratorio.");
                throw;
            }

            return lista;
        }


        /// <summary>
        /// Obtener datos
        /// </summary>
        /// <param name="desde"></param>
        /// <param name="hasta"></param>
        /// <param name="medicoId"></param>
        /// <param name="seguroId"></param>
        /// <returns></returns>
        public async Task<ReporteChartDto> GetChartDataAsync(DateTime? desde, DateTime? hasta, int? medicoId, int? seguroId, int perfilId, int usuarioId)
        {
            var dto = new ReporteChartDto
            {
                Estados = new List<EstadoChartItem>(),
                Meses = new List<string>(),
                ValoresPorMes = new List<int>(),
                TopMedicos = new List<MedicoChartItem>(),
                Pendientes = new List<int>(),
                Finalizados = new List<int>()
            };

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ReporteResultadosLaboratorio", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@fechaDesde", (object?)desde ?? DBNull.Value);
                    command.Parameters.AddWithValue("@fechaHasta", (object?)hasta ?? DBNull.Value);
                    command.Parameters.AddWithValue("@medicoId", (object?)medicoId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@seguroId", (object?)seguroId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@perfilId", perfilId);
                    command.Parameters.AddWithValue("@usuarioId", usuarioId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // 1. Estados
                        while (await reader.ReadAsync())
                        {
                            dto.Estados.Add(new EstadoChartItem
                            {
                                Nombre = reader["estado_nombre"].ToString(),
                                Valor = Convert.ToInt32(reader["cantidad"])
                            });
                        }

                        // 2. Barras por mes
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dto.Meses.Add(reader["mes"].ToString());
                                dto.ValoresPorMes.Add(Convert.ToInt32(reader["total"]));
                            }
                        }

                        // 3. Top médicos
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dto.TopMedicos.Add(new MedicoChartItem
                                {
                                    Nombre = reader["medico"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["cantidad"])
                                });
                            }
                        }

                        // 4. Comparativo Pendientes vs Finalizados
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                dto.Pendientes.Add(Convert.ToInt32(reader["pendientes"]));
                                dto.Finalizados.Add(Convert.ToInt32(reader["finalizados"]));

                                var mes = reader["mes"].ToString();
                                if (!dto.Meses.Contains(mes))
                                    dto.Meses.Add(mes);
                            }
                        }
                    }
                }
            }

            return dto;
        }

    }
}
