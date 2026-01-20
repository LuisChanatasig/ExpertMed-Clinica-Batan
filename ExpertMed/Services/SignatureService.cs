using ExpertMed.Models;
using iText.Bouncycastle;
using iText.Bouncycastleconnector;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ExpertMed.Services
{
    public class SignatureService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SignatureService> _logger;
        private readonly DbExpertmedContext _dbContext;
        private readonly string _baseStoragePath;
        private readonly IWebHostEnvironment _env;

        public SignatureService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<SignatureService> logger,
            DbExpertmedContext dbContext,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _env = env;

            // Si es desarrollo usa una ruta local, si es producción usa la del servidor configurada
            _baseStoragePath = configuration["StorageSettings:DocumentPath"]
                               ?? Path.Combine(Directory.GetCurrentDirectory(), "ExternalStorage");
        }
        // Agregar este método nuevo a SignatureService
        public async Task<int?> GetPatientIdFromTokenAsync(Guid token)
        {
            try
            {
                await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
                await using var cmd = new SqlCommand(@"
            SELECT sr.patient_id 
            FROM signature_requests sr
            INNER JOIN patient p ON sr.patient_code = p.patient_code
            WHERE sr.token = @token", cn)
                {
                    CommandType = CommandType.Text
                };

                cmd.Parameters.AddWithValue("@token", token);
                await cn.OpenAsync();

                var result = await cmd.ExecuteScalarAsync();
                return result == DBNull.Value ? null : Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo patient_id desde token {Token}", token);
                return null;
            }
        }

        public async Task<SignatureRequestDto> CreateRequestAsync(string patientCode, int? createdByUserId, int expiresMinutes = 10)
        {
            await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
            await using var cmd = new SqlCommand("dbo.sp_signature_request_create", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@patient_code", patientCode ?? "");
            cmd.Parameters.AddWithValue("@created_by_userid", (object?)createdByUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@expires_minutes", expiresMinutes);

            await cn.OpenAsync();

            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await rd.ReadAsync())
                throw new InvalidOperationException("SP sp_signature_request_create did not return a row.");

            return new SignatureRequestDto
            {
                Token = rd.GetGuid(rd.GetOrdinal("token")),
                ExpiresAtUtc = rd.GetDateTime(rd.GetOrdinal("expires_at"))
            };
        }

        public async Task<SignatureStatusDto?> GetStatusAsync(Guid token)
        {
            await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
            await using var cmd = new SqlCommand("dbo.sp_signature_status", cn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@token", token);

            await cn.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

            if (!await rd.ReadAsync()) return null;

            // Columnas según SP:
            // status, expires_at, signature_date, signature_data
            var status = Convert.ToByte(rd["status"]);
            var expiresAt = (DateTime)rd["expires_at"];
            DateTime? signedAt = rd["signature_date"] == DBNull.Value ? null : (DateTime)rd["signature_date"];
            string? sig = rd["signature_data"] == DBNull.Value ? null : (string)rd["signature_data"];

            return new SignatureStatusDto
            {
                Status = status,
                ExpiresAtUtc = expiresAt,
                SignedAtLocal = signedAt,
                SignatureDataUrl = sig
            };
        }

        public async Task<SignVm?> GetForSignAsync(Guid token)
        {
            var st = await GetStatusAsync(token);
            if (st == null) return null;

            // Expirado o consumido => no permitir firmar
            if (st.Status is 2 or 3) return null;

            // Si ya expiró por tiempo, también inválido (defensivo)
            if (DateTime.UtcNow > st.ExpiresAtUtc) return null;

            return new SignVm
            {
                Token = token,
                ExpiresAt = st.ExpiresAtUtc,
                Status = st.Status
            };
        }

        public async Task<(bool Ok, string? Message)> SubmitAsync(Guid token, string signatureDataUrl, string? ip, string? userAgent)
        {
            // Validación mínima antes de SP
            if (string.IsNullOrWhiteSpace(signatureDataUrl) || !signatureDataUrl.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
                return (false, "Firma inválida (formato esperado: data:image/png;base64,...)");

            try
            {
                await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
                await using var cmd = new SqlCommand("dbo.sp_signature_submit", cn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@signature_data", signatureDataUrl);
                cmd.Parameters.AddWithValue("@ip_address", (object?)ip ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user_agent", (object?)userAgent ?? DBNull.Value);

                await cn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                return (true, null);
            }
            catch (SqlException ex)
            {
                // Los THROW del SP llegan aquí con Number 5100x
                return (false, ex.Message);
            }
        }

        public async Task ConsumeToPatientAsync(Guid token, int patientId)
        {
            await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
            await using var cmd = new SqlCommand("dbo.sp_signature_consume_to_patient", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@patient_id", patientId);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }


        /// <summary>
        /// Procesa los dos documentos obligatorios para el paciente
        /// </summary>

        // Método ProcessPatientDocumentsAsync CORREGIDO con mejor logging
        public async Task<List<string>> ProcessPatientDocumentsAsync(Guid token, string signatureBase64)
        {
            var archivosGenerados = new List<string>();
            try
            {
                var docsToGenerate = new[] {
            new { Tipo = "Consentimiento", Template = "ConsentimientoDatosCB.pdf", Prefix = "CONSENT" },
            new { Tipo = "LOPDP", Template = "ConsetimientoLOPDP_Template.pdf", Prefix = "LOPDP" }
        };

                foreach (var doc in docsToGenerate)
                {
                    string fileName = $"{doc.Prefix}_{token}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    string templatePath = Path.Combine(_env.ContentRootPath, "Templates", doc.Template);

                    // Generar PDF (usando tu lógica FillSignedPdfAsync)
                    string physicalPath = await FillSignedPdfAsync(templatePath, signatureBase64, fileName, "txt_imagen_firma");

                    if (File.Exists(physicalPath))
                    {
                        archivosGenerados.Add(fileName); // Solo guardamos el nombre
                        _logger.LogInformation("Archivo generado: {FileName}", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en generación física");
            }
            return archivosGenerados;
        }
        public async Task SaveDocumentMetadataAsync(int patientId, string fileName, string physicalPath, string docType)
        {
            await using var cn = new SqlConnection(_dbContext.Database.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GuardarDocumentoFirmado", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@PacienteId", patientId);
            cmd.Parameters.AddWithValue("@NombreArchivo", fileName);
            cmd.Parameters.AddWithValue("@RutaFisica", physicalPath);
            cmd.Parameters.AddWithValue("@TipoDocumento", docType);

            await cn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // Método FillSignedPdfAsync MEJORADO con parámetro de campo firma
        public async Task<string> FillSignedPdfAsync(
            string templatePath,
            string signatureBase64,
            string fileName,
            string signatureFieldName = "txt_imagen_firma")
        {
            BouncyCastleFactoryCreator.SetFactory(new BouncyCastleFactory());

            try
            {
                if (!Directory.Exists(_baseStoragePath))
                {
                    Directory.CreateDirectory(_baseStoragePath);
                    _logger.LogInformation("Directorio creado: {Path}", _baseStoragePath);
                }

                string outputPath = Path.Combine(_baseStoragePath, fileName);
                byte[] templateBytes = await File.ReadAllBytesAsync(templatePath);

                var base64Data = signatureBase64.Contains(",")
                    ? signatureBase64.Split(',')[1]
                    : signatureBase64;

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                ImageData imageData = ImageDataFactory.Create(imageBytes);

                using (MemoryStream ms = new MemoryStream(templateBytes))
                using (PdfReader reader = new PdfReader(ms))
                using (FileStream dest = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                using (PdfWriter writer = new PdfWriter(dest))
                using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
                {
                    PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                    if (form == null)
                    {
                        _logger.LogWarning("El PDF no tiene formulario AcroForm");
                        pdfDoc.Close();
                        return outputPath;
                    }

                    var fields = form.GetAllFormFields();

                    _logger.LogInformation("Campos encontrados en PDF: {Fields}",
                        string.Join(", ", fields.Keys));

                    // Llenar fecha
                    if (fields.ContainsKey("txt_fecha"))
                    {
                        fields["txt_fecha"].SetValue(DateTime.Now.ToString("dd/MM/yyyy"));
                        _logger.LogInformation("Fecha insertada correctamente");
                    }

                    // Insertar firma
                    bool firmaInsertada = false;

                    if (fields.ContainsKey(signatureFieldName))
                    {
                        var widgets = fields[signatureFieldName].GetWidgets();

                        if (widgets.Count > 0)
                        {
                            var widget = widgets[0];
                            var rectangle = widget.GetRectangle().ToRectangle();
                            var page = widget.GetPage();

                            iText.Layout.Element.Image signatureImage = new iText.Layout.Element.Image(imageData);
                            signatureImage.ScaleToFit(rectangle.GetWidth(), rectangle.GetHeight());
                            signatureImage.SetFixedPosition(
                                pdfDoc.GetPageNumber(page),
                                rectangle.GetLeft(),
                                rectangle.GetBottom()
                            );

                            new Canvas(page, rectangle).Add(signatureImage).Close();
                            firmaInsertada = true;

                            _logger.LogInformation("Firma insertada en campo '{Field}'", signatureFieldName);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Campo de firma '{Field}' NO encontrado en el PDF", signatureFieldName);
                    }

                    form.FlattenFields();
                    pdfDoc.Close();

                    if (!firmaInsertada)
                    {
                        _logger.LogWarning("ADVERTENCIA: Firma NO insertada en {FileName}", fileName);
                    }
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en FillSignedPdfAsync para {FileName}", fileName);
                throw;
            }
        }
    }
}
