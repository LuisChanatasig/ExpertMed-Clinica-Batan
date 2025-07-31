using ExpertMed.Models;
using Microsoft.Data.SqlClient; // Asegúrate de tener este using
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
            int citaId,
            DateTime fechaFacturacion,
            decimal totalFactura,
            string metodoPago,
            byte[] comprobantePagoFacturacion,
            string billingDetailsNames,
            string billingDetailsCiNumber,
            string billingDetailsDocumentType,
            string billingDetailsAddress,
            string billingDetailsPhone,
            string billingDetailsEmail,
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

                    // 1. Obtener tarifario para mapear descripciones
                    var tarifario = new Dictionary<string, string>();
                    using (var cmdTarifa = new SqlCommand("SELECT insurance_tariff_code, insurance_tariff_description FROM insurance_tariff", connection))
                    {
                        using var reader = await cmdTarifa.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var code = reader.GetString(0);
                            var description = reader.GetString(1);
                            tarifario[code] = description;
                        }
                    }

                    // 2. Ejecutar el SP
                    using (var command = new SqlCommand("sp_billing", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 60;

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

                        var table = new DataTable();
                        table.Columns.Add("billing_item_code", typeof(string));
                        table.Columns.Add("billing_item_description", typeof(string));
                        table.Columns.Add("billing_item_quantity", typeof(int));
                        table.Columns.Add("billing_item_unit_price", typeof(decimal));

                        foreach (var item in items)
                        {
                            if (string.IsNullOrWhiteSpace(item.Code))
                                throw new ArgumentException("Código de ítem faltante.");

                            string descripcion = item.Description;

                            if (tarifario.ContainsKey(item.Code))
                                descripcion = tarifario[item.Code];

                            if (string.IsNullOrWhiteSpace(descripcion))
                                descripcion = $"Procedimiento {item.Code}";

                            table.Rows.Add(item.Code, descripcion, item.Quantity, item.UnitPrice);
                        }

                        var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
                        {
                            TypeName = "dbo.BillingItemsType",
                            Value = table
                        };
                        command.Parameters.Add(itemsParam);

                        jsonFactura = (string)await command.ExecuteScalarAsync();
                    }

                    // 3. Obtener credenciales Dátil
                    using (var command = new SqlCommand(@"
                SELECT users_xkeytaxo, users_xpasstaxo 
                FROM users 
                WHERE users_id = (SELECT appointment_createuser FROM appointment WHERE appointment_id = @CitaId)", connection))
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

                // Validación final JSON
                if (string.IsNullOrWhiteSpace(jsonFactura) || !jsonFactura.Trim().StartsWith("{"))
                    throw new Exception("El JSON generado por el SP es inválido o está vacío.");

                _logger.LogDebug("Factura JSON para cita {CitaId}: {JsonFactura}", citaId, jsonFactura);
                Console.WriteLine($"Factura JSON para cita {citaId}: {jsonFactura}");

                // 4. Envío a Dátil
                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://link.datil.co/invoices/issue"))
                {
                    request.Content = new StringContent(jsonFactura, Encoding.UTF8, "application/json");
                    request.Headers.Add("X-Key", xKey);
                    request.Headers.Add("X-Password", xPassword);

                    var response = await _httpClient.SendAsync(request);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var errorObj = JsonSerializer.Deserialize<DatilErrorResponse>(responseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                            if (errorObj?.Errors != null && errorObj.Errors.Any(e => e.Code == "INVALID_RECEIPT"))
                            {
                                var errorDetail = errorObj.Errors.First(e => e.Code == "INVALID_RECEIPT").Details;
                                throw new Exception($"Factura rechazada: {errorDetail}");
                            }

                            throw new Exception($"Error al emitir factura: {response.StatusCode} - {responseContent}");
                        }
                        catch (JsonException)
                        {
                            throw new Exception($"Error inesperado al emitir factura: {response.StatusCode} - {responseContent}");
                        }
                    }

                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateAndSendInvoiceAsync para la cita ID: {CitaId}", citaId);
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

            // 1. Traer desde base de datos local (notas de venta)
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
                                Secuencial = reader.GetInt32(reader.GetOrdinal("Secuencial")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Paciente = reader.IsDBNull(reader.GetOrdinal("Paciente"))
                                           ? "(Sin nombre)"
                                           : reader.GetString(reader.GetOrdinal("Paciente")),
                                Medico = reader.IsDBNull(reader.GetOrdinal("Medico"))
                                         ? "(Sin médico)"
                                         : reader.GetString(reader.GetOrdinal("Medico")),
                                Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                                TotalAseguradora = reader.GetDecimal(reader.GetOrdinal("TotalAseguradora")),
                                TotalCopago = reader.GetDecimal(reader.GetOrdinal("TotalCopago")),
                                MetodoPago = reader.IsDBNull(reader.GetOrdinal("MetodoPago"))
                                             ? "-"
                                             : reader.GetString(reader.GetOrdinal("MetodoPago")),
                                Aseguradora = reader.IsDBNull(reader.GetOrdinal("Aseguradora"))
                                              ? "Particular"
                                              : reader.GetString(reader.GetOrdinal("Aseguradora")),
                                TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                                Origen = "LOCAL"
                            });
                        }
                    }
                }
            }

            // 2. Traer desde Dátil (facturas autorizadas)
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "token=7278cc50a72640eea6384a075b8e8335");

            var url = "https://link.datil.co/invoices?from=2025-06-01&to=2025-06-30";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);

                foreach (var f in json.RootElement.EnumerateArray())
                {
                    facturas.Add(new FacturaEmitidaDTO
                    {
                        FacturaId = 0,
                        Fecha = f.GetProperty("issued_at").GetDateTime(),
                        Paciente = f.GetProperty("client").GetProperty("name").GetString(),
                        Subtotal = f.GetProperty("totals").GetProperty("subtotal_without_tax").GetDecimal(),
                        TotalAseguradora = 0,
                        TotalCopago = f.GetProperty("totals").GetProperty("total").GetDecimal(),
                        MetodoPago = "-", // Dátil no da forma de pago
                        Aseguradora = f.GetProperty("client").GetProperty("identification").GetString(),
                        TotalItems = f.GetProperty("items").GetArrayLength(),
                        Origen = "DATIL"
                    });
                }
            }

            return facturas.OrderByDescending(f => f.Fecha).ToList();
        }



        public async Task<FacturaDetalleDTO?> GetFacturaConDetalleAsync(int facturaId)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            await connection.OpenAsync();

            FacturaDetalleDTO? factura = null;

            var commandText = @"
        SELECT
            b.billing_id,                      -- 0
            b.billing_creationdate,            -- 1
            ISNULL(p.patient_firstname, '') + ' ' +
            ISNULL(p.patient_middlename, '') + ' ' +
            ISNULL(p.patient_firstsurname, '') + ' ' +
            ISNULL(p.patient_secondlastname, '') AS paciente, -- 2
            p.patient_landline_phone,          -- 3
            p.patient_cellular_phone,          -- 4
            p.patient_email,                   -- 5
            p.patient_address,                 -- 6
            b.billing_payment_method,          -- 7
            ic.insurance_company_name,         -- 8
            bi.billing_item_description,       -- 9
            bi.billing_item_quantity,          -- 10
            bi.billing_item_unit_price         -- 11
        FROM billing b
        INNER JOIN appointment a ON b.appointment_id = a.appointment_id
        INNER JOIN patient p ON a.appointment_patientid = p.patient_id
        LEFT JOIN insurance_company ic ON a.appointment_insurance_company_id = ic.insurance_company_id
        LEFT JOIN billing_item bi ON bi.billing_id = b.billing_id
        WHERE b.billing_id = @FacturaId
    ";

            using var command = new SqlCommand(commandText, connection);
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
                        Paciente = reader.IsDBNull(2) ? "Sin nombre" : reader.GetString(2).Trim(),
                        PacienteNumeroFijo = reader.IsDBNull(3) ? null : reader.GetString(3),
                        PacienteNumeroCelular = reader.IsDBNull(4) ? null : reader.GetString(4),
                        PacienteEmail = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PacienteDireccion = reader.IsDBNull(6) ? null : reader.GetString(6),
                        MetodoPago = reader.IsDBNull(7) ? "No especificado" : reader.GetString(7),
                        Aseguradora = reader.IsDBNull(8) ? "Particular" : reader.GetString(8),
                        Items = new List<FacturaItemDTO>()
                    };
                }

                // Validar si hay ítems
                if (!reader.IsDBNull(9))
                {
                    var item = new FacturaItemDTO
                    {
                        Descripcion = reader.GetString(9),
                        Cantidad = reader.IsDBNull(10) ? 0 : reader.GetInt32(10),
                        PrecioUnitario = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11)
                    };
                    factura.Items.Add(item);
                }
            }

            if (factura != null)
            {
                factura.Subtotal = factura.Items.Sum(i => i.Total);

                // Lógica provisional: puedes ajustarla según cómo se calcule aseguradora/copago
                factura.TotalAseguradora = factura.MetodoPago.ToLower().Contains("seguro")
                    ? factura.Subtotal
                    : 0;

                factura.TotalCopago = factura.Subtotal - factura.TotalAseguradora;
            }

            return factura;
        }
    }

    // Updated DTOs
    public class FacturaDetalleDTO
    {
        public int FacturaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Paciente { get; set; }
        public string? PacienteDireccion { get; set; } // Added '?' for nullability
        public string? PacienteNumeroCelular { get; set; } // Added '?' for nullability
        public string? PacienteNumeroFijo { get; set; } // Added '?' for nullability
        public string? PacienteEmail { get; set; } // NEW: Added patient email

        public decimal Subtotal { get; set; }
        public decimal TotalAseguradora { get; set; }
        public decimal TotalCopago { get; set; }
        public string MetodoPago { get; set; }
        public string Aseguradora { get; set; }
        public List<FacturaItemDTO> Items { get; set; } = new(); // Initialize list to avoid NullReferenceException
    }

    public class FacturaItemDTO
    {
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total => PrecioUnitario * Cantidad;
    }

}

