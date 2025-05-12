using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class MedicalOfficeService
    {

        private readonly DbExpertmedContext _dbContext;

        public MedicalOfficeService(DbExpertmedContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Servicio para crear un nuevo consultorio
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="establishmentId"></param>
        /// <returns></returns>
        public async Task<int> CreateMedicalOfficeAsync(MedicalOfficeCreateDto model, int userId, int establishmentId)
        {
            int newMedicalOfficeId;

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_CreateMedicalOffice", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@EstablishmentId", SqlDbType.Int) { Value = establishmentId });
                    command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = model.Name });
                    command.Parameters.Add(new SqlParameter("@Location", SqlDbType.NVarChar, 150) { Value = (object?)model.Location ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 255) { Value = (object?)model.Description ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@CreateBy", SqlDbType.Int) { Value = userId });

                    // Ejecutar y obtener el ID generado
                    var result = await command.ExecuteScalarAsync();
                    newMedicalOfficeId = Convert.ToInt32(result);
                }
            }

            return newMedicalOfficeId;
        }

        /// <summary>
        /// Servicio para listar los consultorios
        /// </summary>
        /// <param name="perfilId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task<List<MedicalOfficeListDto>> GetMedicalOfficesAsync(int perfilId, int usuarioId)
        {
            var medicalOffices = new List<MedicalOfficeListDto>();

            using (var connection = new SqlConnection(_dbContext.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ListMedicalOffices", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PerfilId", perfilId);
                    command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            medicalOffices.Add(new MedicalOfficeListDto
                            {
                                MedicalOfficeId = reader.GetInt32(reader.GetOrdinal("medicaloffice_id")),
                                EstablishmentId = reader.GetInt32(reader.GetOrdinal("medicaloffice_establishmentid")),
                                EstablishmentName = reader.GetString(reader.GetOrdinal("establishment")),
                                Name = reader.GetString(reader.GetOrdinal("medicaloffice_name")),
                                Location = reader.IsDBNull(reader.GetOrdinal("medicaloffice_location")) ? null : reader.GetString(reader.GetOrdinal("medicaloffice_location")),
                                Description = reader.IsDBNull(reader.GetOrdinal("medicaloffice_description")) ? null : reader.GetString(reader.GetOrdinal("medicaloffice_description")),
                                Status = reader.GetBoolean(reader.GetOrdinal("medicaloffice_status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("medicaloffice_createat")),
                                CreatedByUserId = reader.GetInt32(reader.GetOrdinal("medicaloffice_createby")),
                                CreatedByName = reader.GetString(reader.GetOrdinal("creado_por"))
                            });
                        }
                    }
                }
            }

            return medicalOffices;
        }

        /// <summary>
        /// Metodo para obtener por id
        /// </summary>
        /// <param name="medicalOfficeId"></param>
        /// <returns></returns>
        public async Task<MedicalOfficeListDto?> GetMedicalOfficeByIdAsync(int medicalOfficeId)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_GetMedicalOfficeById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@MedicalOfficeId", medicalOfficeId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MedicalOfficeListDto
                {
                    MedicalOfficeId = reader.GetInt32(reader.GetOrdinal("medicaloffice_id")),
                    EstablishmentId = reader.GetInt32(reader.GetOrdinal("medicaloffice_establishmentid")),

                    Name = reader.GetString(reader.GetOrdinal("medicaloffice_name")),
                    Location = reader.IsDBNull(reader.GetOrdinal("medicaloffice_location")) ? null : reader.GetString(reader.GetOrdinal("medicaloffice_location")),
                    Description = reader.IsDBNull(reader.GetOrdinal("medicaloffice_description")) ? null : reader.GetString(reader.GetOrdinal("medicaloffice_description")),
                    Status = reader.GetBoolean(reader.GetOrdinal("medicaloffice_status"))
                };
            }

            return null;
        }

        /// <summary>
        /// Metodo para actualizar el consultorio
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task UpdateMedicalOfficeAsync(MedicalOfficeListDto model)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_UpdateMedicalOffice", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@MedicalOfficeId", model.MedicalOfficeId);
            command.Parameters.AddWithValue("@Name", model.Name);
            command.Parameters.AddWithValue("@Location", (object?)model.Location ?? DBNull.Value);
            command.Parameters.AddWithValue("@Description", (object?)model.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", model.Status);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Servicio que consume el sp
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assignedBy"></param>
        /// <param name="consultorioIds"></param>
        /// <returns></returns>
        public async Task<bool> AssignConsultoriosAsync(int userId, int assignedBy, List<int> consultorioIds)
        {
            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_AsignarConsultorios", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                command.Parameters.Add(new SqlParameter("@AssignedBy", SqlDbType.Int) { Value = assignedBy });

                // Construir el DataTable para el TVP
                var tvp = new DataTable();
                tvp.Columns.Add("Id", typeof(int));
                foreach (var id in consultorioIds)
                    tvp.Rows.Add(id);

                var tvpParam = new SqlParameter("@ConsultoriosIds", SqlDbType.Structured)
                {
                    TypeName = "dbo.IdList",
                    Value = tvp
                };
                command.Parameters.Add(tvpParam);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception)
            {
                // Puedes loggear el error si quieres
                return false;
            }
        }

        /// <summary>
        /// Servicio para traer las asignaciones
        /// </summary>
        /// <param name="perfilId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        public async Task<List<AssignedOfficeDto>> GetAssignedOfficesAsync(int perfilId, int usuarioId)
        {
            var lista = new List<AssignedOfficeDto>();

            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_ListAssignedOffices", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PerfilId", perfilId);
                command.Parameters.AddWithValue("@UsuarioId", usuarioId);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new AssignedOfficeDto
                    {
                     
                        UserId = reader.GetInt32(reader.GetOrdinal("users_id")),
                        Medico = reader.GetString(reader.GetOrdinal("medico")),
                        Consultorio = reader.GetString(reader.GetOrdinal("consultorios")),
                        PrimeraAsignacion = reader.GetDateTime(reader.GetOrdinal("primera_asignacion")),
                        EstadoAsignacion = reader.GetString(reader.GetOrdinal("estado_asignacion"))
                    });
                }
            }
            catch (Exception)
            {
                // Manejo de errores opcional: loggear o lanzar
            }

            return lista;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AssignedOfficeDto> GetAsignacionByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_GetAssignmentByUserId", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);

            var consultorioIds = new List<int>();
            bool status = true;
            int assignmentId = 0;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                assignmentId = reader.GetInt32(reader.GetOrdinal("assignment_id")); // opcional, si lo necesitas
                status = reader.GetBoolean(reader.GetOrdinal("assignment_status"));
                consultorioIds.Add(reader.GetInt32(reader.GetOrdinal("medical_office_id")));
            }

            return new AssignedOfficeDto
            {
                AssignmentId = assignmentId,
                UserId = userId,
                AssignmentStatus = status,
                ConsultorioIds = consultorioIds
            };
        }

        /// <summary>
        /// Actualizar infromacion
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="medicalOffices"></param>
        /// <param name="assignmentStatus"></param>
        /// <returns></returns>
        public async Task ActualizarAsignacionAsync(int userId, List<int> medicalOffices, bool assignmentStatus)
        {
            using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_UpdateAssignmentMultiple", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@AssignmentStatus", assignmentStatus);

            // Crear el DataTable para el tipo IdList
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));

            foreach (var id in medicalOffices)
            {
                table.Rows.Add(id);
            }

            var tvpParam = new SqlParameter("@MedicalOffices", SqlDbType.Structured)
            {
                TypeName = "dbo.IdList",
                Value = table
            };

            command.Parameters.Add(tvpParam);

            await command.ExecuteNonQueryAsync();
        }

    }
}
