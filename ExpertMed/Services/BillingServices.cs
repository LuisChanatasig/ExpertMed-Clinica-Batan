using ExpertMed.Models;
using Microsoft.Data.SqlClient; // Asegúrate de tener este using
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;
namespace ExpertMed.Services
{
    public class BillingServices
    {
        private readonly DbExpertmedContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BillingServices> _logger;
        private readonly HttpClient _httpClient;

        public BillingServices(DbExpertmedContext context, IHttpContextAccessor httpContextAccessor, ILogger<BillingServices> logger, HttpClient httpClient)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClient; // HttpClient inyectado
        }

        public async Task<string> CreateAndSendInvoiceAsync(
        int citaId, DateTime fechaFacturacion, decimal totalFactura, string metodoPago, byte[] comprobantePagoFacturacion,
        string billingDetailsNames, string billingDetailsCiNumber, string billingDetailsDocumentType,
        string billingDetailsAddress, string billingDetailsPhone, string billingDetailsEmail)
        {
            string jsonFactura = string.Empty;
            string xKey = string.Empty;
            string xPassword = string.Empty;

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_billing", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar parámetros al comando
                        command.Parameters.AddWithValue("@CitaId", citaId);
                        command.Parameters.AddWithValue("@FechaFacturacion", fechaFacturacion);
                        command.Parameters.AddWithValue("@TotalFactura", totalFactura);
                        command.Parameters.AddWithValue("@MetodoPago", metodoPago);
                        command.Parameters.AddWithValue("@ComprobantePago", (object)comprobantePagoFacturacion ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_names", (object)billingDetailsNames ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_cinumber", (object)billingDetailsCiNumber ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_documenttype", (object)billingDetailsDocumentType ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_address", (object)billingDetailsAddress ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_phone", (object)billingDetailsPhone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@billing_details_email", (object)billingDetailsEmail ?? DBNull.Value);

                        // Ejecutar el procedimiento almacenado y obtener el JSON de la factura
                        jsonFactura = (string)await command.ExecuteScalarAsync();
                    }
                    // Imprimir el JSON en la consola
                    Console.WriteLine("JSON de la factura:");
                    Console.WriteLine(jsonFactura);
                    // Obtener credenciales para la API
                    using (var command = new SqlCommand("SELECT users_xkeytaxo, users_xpasstaxo FROM users WHERE users_id = (SELECT appointment_createuser FROM appointment WHERE appointment_id = @CitaId)", connection))
                    {
                        command.Parameters.AddWithValue("@CitaId", citaId);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                xKey = reader["users_xkeytaxo"].ToString();
                                xPassword = reader["users_xpasstaxo"].ToString();
                            }
                        }
                    }
                }

                // Enviar la factura a Datil
                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://link.datil.co/invoices/issue"))
                {
                    request.Content = new StringContent(jsonFactura, Encoding.UTF8, "application/json");
                    request.Headers.Add("X-Key", xKey);
                    request.Headers.Add("X-Password", xPassword);

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores centralizado
                Console.WriteLine($"Error en CreateAndSendInvoiceAsync: {ex.Message}");
                throw;
            }
        }



    }
}
