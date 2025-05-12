using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class ImagesService
    {
        private readonly DbExpertmedContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ImagesService> _logger;

        public ImagesService(DbExpertmedContext dbContext, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<ImagesService> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de imágenes pendientes para un perfil y usuario específicos.
        /// </summary>
        /// <param name="perfilId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ImagenPendienteDto>> GetImagenesPendientesAsync(int perfilId, int usuarioId)
        {
            var lista = new List<ImagenPendienteDto>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                using (var command = new SqlCommand("sp_ListarImagenesPendientes", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Perfil", perfilId);
                    command.Parameters.AddWithValue("@UsuarioID", usuarioId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ImagenPendienteDto
                            {
                                ImagesId = reader.GetInt32(reader.GetOrdinal("images_id")),
                                ImagesSequential = reader.GetInt32(reader.GetOrdinal("images_sequential")),
                                ImagesAmount = reader.GetString(reader.GetOrdinal("images_amount")),
                                ImagesObservation = reader.GetString(reader.GetOrdinal("images_observation")),
                                ImagesStatus = reader.GetInt32(reader.GetOrdinal("images_status")),
                                ImagesConsultationId = reader.GetInt32(reader.GetOrdinal("images_consultationid")),
                                EstadoTexto = reader.GetString(reader.GetOrdinal("estado_texto")),

                                NombreExamen = reader.GetString(reader.GetOrdinal("nombre_examen")),
                                FechaSolicitud = reader.GetDateTime(reader.GetOrdinal("fecha_solicitud")),

                                NombrePaciente = reader.GetString(reader.GetOrdinal("nombre_paciente")),
                                CedulaPaciente = reader.GetString(reader.GetOrdinal("cedula_paciente")),
                                CodigoPaciente = reader.GetString(reader.GetOrdinal("codigo_paciente")),
                                SeguroId = reader.IsDBNull(reader.GetOrdinal("patient_healt_insurance"))
                                           ? null
                                           : reader.GetInt32(reader.GetOrdinal("patient_healt_insurance")),
                                NombreSeguro = reader.IsDBNull(reader.GetOrdinal("nombre_seguro"))
                                               ? null
                                               : reader.GetString(reader.GetOrdinal("nombre_seguro")),

                                MedicoId = reader.GetInt32(reader.GetOrdinal("medico_id")),
                                NombreMedico = reader.GetString(reader.GetOrdinal("nombre_medico"))
                            };

                            lista.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores según tu política
                throw new Exception("Error al obtener las imágenes pendientes.", ex);
            }

            return lista;
        }

        /// <summary>
        /// Obtiene el historial de imágenes para un perfil y usuario específicos.
        /// </summary>
        /// <param name="perfilId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<List<ImagenPendienteDto>> GetHistorialImagenesAsync(int perfilId, int usuarioId)
        {
            var lista = new List<ImagenPendienteDto>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                using (var command = new SqlCommand("sp_ListHistoryImages", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Perfil", perfilId);
                    command.Parameters.AddWithValue("@UsuarioID", usuarioId);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ImagenPendienteDto
                            {
                                ImagesId = reader.GetInt32(reader.GetOrdinal("images_id")),
                                ImagesSequential = reader.GetInt32(reader.GetOrdinal("images_sequential")),
                                ImagesAmount = reader.GetString(reader.GetOrdinal("images_amount")),
                                ImagesObservation = reader.GetString(reader.GetOrdinal("images_observation")),
                                ImagesStatus = reader.GetInt32(reader.GetOrdinal("images_status")),
                                ImagesConsultationId = reader.GetInt32(reader.GetOrdinal("images_consultationid")),
                                EstadoTexto = reader.GetString(reader.GetOrdinal("estado_texto")),

                                NombreExamen = reader.GetString(reader.GetOrdinal("nombre_examen")),
                                FechaSolicitud = reader.GetDateTime(reader.GetOrdinal("fecha_solicitud")),

                                NombrePaciente = reader.GetString(reader.GetOrdinal("nombre_paciente")),
                                CedulaPaciente = reader.GetString(reader.GetOrdinal("cedula_paciente")),
                                CodigoPaciente = reader.GetString(reader.GetOrdinal("codigo_paciente")),
                                NombreSeguro = reader.IsDBNull(reader.GetOrdinal("nombre_seguro"))
                                               ? null
                                               : reader.GetString(reader.GetOrdinal("nombre_seguro")),
                                NombreMedico = reader.GetString(reader.GetOrdinal("nombre_medico"))
                            };

                            lista.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el historial de imágenes.", ex);
            }

            return lista;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagenId"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<bool> ChangeEstadoImagenAsync(int imagenId, int nuevoEstado)
        {
            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                using (var command = new SqlCommand("sp_ChangeStatusImages", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ImagenId", imagenId);
                    command.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (SqlException ex)
            {
                // Puedes loguear o manejar errores específicos por código de error SQL
                throw new Exception("Error al cambiar el estado de la imagen.", ex);
            }
        }

        /// <summary>
        /// Obtiene todas las solicitudes de imágenes para un perfil y usuario específicos.
        /// </summary>
        /// <param name="perfil"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task<List<ImagenPendienteDto>> GetAllImageRequestsAsync(int perfil, int usuarioId)
        {
            var result = new List<ImagenPendienteDto>();

            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ListarTodasImagenes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@Perfil", SqlDbType.Int) { Value = perfil });
                        command.Parameters.Add(new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = usuarioId });

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var request = new ImagenPendienteDto
                                {
                                    ImagesId = reader.GetInt32(reader.GetOrdinal("images_id")),
                                    ImagesConsultationId = reader.GetInt32(reader.GetOrdinal("images_consultationid")),
                                    ImagesSequential = reader.GetInt32(reader.GetOrdinal("images_sequential")),
                                    ImagesAmount = reader["images_amount"]?.ToString(),
                                    ImagesObservation = reader["images_observation"]?.ToString(),
                                    ImagesStatus = reader.GetInt32(reader.GetOrdinal("images_status")),
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
                _logger.LogError(ex, "Error al obtener las solicitudes de imágenes");
                throw;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resultados"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task InsertarResultadosImagenesAsync(List<ResultadoImagenInput> resultados, int usuarioId)
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

                        using (var command = new SqlCommand("sp_InsertImageResult", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@ImageId", resultado.ImageId);
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
                _logger.LogError(ex, "Error al insertar resultados de imágenes.");
                throw;
            }
        }

        /// <summary>
        /// Servicio para descargar los resultados
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        public async Task<(byte[] FileData, string FileName)> GetResultadoArchivoPorImageIdAsync(int imageId)
        {
            try
            {
                using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetImageResultFile", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ImageId", imageId);

                        using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                        {
                            if (await reader.ReadAsync())
                            {
                                var fileData = (byte[])reader["FileData"];
                                var fileName = $"resultado_imagen_{imageId}.pdf"; // Puedes adaptar extensión si deseas

                                return (fileData, fileName);
                            }
                            else
                            {
                                throw new Exception("No se encontró el resultado para la imagen especificada.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el archivo de imagen con ID {imageId}");
                throw;
            }
        }


    }
}
