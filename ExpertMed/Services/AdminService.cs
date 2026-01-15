using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class AdminService
    {
        private readonly DbExpertmedContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminService> _logger;
        private readonly HttpClient _httpClient;

        public AdminService(DbExpertmedContext context, IHttpContextAccessor httpContextAccessor, ILogger<AdminService> logger, HttpClient httpClient)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Inserta un nuevo item en el catálogo general mediante Store Procedure sin Dapper.
        /// </summary>
        public async Task<bool> InsertCatalogItemAsync(string name, string category, int status = 1)
        {
            try
            {
                // Definición de parámetros para evitar SQL Injection
                var paramName = new SqlParameter("@Name", name ?? (object)DBNull.Value);
                var paramCategory = new SqlParameter("@Category", category ?? (object)DBNull.Value);
                var paramStatus = new SqlParameter("@Status", status);

                // Ejecución directa a través del contexto de EF Core
                // ExecuteSqlRawAsync devuelve el número de filas afectadas
                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_InsertCatalogItem @Name, @Category, @Status",
                    paramName, paramCategory, paramStatus
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en InsertCatalogItemAsync: No se pudo insertar {Name} en la categoría {Category}", name, category);
                return false;
            }
        }
    }
}