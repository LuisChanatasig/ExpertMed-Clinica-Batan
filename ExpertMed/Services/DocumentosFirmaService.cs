using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;

namespace ExpertMed.Services
{
    public class DocumentosFirmaService
    {
        private readonly DbExpertmedContext _dbContext; // Tu nombre de contexto        private readonly ILogger<DocumentosFirmaService> _logger;
        private readonly ILogger<DocumentosFirmaService> _logger;
        public DocumentosFirmaService(DbExpertmedContext dbContext, ILogger<DocumentosFirmaService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<PacienteDocumentosAgrupadoDto>> GetPacientesDocumentosAgrupadosAsync()
        {
            var resultado = new List<PacienteDocumentosAgrupadoDto>();

            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                using var command = new SqlCommand("sp_ListarPacientesDocumentosAgrupados", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    resultado.Add(new PacienteDocumentosAgrupadoDto
                    {
                        PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                        Paciente = reader.GetString(reader.GetOrdinal("Paciente")),
                        // SQL devuelve NULL si no hay documentos en el subquery
                        MisDocumentos = reader.IsDBNull(reader.GetOrdinal("MisDocumentos"))
                                        ? "[]"
                                        : reader.GetString(reader.GetOrdinal("MisDocumentos"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar sp_ListarPacientesDocumentosAgrupados");
                throw;
            }

            return resultado;
        }
    }
}