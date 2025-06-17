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
         string billingDetailsAddress, string billingDetailsPhone, string billingDetailsEmail,
         List<BillingItemDTO> items)
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

                        // Crear DataTable para los items
                        var table = new DataTable();
                        table.Columns.Add("billing_item_code", typeof(string));
                        table.Columns.Add("billing_item_description", typeof(string));
                        table.Columns.Add("billing_item_quantity", typeof(int));
                        table.Columns.Add("billing_item_unit_price", typeof(decimal));

                        foreach (var item in items)
                        {
                            table.Rows.Add(item.Code ?? string.Empty, item.Description ?? string.Empty, item.Quantity, item.UnitPrice);
                        }

                        var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
                        {
                            TypeName = "dbo.BillingItemsType",
                            Value = table
                        };

                        command.Parameters.Add(itemsParam);

                        jsonFactura = (string)await command.ExecuteScalarAsync();
                    }

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
                Console.WriteLine($"Error en CreateAndSendInvoiceAsync: {ex.Message}");
                throw;
            }
        }



        public async Task<AppointmentBillingDTO?> GetAppointmentBillingDataAsync(int appointmentId)
        {
            var parameters = new[]
            {
        new SqlParameter("@appointment_id", appointmentId)
    };

            var cita = _context
                .Set<AppointmentBillingDTO>()
                .FromSqlRaw("EXEC sp_GetAppointmentBillingData @appointment_id", parameters)
                .AsEnumerable()
                .FirstOrDefault();

            return await Task.FromResult(cita);
        }


        public async Task<List<FacturaEmitidaDTO>> ObtenerFacturasEmitidasAsync()
        {
            var facturas = new List<FacturaEmitidaDTO>();

            using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ListarFacturasEmitidas", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            facturas.Add(new FacturaEmitidaDTO
                            {
                                FacturaId = reader.GetInt32(reader.GetOrdinal("FacturaId")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Paciente = reader.GetString(reader.GetOrdinal("Paciente")),
                                Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                                TotalAseguradora = reader.GetDecimal(reader.GetOrdinal("TotalAseguradora")),
                                TotalCopago = reader.GetDecimal(reader.GetOrdinal("TotalCopago")),
                                MetodoPago = reader.GetString(reader.GetOrdinal("MetodoPago")),
                                Aseguradora = reader.IsDBNull(reader.GetOrdinal("Aseguradora")) ? "Particular" : reader.GetString(reader.GetOrdinal("Aseguradora")),
                                TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems"))
                            });
                        }
                    }
                }
            }

            return facturas;
        }


        public async Task<FacturaDetalleDTO?> GetFacturaConDetalleAsync(int facturaId)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            FacturaDetalleDTO? factura = null;

            var commandText = @"
        SELECT 
            b.billing_id,
            b.billing_creationdate,
            p.patient_firstname + ' ' + p.patient_firstsurname AS paciente,
            b.billing_payment_method,
            ic.insurance_company_name,
            bi.billing_item_description,
            bi.billing_item_quantity,
            bi.billing_item_unit_price
        FROM billing b
        INNER JOIN appointment a ON b.appointment_id = a.appointment_id
        INNER JOIN patient p ON a.appointment_patientid = p.patient_id
        LEFT JOIN insurance_company ic ON a.appointment_insurance_company_id = ic.insurance_company_id
        LEFT JOIN billing_item bi ON bi.billing_id = b.billing_id
        WHERE b.billing_id = @FacturaId
    ";

            var command = new SqlCommand(commandText, connection);
            command.Parameters.AddWithValue("@FacturaId", facturaId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (factura == null)
                {
                    factura = new FacturaDetalleDTO
                    {
                        FacturaId = reader.GetInt32(0),
                        Fecha = reader.GetDateTime(1),
                        Paciente = reader.GetString(2),
                        MetodoPago = reader.GetString(3),
                        Aseguradora = reader.IsDBNull(4) ? "N/A" : reader.GetString(4)
                    };
                }

                if (!reader.IsDBNull(5))
                {
                    var item = new FacturaItemDTO
                    {
                        Descripcion = reader.GetString(5),
                        Cantidad = reader.GetInt32(6),
                        PrecioUnitario = reader.GetDecimal(7)
                    };

                    factura.Items.Add(item);
                }
            }

            if (factura != null)
            {
                factura.Subtotal = factura.Items.Sum(i => i.Total);
                factura.TotalAseguradora = factura.Subtotal; // Puedes ajustar esto si manejas seguros
                factura.TotalCopago = 0; // Ajustar si manejas copagos por ítem
            }

            return factura;
        }
        public class FacturaDetalleDTO
        {
            public int FacturaId { get; set; }
            public DateTime Fecha { get; set; }
            public string Paciente { get; set; }
            public decimal Subtotal { get; set; }
            public decimal TotalAseguradora { get; set; }
            public decimal TotalCopago { get; set; }
            public string MetodoPago { get; set; }
            public string Aseguradora { get; set; }
            public List<FacturaItemDTO> Items { get; set; } = new();
        }

        public class FacturaItemDTO
        {
            public string Descripcion { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Total => PrecioUnitario * Cantidad;
        }

    }
}
