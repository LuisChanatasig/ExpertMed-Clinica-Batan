using ExpertMed.Models;
using ExpertMed.Services;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Globalization;
using System.Text;


namespace ExpertMed.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly UserService _usersService;
        private readonly ILogger<DocumentsController> _logger;
        private readonly SelectsService _selectService;
        private readonly PatientService _patientService;
        private readonly ConsultationService _consultationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //Inyección de dependencias
        public DocumentsController(UserService usersService, ILogger<DocumentsController> logger, SelectsService selectService, PatientService patientService, ConsultationService consultationService, IWebHostEnvironment webHostEnvironment)
        {
            _usersService = usersService;
            _logger = logger;
            _selectService = selectService;
            _patientService = patientService;
            _consultationService = consultationService;
            _webHostEnvironment = webHostEnvironment;
        }
        private string FormatearFechaLarga(DateTime fecha)
        {
            var cultura = new CultureInfo("es-ES");

            string diaSemana = cultura.DateTimeFormat.GetDayName(fecha.DayOfWeek).ToUpper();
            int dia = fecha.Day;

            // Números en letras (hasta 31)
            var diaEnLetras = new[] {
        "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE",
        "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUNO", "VEINTIDÓS", "VEINTITRÉS",
        "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE",
        "TREINTA", "TREINTA Y UNO"
    };

            string mes = cultura.DateTimeFormat.GetMonthName(fecha.Month).ToUpper();
            string anioTexto = ConvertirAnioALetras(fecha.Year);

            return $"{diaSemana} {dia} ({diaEnLetras[dia]}) DE {mes} DE {anioTexto}";
        }
        private string ConvertirAnioALetras(int anio)
        {
            switch (anio)
            {
                case 2020: return "DOS MIL VEINTE";
                case 2021: return "DOS MIL VEINTIUNO";
                case 2022: return "DOS MIL VEINTIDÓS";
                case 2023: return "DOS MIL VEINTITRÉS";
                case 2024: return "DOS MIL VEINTICUATRO";
                case 2025: return "DOS MIL VEINTE Y CINCO";
                case 2026: return "DOS MIL VEINTE Y SEIS";
                case 2027: return "DOS MIL VEINTE Y SIETE";
                case 2028: return "DOS MIL VEINTE Y OCHO";
                case 2029: return "DOS MIL VEINTE Y NUEVE";
                case 2030: return "DOS MIL TREINTA";
                default: return anio.ToString(); // fallback
            }
        }

        private string ConvertirNumeroATexto(int numero)
{
    if (numero <= 0 || numero > 99) return numero.ToString();
    
    var unidades = new[] { "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
    var especiales = new[] { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", 
                             "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
    var decenas = new[] { "", "", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", 
                          "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
    
    if (numero < 10)
        return unidades[numero];
    
    if (numero < 20)
        return especiales[numero - 10];
    
    if (numero < 30)
    {
        if (numero == 20) return "VEINTE";
        return "VEINTI" + unidades[numero - 20];
    }
    
    int decena = numero / 10;
    int unidad = numero % 10;
    
    if (unidad == 0)
        return decenas[decena];
    
    return decenas[decena] + " Y " + unidades[unidad];
}

        void InsertImageFromField(PdfStamper stamper, AcroFields fields, string fieldName, byte[] imageBytes)
        {
            var fieldPosition = fields.GetFieldPositions(fieldName)?.FirstOrDefault();
            if (fieldPosition != null && imageBytes != null && imageBytes.Length > 0)
            {
                var rect = fieldPosition.position;
                var page = fieldPosition.page;

                var image = iTextSharp.text.Image.GetInstance(imageBytes);
                image.ScaleToFit(rect.Width, rect.Height); // Escala al tamaño exacto del campo
                image.SetAbsolutePosition(rect.Left, rect.Bottom);

                stamper.GetOverContent(page).AddImage(image);
            }
        }



        [HttpGet]
        public async Task<IActionResult> MedicalCertificate(int consultationId)
        {
            // 1) Obtener consulta - usar versión async si existe
            var consultation =  _consultationService.GetConsultationDetails(consultationId);
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // 2) Obtener paciente
            var patient = await _patientService.GetPatientFullByIdAsync(consultation.ConsultationPatient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // 3) Plantilla base
            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "Certificado_MedicoV12.pdf");

            byte[] pdfBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var pdfReader = new PdfReader(templatePath))
                using (var pdfStamper = new PdfStamper(pdfReader, memoryStream))
                {
                    AcroFields formFields = pdfStamper.AcroFields;
                    var cultura = new CultureInfo("es-ES");

                    // =========================
                    // LOGO DE LA CABECERA
                    // =========================
                    InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo", consultation.UsersEstablishmentLogo);

                    // =========================
                    // FECHA Y TEXTO CABECERA
                    // =========================
                    var fechaEmision = consultation.ConsultationCreationdate ?? DateTime.Today;

                    string nombreCompletoPaciente = $"{patient.PatientFirstname ?? ""} {patient.PatientMiddlename ?? ""} {patient.PatientFirstsurname ?? ""} {patient.PatientSecondlastname ?? ""}"
                                                    .Trim().ToUpper();

                    string diaSemana = cultura.DateTimeFormat.GetDayName(fechaEmision.DayOfWeek).ToUpper();

                    var diasEnLetras = new[]
                    {
                "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
                "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISÉIS", "DIECISIETE",
                "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUNO", "VEINTIDÓS", "VEINTITRÉS",
                "VEINTICUATRO", "VEINTICINCO", "VEINTISÉIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE",
                "TREINTA", "TREINTA Y UNO"
            };

                    string diaEnLetras = diasEnLetras[fechaEmision.Day];
                    string mes = cultura.DateTimeFormat.GetMonthName(fechaEmision.Month).ToUpper();

                    string textoCabecera = $"Certifico que el paciente {nombreCompletoPaciente} con identificación " +
                                           $"{patient.PatientDocumentnumber ?? "N/A"} fue atendido el día de hoy " +
                                           $"{diaSemana} {fechaEmision.Day} ({diaEnLetras}) DE {mes} DE {fechaEmision.Year}";

                    formFields.SetField("txt_cabecera_certificado", textoCabecera);
                    formFields.SetField("txt_fecha_emision", DateTime.Now.ToString("dd/MM/yyyy"));

                    // =========================
                    // DATOS DEL PACIENTE
                    // =========================
                    string nombreCompleto = $"{patient.PatientFirstname ?? ""} {patient.PatientMiddlename ?? ""} {patient.PatientFirstsurname ?? ""} {patient.PatientSecondlastname ?? ""}".Trim();
                    string dataPaciente1 = $"Nombres y Apellidos: {nombreCompleto}\n" +
                                           $"Dirección domicilio: {patient.PatientAddress ?? "N/A"}\n" +
                                           $"Número telefónico de contacto: {patient.PatientCellularPhone ?? "N/A"} / {patient.PatientLandlinePhone ?? "N/A"}\n" +
                                           $"Institución/Empresa: {patient.PatientCompany ?? "N/A"}\n" +
                                           $"Puesto de trabajo: {patient.PatientOcupation ?? "N/A"}\n" +
                                           $"Cédula/Pasaporte: {patient.PatientDocumentnumber ?? "N/A"}\n" +
                                           $"Historia clínica: {patient.PatientDocumentnumber ?? "N/A"}";

                    formFields.SetField("txt_datos_paciente", dataPaciente1);
                    formFields.SetField("txt_datos_paciente_2", dataPaciente1);

                    // =========================
                    // DIAGNÓSTICOS
                    // =========================
                    var diagnosisList = await _selectService.GetAllDiagnosisAsync();

                    var diagnosisDefNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisDefinitive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    var diagnosisPreNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisPresumptive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    string diagnosticosTexto =
                        $"• Diagnóstico Definitivo: {(diagnosisDefNames.Any() ? string.Join(", ", diagnosisDefNames) : "N/A")}\n" +
                        $"• Diagnóstico Presuntivo: {(diagnosisPreNames.Any() ? string.Join(", ", diagnosisPreNames) : "N/A")}";

                    formFields.SetField("txt_diagnosticos", diagnosticosTexto);

                    // =========================
                    // PROCEDIMIENTOS EN LÍNEA COMPACTA
                    // =========================
                    var procedimientosTexto = new StringBuilder();
                    if (consultation.Procedures?.Any() == true)
                    {
                        int count = 1;
                        foreach (var proc in consultation.Procedures.OrderBy(p => p.Procedure_Date ?? DateTime.MaxValue))
                        {
                            string nombreProc = string.IsNullOrWhiteSpace(proc.Procedure_Name) ? "Sin especificar" : proc.Procedure_Name;
                            string fechaProc = proc.Procedure_Date?.ToString("dd/MM/yy", cultura) ?? "S/F";

                            // Truncar a 25 caracteres
                            if (nombreProc.Length > 25)
                                nombreProc = nombreProc.Substring(0, 22) + "...";

                            // Todo en una línea: "1) Nombre (fecha) | "
                            procedimientosTexto.Append($"{count}) {nombreProc} ({fechaProc})  |  ");
                            count++;

                            // Salto de línea cada 2 procedimientos (ajustable)
                            if (count % 2 == 1)
                                procedimientosTexto.AppendLine();
                        }
                    }
                    else
                    {
                        procedimientosTexto.AppendLine("No se registraron procedimientos");
                    }
                    formFields.SetField("txt_procedimientos", procedimientosTexto.ToString().TrimEnd());
                    // =========================
                    // SÍNTOMAS Y ENFERMEDAD
                    // =========================
                    formFields.SetField("chk_sintomas_si", consultation.ConsutationHasSymptoms == true ? "X" : "");
                    formFields.SetField("chk_sintomas_no", consultation.ConsutationHasSymptoms != true ? "X" : "");

                    bool tieneEnfermedad = consultation.ConsultationHasdisease ?? false;
                    formFields.SetField("chk_enfermedad_si", tieneEnfermedad ? "X" : "");
                    formFields.SetField("chk_enfermedad_no", !tieneEnfermedad ? "X" : "");

                    string descripcionEnfermedad = tieneEnfermedad
                        ? (consultation.ConsultationDiseaseobservation ?? consultation.ConsultationDisease ?? "N/A")
                        : "N/A";
                    formFields.SetField("txt_descripcion_enfermedad", descripcionEnfermedad);

                    // =========================
                    // CONTINGENCIA
                    // =========================
                    formFields.SetField("txt_tipo_contigencia", consultation.ConsultationContingencytype ?? "Común");

                    // =========================
                    // REPOSO MÉDICO
                    // =========================
                    // =========================
                    // REPOSO MÉDICO
                    // =========================
                    int diasReposo = consultation.ConsultationDisablilitydays ?? 0;

                    string reposoTexto;

                    var fechaFin = fechaEmision.AddDays(Math.Max(diasReposo - 1, 0)); // evita negativos

                    // Convertir número a texto
                    string diasEnTexto = ConvertirNumeroATexto(diasReposo);
                    string pluralDia = diasReposo == 1 ? "Día" : "Días";

                    // Fecha DESDE
                    string diaSemanaDesde = cultura.DateTimeFormat.GetDayName(fechaEmision.DayOfWeek).ToUpper();
                    string diaEnLetrasDesde = diasEnLetras[fechaEmision.Day];
                    string mesDesde = cultura.DateTimeFormat.GetMonthName(fechaEmision.Month).ToUpper();
                    string anioDesde = ConvertirAnioALetras(fechaEmision.Year);

                    // Fecha HASTA
                    string diaSemanaHasta = cultura.DateTimeFormat.GetDayName(fechaFin.DayOfWeek).ToUpper();
                    string diaEnLetrasHasta = diasEnLetras[fechaFin.Day];
                    string mesHasta = cultura.DateTimeFormat.GetMonthName(fechaFin.Month).ToUpper();
                    string anioHasta = ConvertirAnioALetras(fechaFin.Year);

                    // Texto siempre generado (IESS lo exige)
                    reposoTexto = $"Se requiere reposo médico en domicilio por {diasReposo} ({diasEnTexto}) {pluralDia}." +
                                  $" DESDE: {fechaEmision:dd/MM/yyyy} {diaSemanaDesde} {fechaEmision.Day} ({diaEnLetrasDesde}) DE {mesDesde} DE {anioDesde}." +
                                  $" HASTA: {fechaFin:dd/MM/yyyy} {diaSemanaHasta} {fechaFin.Day} ({diaEnLetrasHasta}) DE {mesHasta} DE {anioHasta}.";

                    formFields.SetField("txt_reposo_medico", reposoTexto);

                    // =========================
                    // DATOS DEL MÉDICO
                    // =========================
                    string datosMedico = $"Dr(a). {consultation.UsersNames ?? ""} {consultation.UsersSurcenames ?? ""}".Trim() + "\n" +                                     
                                         $"{consultation.UsersDocumentNumber ?? "N/A"}\n" +
                                         $"{consultation.SpecialityName ?? "Médico General"}\n" +
                                         $"Email: {consultation.UsersEmail ?? "N/A"}\n" +
                                         $"Teléfono: {consultation.UsersPhone ?? "N/A"}";

                    formFields.SetField("txt_datos_medico", datosMedico);

                    // =========================
                    // FINALIZAR PDF
                    // =========================
                    pdfStamper.FormFlattening = true;
                    pdfStamper.Close(); // ✅ CRÍTICO: Cerrar antes de leer el stream
                }

                pdfBytes = memoryStream.ToArray(); // ✅ Extraer bytes después de cerrar stamper
            }

            var randomNumber = new Random().Next(1000, 9999);
            return File(pdfBytes, "application/pdf", $"certificado_medico_{randomNumber}.pdf");
        }


        public async Task<IActionResult> MedicalForm(int consultationId)
        {
            // Obtener los detalles de la consulta
            var consultation = _consultationService.GetConsultationDetails(consultationId);

            // Verificar si la consulta existe
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener el patientId de la consulta
            var patientId = consultation.ConsultationPatient;

            // Obtener los detalles del paciente
            var patient = await _patientService.GetPatientFullByIdAsync(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener datos adicionales para la vista
            var genderTypes = await _selectService.GetGenderTypeAsync();
            var bloodTypes = await _selectService.GetBloodTypeAsync();
            var documentTypes = await _selectService.GetDocumentTypeAsync();
            var civilTypes = await _selectService.GetCivilTypeAsync();
            var professionalTrainingTypes = await _selectService.GetProfessionaltrainingTypeAsync();
            var sureHealthTypes = await _selectService.GetSureHealtTypeAsync();
            var countries = await _selectService.GetAllCountriesAsync();
            var provinces = await _selectService.GetAllProvinceAsync();
            var parents = await _selectService.GetRelationshipTypeAsync();
            var allergies = await _selectService.GetAllergiesTypeAsync();
            var surgeries = await _selectService.GetSurgeriesTypeAsync();
            var familyMember = await _selectService.GetFamiliarTypeAsync();
            var diagnosis = await _selectService.GetAllDiagnosisAsync();
            var medications = await _selectService.GetAllMedicationsAsync();
            var images = await _selectService.GetAllImagesAsync();
            var laboratories = await _selectService.GetAllLaboratoriesAsync();

            // Crear el ViewModel
            var consulta = new NewPatientViewModel
            {
                DetailsPatient = patient,
                GenderTypes = genderTypes,
                BloodTypes = bloodTypes,
                DocumentTypes = documentTypes,
                CivilTypes = civilTypes,
                ProfessionalTrainingTypes = professionalTrainingTypes,
                SureHealthTypes = sureHealthTypes,
                Countries = countries,
                Provinces = provinces,
                Parents = parents,
                AllergiesTypes = allergies,
                SurgeriesTypes = surgeries,
                FamilyMember = familyMember,
                Diagnoses = diagnosis,
                Medications = medications,
                Images = images,
                Laboratories = laboratories,
                Consultation = consultation // Agregar los detalles de la consulta al ViewModel
            };

            // Tamaño de página A4 estándar
            var document = Document.Create(container =>
            {

                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(598, 845);

                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    // Header con una tabla de 6 columnas
                    page.Header().Border(2).BorderColor("#808080").Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Establecimiento
                            columns.ConstantColumn(100); // Nombre
                            columns.ConstantColumn(100); // Apellido
                            columns.ConstantColumn(70); // Sexo
                            columns.ConstantColumn(70); // Edad
                            columns.ConstantColumn(118); // Nº Historia Clínica
                        });

                        // Fila de encabezados
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("ESTABLECIMIENTO").FontSize(6);
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("NOMBRE").FontSize(6);
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("APELLIDO").FontSize(6);
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("SEXO").FontSize(6);
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("EDAD").FontSize(6);
                        table.Cell().Border(1).BorderColor("#808080").Element(CellStyle => CellStyle.Background("#ccffcc"))
                            .MinHeight(14).AlignCenter().PaddingTop(3).Text("Nº HISTORIA CLÍNICA").FontSize(6);






                        // Fila de contenido
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3).Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consultation.EstablishmentName)
                            .FontSize(7);
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3)
                            .Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consulta.DetailsPatient.PatientFirstsurname).FontSize(7);
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3)
                            .Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consulta.DetailsPatient.PatientFirstname).FontSize(7);
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3)
                            .Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consulta.DetailsPatient.PatientGenderName).FontSize(7);
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3)
                            .Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consulta.DetailsPatient.PatientAge).FontSize(7);
                        table.Cell().Border(1).BorderColor("#808080").MinHeight(7).AlignCenter().PaddingTop(3)
                            .Element(CellStyle => CellStyle.Background("#FFFFFF")).Text(consulta.DetailsPatient.PatientDocumentnumber).FontSize(7);
                    });

                    // Contenido principal con múltiples tablas
                    page.Content().PaddingTop(6).Column(contentColumn =>
                    {
                        // Primera tabla
                        contentColumn.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557);
                            });

                            // Fila de encabezado
                            table.Cell().MinHeight(14).Border(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).PaddingLeft(3).Text("1. MOTIVO DE CONSULTA").FontSize(10).Bold();

                            // Fila de datos
                            table.Cell().MinHeight(14).Border(2).BorderColor(Colors.Grey.Medium).Text($"{consulta.Consultation.ConsultationReason}").FontSize(10);
                        });

                        // Segunda tabla
                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557);
                            });

                            // Fila de encabezado
                            table.Cell().MinHeight(14).Border(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).PaddingLeft(3).Text("2. ANTECEDENTES PERSONALES").FontSize(10).Bold();

                            // Fila de datos
                            table.Cell().MinHeight(14).BorderLeft(2).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text($"{consulta.Consultation.ConsultationPersonalbackground}").FontSize(10);
                            // Celda para Alergias
                            // Celda para Alergias
                            table.Cell().MinHeight(14).BorderLeft(2).BorderBottom(1).BorderRight(2).BorderColor("#808080")
     .Column(column =>
     {
         if (consulta.Consultation.AllergiesConsultations != null && consulta.Consultation.AllergiesConsultations.Any())
         {
             // Obtener la lista de nombres de cirugías a partir de los IDs
             var surgeriesName = consulta.Consultation.AllergiesConsultations
                 .Select(surgery => consulta.AllergiesTypes
                 .FirstOrDefault(type => type.CatalogId == surgery.AllergiesCatalogid)?.CatalogName ?? "N/A")
                 .ToList();

             // Unir los nombres de las cirugías en una sola cadena
             var cirugiasTexto = string.Join(", ", surgeriesName);

             column.Item().Text(text =>
             {
                 // "Cirugías:" en negrita
                 text.Span("Alergias:").Bold().FontSize(10);
                 // Nombres de cirugías sin negrita
                 text.Span($" {cirugiasTexto}.").FontSize(8);
             });
         }
         else
         {
             column.Item().Text("Alergias: No se registraron cirugías.").FontSize(10);
         }
     });





                            // Celda para Cirugías
                            table.Cell().MinHeight(14).BorderLeft(2).BorderBottom(1).BorderRight(2).BorderColor("#808080")
      .Column(column =>
      {
          if (consulta.Consultation.SurgeriesConsultations != null && consulta.Consultation.SurgeriesConsultations.Any())
          {
              // Obtener la lista de nombres de cirugías a partir de los IDs
              var surgeriesName = consulta.Consultation.SurgeriesConsultations
                  .Select(surgery => consulta.SurgeriesTypes
                  .FirstOrDefault(type => type.CatalogId == surgery.SurgeriesCatalogid)?.CatalogName ?? "N/A")
                  .ToList();

              // Unir los nombres de las cirugías en una sola cadena
              var cirugiasTexto = string.Join(", ", surgeriesName);

              column.Item().Text(text =>
              {
                  // "Cirugías:" en negrita
                  text.Span("Cirugías:").Bold().FontSize(10);
                  // Nombres de cirugías sin negrita
                  text.Span($" {cirugiasTexto}.").FontSize(8);
              });
          }
          else
          {
              column.Item().Text("Cirugías: No se registraron cirugías.").FontSize(10);
          }
      });






                        });

                        // Tercera tabla

                        contentColumn.Item().PaddingTop(7).Border(2).BorderColor("808080").Table(table =>
                        {
                            // Definir las columnas de la tabla para el encabezado
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557); // Encabezado general ocupa toda la fila
                            });

                            // Fila de encabezado general "3 ANTECEDENTES FAMILIARES"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderRight(2).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).Text("3 ANTECEDENTES FAMILIARES").FontSize(10).Bold();

                            table.Cell().Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(27); // Primera columna
                                        columns.ConstantColumn(28); // Segunda columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(28); // Tercera columna
                                        columns.ConstantColumn(29); // Tercera columna
                                        columns.ConstantColumn(24); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("1.\nCARDIOPATIA").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundHeartdisease == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("2. \nDIABETES").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDiabetes == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(15).MinWidth(3).Text("3. ENF.CARDIOVASCULAR\n").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxcardiovascular == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("4.  HIPERTENSION").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundHypertension == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("5.\nCANCER").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundCancer == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("6. TUBERCULOSIS").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundTuberculosis == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("7.ENF MENTAL").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxmental == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("8. ENF INFECCIOSA").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxinfectious == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("9. MAL FORMACION").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundMalformation == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("10 OTRO").FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).PaddingTop(6).Text(consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundOther == true ? "X" : "").FontSize(7).AlignCenter();

                                });
                            });

                            // Crear las observaciones para cada patología con parentesco u observación
                            var observaciones = new List<string>();

                            var familyMembers = consulta.FamilyMember; // Lista de relaciones familiares

                            if (consulta.Consultation.FamiliaryBackground != null)
                            {
                                void AgregarObservacion(string titulo, int? relacionId, string observacion)
                                {
                                    if (relacionId.HasValue || !string.IsNullOrEmpty(observacion))
                                    {
                                        // Buscar el nombre de la relación en la lista de familiares
                                        var relacionNombre = familyMembers?.FirstOrDefault(c => c.CatalogId == relacionId)?.CatalogName ?? "N/A";

                                        // Agregar a la lista de observaciones
                                        observaciones.Add($"{titulo}: Relación - {relacionNombre}, Observación - {observacion}");
                                    }
                                }

                                // Llamadas a la función para agregar cada patología
                                AgregarObservacion("Cardiopatía", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogHeartdisease,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundHeartdiseaseObservation);

                                AgregarObservacion("Diabetes", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogDiabetes,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDiabetesObservation);

                                AgregarObservacion("Enf. Cardiovascular", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogDxcardiovascular,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxcardiovascularObservation);

                                AgregarObservacion("Hipertensión", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogHypertension,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundHypertensionObservation);

                                AgregarObservacion("Cáncer", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogCancer,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundCancerObservation);

                                AgregarObservacion("Tuberculosis", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshTuberculosis,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundTuberculosisObservation);

                                AgregarObservacion("Enf. Mental", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogDxmental,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxmentalObservation);

                                AgregarObservacion("Enf. Infecciosa", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogDxinfectious,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundDxinfectiousObservation);

                                AgregarObservacion("Mal Formación", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogMalformation,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundMalformationObservation);

                                AgregarObservacion("Otro", consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundRelatshcatalogOther,
                                                   consulta.Consultation.FamiliaryBackground.FamiliaryBackgroundOtherObservation);
                            }

                            // Renderizar observaciones en la tabla si hay alguna
                            if (observaciones.Any())
                            {
                                foreach (var observacion in observaciones)
                                {
                                    var observacionFormateada = char.ToUpper(observacion[0]) + observacion.Substring(1).ToLower();

                                    table.Cell().BorderLeft(2).BorderBottom(1).BorderRight(2).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                        .Text(observacionFormateada).FontSize(9).AlignStart();
                                }
                            }
                            else
                            {
                                table.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                    .Text("").FontSize(9).AlignStart();
                            }

                            table.Cell().MinHeight(16).BorderLeft(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);


                        });
                        //CUARTA TABLA
                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557);
                            });

                            // Fila de encabezado
                            table.Cell().MinHeight(14).Border(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).PaddingLeft(3).Text("4 ENFERMEDAD O PROBLEMA ACTUAL").FontSize(10).Bold();

                            var texto = consulta.Consultation.ConsultationDisease;

                            // Definir un límite de caracteres que se ajuste a una celda.
                            var limiteCaracteresPorFila = 700;

                            // Dividir el texto en fragmentos según el límite
                            var partesTexto = Enumerable.Range(0, (texto.Length + limiteCaracteresPorFila - 1) / limiteCaracteresPorFila)
                                                        .Select(i => texto.Substring(i * limiteCaracteresPorFila, Math.Min(limiteCaracteresPorFila, texto.Length - i * limiteCaracteresPorFila)))
                                                        .ToList();

                            // Generar las filas dinámicamente
                            foreach (var parte in partesTexto)
                            {
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text(parte).FontSize(10);

                                // Las siguientes celdas serán "quemadas" (vacías)
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(1).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);
                                table.Cell().BorderLeft(2).MinHeight(14).BorderBottom(2).BorderRight(2).BorderColor("#808080").Text("").FontSize(10);
                            }
                        });
                        //QUINTA TABLA
                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            // Definir las columnas de la tabla para el encabezado
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557); // Encabezado general ocupa toda la fila
                            });

                            // Fila de encabezado general "3 ANTECEDENTES FAMILIARES"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderRight(2).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(3).Text("5 REVISIÓN ACTUAL DE ÓRGANOS Y SISTEMAS").FontSize(10).Bold();

                            table.Cell().Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {

                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(17); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").FontSize(5).Bold().AlignEnd();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).Bold().AlignEnd();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).Bold().AlignEnd();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).Bold().AlignEnd();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).Bold().AlignEnd();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();



                                });
                            });
                            table.Cell().Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {

                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna
                                        columns.ConstantColumn(79); // Tercera columna
                                        columns.ConstantColumn(17); // Tercera columna
                                        columns.ConstantColumn(16); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("1 ÓRGANO DE LOS\r\nSENTIDOS").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsOrgansenses == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsOrgansenses == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("3 CARDIO\r\nVASCULAR").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsCardiovascular == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsCardiovascular == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("5.  GENITAL").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsGenital == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsGenital == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("7. MÚSCULO\r\nESQUELÉTICO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsSkeletalM == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsSkeletalM == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("9. HEMO LINFÁTICO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsLymphatic == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsLymphatic == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("2. RESPIRATORIO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsRespiratory == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsRespiratory == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("4. DIGESTIVO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsDigestive == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsDigestive == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("6. URINARIO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsUrinary == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsUrinary == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("8. ENDOCRINO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsEndrocrine == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsEndrocrine == true ? "" : "X").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("10. NERVIOSO").FontSize(6).Bold().AlignEnd();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsNervous == true ? "X" : "").FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(10).MinWidth(3).Text(consulta.Consultation.OrgansSystem.OrganssystemsNervous == true ? "" : "X").FontSize(7).AlignCenter();
                                });
                            });
                            // Lista de observaciones (puedes ajustarla según tu modelo de datos real)
                            var observaciones = new List<string>
{
    consulta.Consultation.OrgansSystem.OrganssystemsOrgansensesObs,
    consulta.Consultation.OrgansSystem.OrganssystemsCardiovascularObs,
    consulta.Consultation.OrgansSystem.OrganssystemsGenitalObs,
    consulta.Consultation.OrgansSystem.OrganssystemsSkeletalMObs,
    consulta.Consultation.OrgansSystem.OrganssystemsLymphaticObs,
    consulta.Consultation.OrgansSystem.OrganssystemsRespiratoryObs,
    consulta.Consultation.OrgansSystem.OrganssystemsDigestiveObs,
    consulta.Consultation.OrgansSystem.OrganssystemsUrinaryObs,
    consulta.Consultation.OrgansSystem.OrganssystemsEndocrine,
    consulta.Consultation.OrgansSystem.OrganssystemsNervousObs,
    // Agrega más observaciones aquí
};

                            // Iterar sobre las observaciones para generar las filas dinámicamente
                            foreach (var observacion in observaciones.Where(o => !string.IsNullOrEmpty(o)))
                            {
                                // Primera celda con la observación
                                table.Cell()
                                    .MinHeight(13)
                                    .BorderLeft(2)
                                    .BorderRight(2)
                                    .BorderBottom(1)
                                    .BorderColor("#808080")
                                    .Text(observacion)
                                    .FontSize(10);

                                // Las siguientes celdas están quemadas (vacías)
                            }



                        });
                        //SEXTA TABLA
                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            // Definir las columnas de la tabla para el encabezado
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(557); // Encabezado general ocupa toda la fila
                            });

                            // Fila de encabezado general "3 ANTECEDENTES FAMILIARES"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderRight(2).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(3).Text("6 SIGNOS VITALES Y ANTROPOMETRIA").FontSize(10).Bold();
                            table.Cell().Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {

                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(95); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("FECHA DE MEDICIÓN").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).AlignCenter().Text(consulta.Consultation.ConsultationCreationdate).Bold().FontSize(7);
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("TEMPERATURA °C").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationTemperature).Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();


                                });
                            });
                            table.Cell().Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {

                                        columns.ConstantColumn(92); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(47); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("PRESIÓN ARTERIAL").FontSize(5).Bold().AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationBloodpressuredAs).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationBloodpresuredDis).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();

                                });
                            });

                            table.Cell().BorderBottom(2).BorderColor("808080").Element(CellStyle =>
                            {
                                // Crear una tabla interna con varias columnas dentro de la celda "padre"
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Añadir columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {

                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(46); // Tercera columna
                                        columns.ConstantColumn(47); // Tercera columna


                                    });

                                    // Fila dentro de la tabla anidada 
                                    nestedTable.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("PULSO / min").FontSize(5).Bold().AlignCenter();
                                    nestedTable.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("FRECUENCIA\r\nRESPIRATORIA").FontSize(5).Bold().AlignCenter();

                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationPulse).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationRespirationrate).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();

                                    nestedTable.Cell().BorderRight(1).BorderBottom(2).BorderColor("#808080").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("PESO / Kg").FontSize(5).Bold().AlignCenter();
                                    nestedTable.Cell().BorderRight(1).BorderBottom(2).BorderColor("#808080").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("TALLA / cm").FontSize(5).Bold().AlignCenter();

                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationSize).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text(consulta.Consultation.ConsultationWeight).FontSize(7).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(10).MinWidth(3).Text("").FontSize(4).AlignCenter();

                                });
                            });



                        });

                        //SEPTIMA TABLA
                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            // Definir las columnas de la tabla principal para el encabezado
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(185); // Primera columna
                                columns.ConstantColumn(185); // Segunda columna
                                columns.ConstantColumn(187); // Tercera columna
                            });

                            // Fila de encabezado "7 EXAMEN FÍSICO REGIONAL"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(3).Text("7 EXAMEN FÍSICO REGIONAL ").FontSize(10).Bold();

                            table.Cell().MinHeight(14).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(70).Text("CP = CON EVIDENCIA DE PATOLOGÍA: MARCAR \"X\" Y DESCRIBIR ").FontSize(6).Bold().AlignCenter();

                            table.Cell().MinHeight(14).BorderRight(2).BorderTop(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(65).Text("SP = SIN EVIDENCIA DE PATOLOGÍA:\r\n MARCAR \"X\" Y NO DESCRIBIR\r\n").FontSize(6).Bold().AlignCenter();

                            // Aquí creamos una nueva celda para la tabla interna con 18 columnas
                            table.Cell().ColumnSpan(3).Element(CellStyle =>
                            {
                                // Crear una tabla interna con 18 columnas
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Definir 18 columnas dentro de la tabla anidada
                                    nestedTable.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(70);  // Columna 1
                                        columns.ConstantColumn(16);  // Columna 2
                                        columns.ConstantColumn(10);  // Columna 3
                                        columns.ConstantColumn(70);  // Columna 4
                                        columns.ConstantColumn(10);  // Columna 5
                                        columns.ConstantColumn(10);  // Columna 6
                                        columns.ConstantColumn(70);  // Columna 7
                                        columns.ConstantColumn(10);  // Columna 8
                                        columns.ConstantColumn(10);  // Columna 9
                                        columns.ConstantColumn(70);  // Columna 10
                                        columns.ConstantColumn(10);  // Columna 11
                                        columns.ConstantColumn(10);  // Columna 12
                                        columns.ConstantColumn(70);  // Columna 13
                                        columns.ConstantColumn(10);  // Columna 14
                                        columns.ConstantColumn(10);  // Columna 15
                                        columns.ConstantColumn(79);  // Columna 16
                                        columns.ConstantColumn(10);  // Columna 17
                                        columns.ConstantColumn(10);  // Columna 18
                                    });

                                    // Fila dentro de la tabla anidada con 18 celdas creadas manualmente
                                    // Aquí agregas todas las celdas para la tabla de 18 columnas
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("CP").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderLeft(2).BorderBottom(1).BorderColor("#C6C2C2").Background("#99ccff").MinHeight(10).MinWidth(3).Text("SP").Bold().FontSize(5).AlignCenter();
                                    // Continúa agregando las celdas necesarias
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3).Text("1. CABEZA").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationHead == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationHead == true ? "" : "X").Bold().FontSize(7).AlignCenter();

                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3).Text("2. CUELLO").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationNeck == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationNeck == true ? "" : "X").Bold().FontSize(7).AlignCenter();

                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(10).MinWidth(3).Text("3. TÓRAX").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationChest == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationChest == true ? "" : "X").Bold().FontSize(7).AlignCenter();

                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3).Text("4. ABDOMEN").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationAbdomen == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationAbdomen == true ? "" : "X").Bold().FontSize(7).AlignCenter();

                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3).Text("5. PELVIS").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationPelvis == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationPelvis == true ? "" : "X").Bold().FontSize(7).AlignCenter();

                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3).Text("6 . EXTREMIDADES").Bold().FontSize(5).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationLimbs == true ? "X" : "").Bold().FontSize(7).AlignCenter();
                                    nestedTable.Cell().BorderTop(1).BorderBottom(1).BorderLeft(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12).MinWidth(3).Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationLimbs == true ? "" : "X").Bold().FontSize(7).AlignCenter();


                                });
                            });

                            // Aquí agregamos una segunda tabla anidada que abarque todo el ancho
                            table.Cell().ColumnSpan(3).Element(CellStyle =>
                            {
                                // Crear una tabla interna con una sola columna
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderBottom(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Definir una sola columna
                                    nestedTable.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1); // Una columna que abarca todo el ancho
                                    });

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationHeadObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationNeckObs).FontSize(9).AlignStart();
                                    }

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationHeadObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationNeckObs).FontSize(9).AlignStart();
                                    }

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationChestObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationChestObs).FontSize(9).AlignStart();
                                    }

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationAbdomenObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationAbdomenObs).FontSize(9).AlignStart();
                                    }

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationPelvisObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationPelvisObs).FontSize(9).AlignStart();
                                    }

                                    if (!string.IsNullOrEmpty(consulta.Consultation.PhysicalExamination.PhysicalexaminationLimbsObs))
                                    {
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#FFFFFF").MinHeight(12).MinWidth(3)
                                            .Text(consulta.Consultation.PhysicalExamination.PhysicalexaminationLimbsObs).FontSize(9).AlignStart();
                                    }


                                });
                            });
                        });

                        //OCTAVA TABLA

                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            // Definir las columnas de la tabla principal con tamaños específicos
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(170);  // Columna 1 (Diagnóstico 1)
                                columns.ConstantColumn(95);   // Columna 2 (CIE Diagnóstico 1)
                                columns.ConstantColumn(19);   // Columna 3 (PRE Diagnóstico 1)
                                columns.ConstantColumn(20);   // Columna 4 (DEF Diagnóstico 1)
                                columns.ConstantColumn(12);   // Espacio entre diagnósticos
                                columns.ConstantColumn(193);  // Columna 5 (Diagnóstico 2)
                                columns.ConstantColumn(13);   // Columna 6 (CIE Diagnóstico 2)
                                columns.ConstantColumn(16);   // Columna 7 (PRE Diagnóstico 2)
                                columns.ConstantColumn(18);   // Columna 8 (DEF Diagnóstico 2)
                            });

                            // Fila de encabezado "8 DIAGNOSTICO"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(3).Text("8 DIAGNOSTICO").FontSize(10).Bold();

                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).Text("PRE = PRESUNTIVO\r\nDEF = DEFINITIVO").FontSize(7).Bold();

                            // Encabezados de la tabla
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("CIE").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("PRE").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("DEF").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("CIE").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("PRE").FontSize(6).Bold();
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignCenter().PaddingTop(3).MinWidth(2).Text("DEF").FontSize(6).Bold();

                            // Crear una tabla dentro de la celda para los diagnósticos
                            table.Cell().ColumnSpan(9).Element(CellStyle =>
                            {
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderBottom(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Definir columnas dentro de la subtabla para los diagnósticos
                                    nestedTable.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(14);  // Columna 1
                                        columns.ConstantColumn(250); // Columna 2
                                        columns.ConstantColumn(20);  // Columna 3
                                        columns.ConstantColumn(18);  // Columna 4
                                        columns.ConstantColumn(16);  // Columna 5
                                        columns.ConstantColumn(14);  // Columna 6
                                        columns.RelativeColumn(2);   // Columna 7
                                        columns.ConstantColumn(18);  // Columna 8
                                        columns.ConstantColumn(18);  // Columna 9
                                        columns.ConstantColumn(20);  // Columna 10
                                    });

                                    // Filtrar solo los diagnósticos relacionados con la consulta actual
                                    var diagnosticosRelacionados = consulta.Diagnoses
                                        .Where(d => consulta.Consultation.DiagnosisConsultations.Any(dc => dc.DiagnosisDiagnosisid == d.DiagnosisId))
                                        .ToList();

                                    int rowIndex = 1;
                                    for (int i = 0; i < diagnosticosRelacionados.Count; i += 2)
                                    {
                                        var diagnostico1 = diagnosticosRelacionados[i]; // Primer diagnóstico en la fila
                                        var diagnostico2 = (i + 1 < diagnosticosRelacionados.Count) ? diagnosticosRelacionados[i + 1] : null;

                                        // Buscar las consultas relacionadas con los diagnósticos
                                        var consultaDiagnostico1 = consulta.Consultation.DiagnosisConsultations.FirstOrDefault(dc => dc.DiagnosisDiagnosisid == diagnostico1.DiagnosisId);
                                        var consultaDiagnostico2 = diagnostico2 != null ? consulta.Consultation.DiagnosisConsultations.FirstOrDefault(dc => dc.DiagnosisDiagnosisid == diagnostico2.DiagnosisId) : null;

                                        // Columna 1 (Número de fila)
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3)
                                            .Text(rowIndex.ToString()).FontSize(9).AlignCenter();

                                        // Columna 2: Diagnóstico 1
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(12)
                                            .Text(diagnostico1.DiagnosisName).FontSize(9).AlignCenter();

                                        // Columna 3: CIE10 Diagnóstico 1
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(12)
                                            .Text(diagnostico1.DiagnosisCie10?.ToString() ?? "").FontSize(5).AlignCenter();

                                        // Columna 4: Presuntivo Diagnóstico 1
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12)
                                            .Text(consultaDiagnostico1?.DiagnosisPresumptive == true ? "X" : "").FontSize(9).AlignCenter();

                                        // Columna 5: Definitivo Diagnóstico 1
                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12)
                                            .Text(consultaDiagnostico1?.DiagnosisDefinitive == true ? "X" : "").FontSize(9).AlignCenter();

                                        nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ccffcc").MinHeight(12).MinWidth(3)
                                            .Text(rowIndex.ToString()).FontSize(9).AlignCenter();

                                        if (diagnostico2 != null)
                                        {
                                            // Columna 6: Diagnóstico 2 (si existe)
                                            nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(12)
                                                .Text(diagnostico2.DiagnosisName).FontSize(9).AlignCenter();

                                            // Columna 7: CIE10 Diagnóstico 2
                                            nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(12)
                                                .Text(diagnostico2.DiagnosisCie10?.ToString() ?? "").FontSize(5).AlignCenter();

                                            // Columna 8: Presuntivo Diagnóstico 2
                                            nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12)
                                                .Text(consultaDiagnostico2?.DiagnosisPresumptive == true ? "X" : "").FontSize(9).AlignCenter();

                                            // Columna 9: Definitivo Diagnóstico 2
                                            nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffff99").MinHeight(12)
                                                .Text(consultaDiagnostico2?.DiagnosisDefinitive == true ? "X" : "").FontSize(9).AlignCenter();
                                        }
                                        else
                                        {
                                            // Si no hay un segundo diagnóstico, llenar las celdas con espacios vacíos
                                            for (int j = 0; j < 4; j++)
                                            {
                                                nestedTable.Cell().Border(1).BorderColor("#ffffff").Background("#ffffff").MinHeight(12).Text("");
                                            }
                                        }

                                        rowIndex++;
                                    }



                                });
                            });
                        });


                        //Novena TABLA

                        contentColumn.Item().PaddingTop(7).Table(table =>
                        {
                            // Definir las columnas de la tabla principal con dos columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(278);  // Columna 1
                                columns.ConstantColumn(278);  // Columna 2
                            });

                            // Fila de encabezado "9 PLANES DE TRATAMIENTO"
                            table.Cell().MinHeight(14).BorderLeft(2).BorderTop(2).BorderBottom(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignLeft().PaddingTop(3).PaddingLeft(3).Text("9 PLANES DE TRATAMIENTO ").FontSize(10).Bold();

                            // Segunda columna con la descripción
                            table.Cell().MinHeight(14).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Element(CellStyle =>
                                CellStyle.Background("#ccccff")).AlignRight().PaddingTop(3).Text("REGISTRAR LOS PLANES: DIAGNOSTICO, TERAPÉUTICO Y\r\nEDUCACIONAL").FontSize(7);

                            // Subtabla debajo del encabezado que abarca todo el ancho
                            table.Cell().ColumnSpan(2).Element(CellStyle =>
                            {
                                // Crear una subtabla con una columna y cuatro filas de manera estática
                                CellStyle.Background("#ffffff").BorderLeft(2).BorderRight(2).BorderBottom(2).BorderColor("#808080").Table(nestedTable =>
                                {
                                    // Definir una sola columna en la subtabla
                                    nestedTable.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);  // Una columna que abarca todo el ancho
                                    });

                                    // Primera fila
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(20).MinWidth(3).PaddingTop(3)
                                        .Text(consulta.Consultation.ConsultationTreatmentplan).FontSize(9).AlignLeft();

                                    // Segunda fila
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(20).MinWidth(3)
                                        .Text(" ").FontSize(9).AlignLeft();

                                    // Tercera fila
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(20).MinWidth(3)
                                        .Text("").FontSize(9).AlignLeft();

                                    // Cuarta fila
                                    nestedTable.Cell().Border(1).BorderColor("#C6C2C2").Background("#ffffff").MinHeight(20).MinWidth(3)
                                        .Text("").FontSize(9).AlignLeft();
                                });
                            });
                        });

                        contentColumn.Item().PaddingTop(50).Table(table =>
                        {
                            // Definir las columnas de la tabla principal con medidas específicas en puntos (pt)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(54);  // Columna 1 (FECHA)
                                columns.ConstantColumn(57);  // Columna 2 (Valor de FECHA)
                                columns.ConstantColumn(30);  // Columna 3 (HORA)
                                columns.ConstantColumn(54);  // Columna 4 (Valor de HORA)
                                columns.ConstantColumn(57);  // Columna 5 (NOMBRE DEL PROFESIONAL)
                                columns.ConstantColumn(100); // Columna 6 (Valor del NOMBRE)
                                columns.ConstantColumn(57);  // Columna 7 (Número del Profesional)
                                columns.ConstantColumn(50);  // Columna 8 (FIRMA)
                                columns.ConstantColumn(40);  // Columna 9 (Campo vacío para FIRMA)
                                columns.ConstantColumn(30);  // Columna 10 (HOJA)
                                columns.ConstantColumn(22);  // Columna 11 (Valor de HOJA)
                            });

                            // Fila con las celdas del content que abarcan el ancho completo de la página
                            table.Cell().Element(CellStyle => CellStyle.Background("#ccffcc").Border(1)).AlignCenter().Text("FECHA").FontSize(8);
                            table.Cell().Element(CellStyle => CellStyle.Background("#FFFFFF").Border(1)).AlignCenter().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(8);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ccffcc").Border(1)).AlignCenter().Text("HORA").FontSize(8);
                            table.Cell()
          .Background("#ffffff")
          .Border(1)
          .AlignCenter()
          .Text(DateTime.Now.ToString("HH:mm"))  // Formato de hora en 24 horas
          .FontSize(8);

                            table.Cell().Element(CellStyle => CellStyle.Background("#ccffcc").Border(1)).AlignCenter().Text("NOMBRE DEL\r\nPROFESIONAL").FontSize(7);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ffffff").Border(1)).AlignCenter().Text(consulta.Consultation.UsersNames + consulta.Consultation.UsersSurcenames).FontSize(6);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ffffff").Border(1)).AlignCenter().Text("sd").FontSize(8);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ccffcc").Border(1)).AlignCenter().Text("FIRMA").FontSize(8);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ffffff").Border(1)).AlignCenter().Text("").FontSize(8);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ccffcc").Border(1)).AlignCenter().Text("NUMERO DE HOJA").FontSize(4);
                            table.Cell().Element(CellStyle => CellStyle.Background("#ffffff").Border(1)).AlignCenter().Text("1").FontSize(8);
                        });



                    });

                    // Footer de la página
                    // Footer de la página
                    page.Footer().Height(20).PaddingHorizontal(2).Row(row =>
                    {
                        // Texto a la izquierda
                        row.RelativeItem().AlignLeft().Text(text =>
                        {
                            text.Span("SNS-MSP / HCU-form.002 / 2008")
                                .FontSize(7)
                                .Bold();
                        });

                        // Texto a la derecha
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("CONSULTA EXTERNA - ANAMNESIS Y EXAMEN FÍSICO")
                                .FontSize(9)
                                .Bold();
                        });
                    });






                });
                // Segunda página

                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(598, 845);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                    // Contenido de la segunda página con una sola tabla de 5 columnas
                    page.Content().Column(column =>
                    {
                        // Primera tabla de cinco columnas
                        column.Item().Table(table =>
                        {
                            // Definir las columnas de la tabla principal con cinco columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(136);  // Columna 1
                                columns.ConstantColumn(136);  // Columna 2
                                columns.ConstantColumn(10);   // Columna 3 (espaciador)
                                columns.ConstantColumn(136);  // Columna 4
                                columns.ConstantColumn(136);  // Columna 5
                            });

                            // Fila de datos
                            table.Cell().MinHeight(15).BorderLeft(2).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#ccccff")
                                .Text("10 EVOLUCIÓN").FontSize(10).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#ccccff")
                                .Text("FIRMAR AL PIE DE CADA NOTA").FontSize(7).AlignEnd();

                            table.Cell().MinHeight(20).BorderColor("#808080").Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderColor("#808080").Background("#ccccff")
                                .Text("11 PRESCRIPCIONES").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#ccccff")
                                .Text("FIRMAR AL PIE DE CADA PRESCRIPCIÓN").FontSize(5).AlignRight();
                        });

                        // Espacio entre tablas
                        column.Item().Text("REGISTRAR EN ROJO LA ADMINISTRACIÓN DE FÁRMACOS Y OTROS PRODUCTOS (ENFERMERÍA)").FontSize(8).Light().AlignEnd();

                        // Primera tabla de cinco columnas
                        column.Item().Table(table =>
                        {
                            // Definir las columnas de la tabla principal con cinco columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);  // Columna 1
                                columns.ConstantColumn(30);  // Columna 2
                                columns.ConstantColumn(182);  // Columna 2
                                columns.ConstantColumn(13);   // Columna 3 (espaciador)
                                columns.ConstantColumn(220);  // Columna 1
                                columns.ConstantColumn(50);  // Columna 2

                            });

                            // Fila de datos
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#ccffcc")
                                .Text("\nFECHA\r\n(DIA/MES/AÑO)").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#ccffcc")
                                .Text("\nHORA").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#ccffcc")
                                .Text("\nNOTAS DE EVOLUCIÓN").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#ccffcc")
                                .Text("FARMACOTERAPIA E INDICACIONES\r\n(PARA ENFERMERÍA Y OTRO PERSONAL)").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#ccffcc")
                                .Text("ADMINISTR.\r\nFÁRMACOS\r\nY OTROS").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
   .AlignCenter() // ✅ Aplicar alineación al contenedor, no al texto
  .Text(consulta.Consultation.ConsultationCreationdate.GetValueOrDefault().ToString("dd/MM/yyyy"));



                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text(consulta.Consultation.ConsultationEvolutionNotes).FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                       .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderLeft(2).BorderRight(1).BorderTop(2).BorderBottom(2).BorderColor("#808080").Background("#FFFFFF")
                          .Text("").FontSize(8).AlignCenter();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderRight(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter().Bold();

                            table.Cell().MinHeight(15).BorderColor("#808080").BorderLeft(2).BorderRight(2).Background("#ffffff")
                                .Text("").FontSize(9).AlignLeft().Bold();

                            table.Cell().MinHeight(15).BorderTop(2).BorderBottom(2).BorderLeft(2).BorderRight(1).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();

                            table.Cell().MinHeight(20).BorderRight(2).BorderBottom(2).BorderTop(2).BorderColor("#808080").Background("#FFFFFF")
                                .Text("").FontSize(7).AlignCenter();
                        });

                    });




                    // Footer de la segunda página
                    page.Footer().Height(20).PaddingHorizontal(2).Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text(text =>
                        {
                            text.Span("SNS-MSP / HCU-form.002 / 2008").FontSize(7).Bold();
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("CONSULTA EXTERNA - ANAMNESIS Y EXAMEN FÍSICO").FontSize(9).Bold();
                        });
                    });
                });


            });

            byte[] pdfBytes = document.GeneratePdf();

            // Devuelve el archivo PDF al navegador.
            return File(pdfBytes, "application/pdf", "Formulario_Consulta.pdf");
        }


        // Helper class for Family Conditions (can be placed inside your controller or as a private static nested class)
  

        public async Task<IActionResult> MedicalForm2(int consultationId)
        {
            var consultation = _consultationService.GetConsultationDetails(consultationId);
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var patient = await _patientService.GetPatientFullByIdAsync(consultation.ConsultationPatient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "nuevo_formato_consulta.pdf");

            using var memoryStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(templatePath);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, memoryStream);
            AcroFields formFields = pdfStamper.AcroFields;

            // Insertar logo del establecimiento si existe
            // Assuming InsertImageFromField is a method available in your context
            // Example: private void InsertImageFromField(PdfStamper stamper, AcroFields fields, string fieldName, string imagePath) { /* ... */ }
            // If not, you might need to provide its implementation or remove this line if not used.
            // InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo", consultation.UsersEstablishmentLogo); 
            formFields.SetField("txt_institucion_sistema", consultation.EstablishmentName);
            formFields.SetField("txt_unicode",consultation.EstablishmentUnicode);
            formFields.SetField("txt_establecimiento_salud", consultation.EstablishmentName);
            formFields.SetField("txt_numero_historia_clinica", patient.PatientDocumentnumber);
            formFields.SetField("txt_numero_hoja", "1");
            int status = (int)consultation.ConsultationStatus;

            formFields.SetField("txt_primera_consulta", status == 1 ? "X" : "");
            formFields.SetField("txt_segunda_consulta", status == 2 ? "X" : "");

            // DATOS DEL PACIENTE
            formFields.SetField("txt_primer_apellido_paciente", patient.PatientFirstsurname);
            formFields.SetField("txt_segundo_apellido_paciente", patient.PatientSecondlastname);
            formFields.SetField("txt_primer_nombre_paciente", patient.PatientFirstname);
            formFields.SetField("txt_segundo_nombre_paciente", patient.PatientMiddlename);
            formFields.SetField("txt_edad", patient.PatientAge.ToString());
            formFields.SetField("txt_cedula", patient.PatientDocumentnumber);
            string sexo = patient.PatientGenderName switch
            {
                "Masculino" => "M",
                "Femenino" => "F",
                _ => ""
            };
            formFields.SetField("txt_sexo", sexo);

            // MOTIVO Y ENFERMEDAD
            formFields.SetField("txt_motivo_consulta", consultation.ConsultationReason ?? "N/A");
            formFields.SetField("txt_enfermedad_problema", consultation.ConsultationDisease ?? "N/A");
            // ======================
            // FECHA Y HORA DE CONSULTA
            // ======================
            if (consultation.ConsultationCreationdate.HasValue)
            {
                var fecha = consultation.ConsultationCreationdate.Value.ToString("dd/MM/yyyy");
                var hora = consultation.ConsultationCreationdate.Value.ToString("HH:mm");

                formFields.SetField("txt_fecha_sig", fecha);
                formFields.SetField("txt_hora_sig", hora);
            }
            else
            {
                formFields.SetField("txt_fecha_sig", "N/A");
                formFields.SetField("txt_hora_sig", "N/A");
            }

            // CONSTANTES VITALES

            formFields.SetField("txt_temp_sig", consultation.ConsultationTemperature ?? "N/A");
            formFields.SetField("txt_art_sis_sig", consultation.ConsultationBloodpressuredAs ?? "N/A");
            formFields.SetField("txt_art_dis_sig", consultation.ConsultationBloodpresuredDis ?? "N/A");
            formFields.SetField("txt_pulso_sig", consultation.ConsultationPulse ?? "N/A");
            formFields.SetField("txt_frecuencia_sig", consultation.ConsultationRespirationrate ?? "N/A");
            formFields.SetField("txt_peso_sig", consultation.ConsultationWeight ?? "N/A");
            formFields.SetField("txt_talla_sig", consultation.ConsultationSize ?? "N/A");
            formFields.SetField("txt_ICM_sig", consultation.ConsultationImc?.ToString() ?? "N/A");
            formFields.SetField("txt_perimetro_abdominal_sig", consultation.ConsultationAbdominalPerimeter?.ToString() ?? "N/A");
            formFields.SetField("txt_hemoglobina_capilar_sig", consultation.ConsultationCapillaryHemoglobin?.ToString() ?? "N/A");
            formFields.SetField("txt_glucosa_capilar_sig", consultation.ConsultationCapillaryGlucose?.ToString() ?? "N/A");
            formFields.SetField("txt_pulsioximetria_sig", consultation.ConsultationSpo2?.ToString() ?? "N/A");

            // ======================
            // ANTECEDENTES PERSONALES
            // ======================
            var pb = consultation.PersonalBackground;
            var antecedentesPersonales = new List<string>();

            void SetPersonalField(string pdfField, string displayName, bool? flag, string obs)
            {
                // Marca con "X" si está activo
                formFields.SetField(pdfField, flag == true ? "X" : "");

                // Si está activo y tiene observación, la añadimos a la lista
                if (flag == true && !string.IsNullOrWhiteSpace(obs))
                    antecedentesPersonales.Add($"{displayName}: {obs}");
            }

            // Condiciones personales
            SetPersonalField("txt_cardiopatia", "Cardiopatía", pb?.PersonalBackgroundHeartdisease, pb?.PersonalBackgroundHeartdiseaseObservation);
            SetPersonalField("txt_hipertension", "Hipertensión", pb?.PersonalBackgroundHypertension, pb?.PersonalBackgroundHypertensionObservation);
            SetPersonalField("txt_enf_cardiovascular", "Enfermedad cardiovascular", pb?.PersonalBackgroundDxcardiovascular, pb?.PersonalBackgroundDxcardiovascularObservation);
            SetPersonalField("txt_end_metabolico", "Endocrino-metabólico", pb?.PersonalBackgroundEndometabolic, pb?.PersonalBackgroundEndometabolicObservation);
            SetPersonalField("txt_cancer", "Cáncer", pb?.PersonalBackgroundCancer, pb?.PersonalBackgroundCancerObservation);
            SetPersonalField("txt_tuberculosis", "Tuberculosis", pb?.PersonalBackgroundTuberculosis, pb?.PersonalBackgroundTuberculosisObservation);
            SetPersonalField("txt_enf_mental", "Enfermedad mental", pb?.PersonalBackgroundDxmental, pb?.PersonalBackgroundDxmentalObservation);
            SetPersonalField("txt_enf_infecciosa", "Enfermedad infecciosa", pb?.PersonalBackgroundDxinfectious, pb?.PersonalBackgroundDxinfectiousObservation);
            SetPersonalField("txt_mal_formacion", "Malformación", pb?.PersonalBackgroundMalformation, pb?.PersonalBackgroundMalformationObservation);
            SetPersonalField("txt_otro", "Otro", pb?.PersonalBackgroundOther, pb?.PersonalBackgroundOtherObservation);

            // ======================
            // ALERGIAS Y CIRUGÍAS
            // ======================
            var alergiasCirugias = new List<string>();

            // Alergias
            if (consultation.AllergiesConsultations != null && consultation.AllergiesConsultations.Count > 0)
            {
                foreach (var a in consultation.AllergiesConsultations)
                {
                    // Si tiene observación, la mostramos
                    if (!string.IsNullOrWhiteSpace(a.AllergiesObservation))
                        alergiasCirugias.Add($"Alergia: {a.AllergiesObservation}");
                }
            }

            // Cirugías
            if (consultation.SurgeriesConsultations != null && consultation.SurgeriesConsultations.Count > 0)
            {
                foreach (var c in consultation.SurgeriesConsultations)
                {
                    if (!string.IsNullOrWhiteSpace(c.SurgeriesObservation))
                        alergiasCirugias.Add($"Cirugía: {c.SurgeriesObservation}");
                }
            }

            // Construimos texto horizontal y línea visual debajo
            var textoAlergiasCirugias = alergiasCirugias.Count > 0
                ? string.Join(", ", alergiasCirugias) // Une todas las entradas separadas por ", "
                : "N/A"; // Si no hay nada, muestra "N/A"

            // ======================
            // ASIGNACIÓN AL PDF 
            // ======================
            formFields.SetField("txt_alergias_cirugias", textoAlergiasCirugias);

       
            // Resumen horizontal separado por comas
            formFields.SetField("txt_antecedentes_personales",
                antecedentesPersonales.Count > 0 ? string.Join(", ", antecedentesPersonales) : "N/A");

            // ======================
            // ANTECEDENTES FAMILIARES
            // ======================
            var fam = consultation.FamiliaryBackground;
            var antecedentesFamiliares = new List<string>();

            void SetFamilyField(
                string pdfField,
                string displayName,
                bool? flag,
                string? obs,
                string? relatshName
            )
            {
                // Marca con "X" el campo del PDF si hay antecedente
                formFields.SetField(pdfField, flag == true ? "X" : "");

                // Solo agregamos al resumen si está activo
                if (flag == true)
                {
                    var texto = displayName;

                    // Agregar parentesco si viene de catálogo
                    if (!string.IsNullOrWhiteSpace(relatshName))
                        texto += $" ({relatshName})";

                    // Agregar observación si existe
                    if (!string.IsNullOrWhiteSpace(obs))
                        texto += $": {obs}";

                    antecedentesFamiliares.Add(texto);
                }
            }

            // Campos individuales del PDF
            SetFamilyField(
                "txt_cardiopatia_familiar",
                "Cardiopatía",
                fam?.FamiliaryBackgroundHeartdisease,
                fam?.FamiliaryBackgroundHeartdiseaseObservation,
                fam?.RelatshHeartdiseaseName
            );

            SetFamilyField(
                "txt_hipertension_familiar",
                "Hipertensión",
                fam?.FamiliaryBackgroundHypertension,
                fam?.FamiliaryBackgroundHypertensionObservation,
                fam?.RelatshHypertensionName
            );

            SetFamilyField(
                "txt_enf_cardiovascular_familiar",
                "Enfermedad cardiovascular",
                fam?.FamiliaryBackgroundDxcardiovascular,
                fam?.FamiliaryBackgroundDxcardiovascularObservation,
                fam?.RelatshDxcardiovascularName
            );

            SetFamilyField(
                "txt_end_metabolico_familiar",
                "Endocrino–metabólico",
                fam?.FamiliaryBackgroundDiabetes,
                fam?.FamiliaryBackgroundDiabetesObservation,
                fam?.RelatshDiabetesName
            );

            SetFamilyField(
                "txt_cancer_familiar",
                "Cáncer",
                fam?.FamiliaryBackgroundCancer,
                fam?.FamiliaryBackgroundCancerObservation,
                fam?.RelatshCancerName
            );

            SetFamilyField(
                "txt_tuberculosis_familiar",
                "Tuberculosis",
                fam?.FamiliaryBackgroundTuberculosis,
                fam?.FamiliaryBackgroundTuberculosisObservation,
                fam?.RelatshTuberculosisName
            );

            SetFamilyField(
                "txt_enf_mental_familiar",
                "Enfermedad mental",
                fam?.FamiliaryBackgroundDxmental,
                fam?.FamiliaryBackgroundDxmentalObservation,
                fam?.RelatshDxmentalName
            );

            SetFamilyField(
                "txt_enf_infecciosa_familiar",
                "Enfermedad infecciosa",
                fam?.FamiliaryBackgroundDxinfectious,
                fam?.FamiliaryBackgroundDxinfectiousObservation,
                fam?.RelatshDxinfectiousName
            );

            SetFamilyField(
                "txt_mal_formacion_familiar",
                "Malformación",
                fam?.FamiliaryBackgroundMalformation,
                fam?.FamiliaryBackgroundMalformationObservation,
                fam?.RelatshMalformationName
            );

            SetFamilyField(
                "txt_otro_familiar",
                "Otro",
                fam?.FamiliaryBackgroundOther,
                fam?.FamiliaryBackgroundOtherObservation,
                fam?.RelatshOtherName
            );

            // Resumen horizontal en txt_antecedentes_familiares
            var textoFamiliares = antecedentesFamiliares.Count > 0
                ? string.Join(", ", antecedentesFamiliares)
                : "N/A";

            formFields.SetField("txt_antecedentes_familiares", textoFamiliares);



            // ==============================
            // REVISIÓN DE ÓRGANOS Y SISTEMAS
            // ==============================
            var organs = consultation.OrgansSystem;
            var revisionOrganos = new List<string>();

            void SetOrganField(string pdfField, string displayName, string? obs)
            {
                // Si tiene valor, marcar con "X"; si no, dejar vacío
                var textoCampo = string.IsNullOrWhiteSpace(obs) ? "" : "X";

                // Establecer campo en el PDF
                formFields.SetField(pdfField, textoCampo);

                // Agregar al resumen solo si hay texto original
                if (!string.IsNullOrWhiteSpace(obs))
                    revisionOrganos.Add($"{displayName}: {obs.Trim()}");
            }


            // Campos individuales
            SetOrganField(
                "txt_piel_organos_sistemas",
                "Piel y anexos",
                organs?.OrganssystemsSkinObs
            );

            SetOrganField(
                "txt_respiratorio_organos_sistemas",
                "Respiratorio",
                organs?.OrganssystemsRespiratoryObs
            );

            SetOrganField(
                "txt_digestivo_organos_sistemas",
                "Digestivo",
                organs?.OrganssystemsDigestiveObs
            );

            SetOrganField(
                "txt_musculo_esqueletico_organos_sistemas",
                "Músculo–esquelético",
                organs?.OrganssystemsSkeletalMObs
            );

            SetOrganField(
                "txt_hemo_linfatico_organos_sistemas",
                "Hemo–linfático",
                organs?.OrganssystemsLymphaticObs
            );

            SetOrganField(
                "txt_osentidos_organos_sistemas",  // ajusta al nombre real del campo
                "Órganos de los sentidos",
                organs?.OrganssystemsOrgansensesObs
            );

            SetOrganField(
                "txt_cardio_vascular_sistemas",
                "Cardio-vascular",
                organs?.OrganssystemsCardiovascularObs
            );

            SetOrganField(
                "txt_genito_urinario_sistemas",
                "Genito-urinario",
                organs?.OrganssystemsGenitalObs
            );

            SetOrganField(
                "txt_endocrino_sistemas",
                "Endocrino",
                organs?.OrganssystemsEndocrine
            );

            SetOrganField(
                "txt_nervioso_sistemas",
                "Nervioso",
                organs?.OrganssystemsNervousObs
            );

            // Resumen horizontal
            var textoRevision = revisionOrganos.Count > 0
                ? string.Join(" | ", revisionOrganos)
                : "N/A";

            formFields.SetField("txt_revision_organos_sistemas", textoRevision);


            // ======================
            // EXAMEN FÍSICO
            // ======================
            var ex = consultation.PhysicalExamination;
            var examenFisico = new List<string>();

            // Usa el mismo 'organs' que ya definiste en la sección de
            // REVISIÓN DE ÓRGANOS Y SISTEMAS:
            // var organs = consultation.OrgansSystem;

            void SetPhysicalField(string pdfField, string displayName, string? obs)
            {
                // Si hay valor, colocar "X"; si no, dejar vacío
                var textoCampo = string.IsNullOrWhiteSpace(obs) ? "" : "X";

                // Establecer el campo en el PDF
                formFields.SetField(pdfField, textoCampo);

                // Agregar al resumen solo si hay texto original
                if (!string.IsNullOrWhiteSpace(obs))
                    examenFisico.Add($"{displayName}: {obs.Trim()}");
            }


            // Parte “anatómica” del examen físico (usa PhysicalExamination)
            SetPhysicalField("txt_piel_faneras_examenfisico", "Piel y faneras", ex?.PhysicalexaminationSkinfanerasObs);
            SetPhysicalField("txt_cabeza_examenfisico", "Cabeza", ex?.PhysicalexaminationHeadObs);
            SetPhysicalField("txt_ojos_examenfisico", "Ojos", ex?.PhysicalexaminationEyesObs);
            SetPhysicalField("txt_oidos_examenfisico", "Oídos", ex?.PhysicalexaminationEarsObs);
            SetPhysicalField("txt_nariz_examenfisico", "Nariz", ex?.PhysicalexaminationNoseObs);
            SetPhysicalField("txt_boca_examenfisico", "Boca", ex?.PhysicalexaminationMouthObs);
            SetPhysicalField("txt_orofaringe_examenfisico", "Orofaringe", ex?.PhysicalexaminationOropharynxObs);
            SetPhysicalField("txt_cuello_examenfisico", "Cuello", ex?.PhysicalexaminationNeckObs);
            SetPhysicalField("txt_axilas_examenfisico", "Axilas", ex?.PhysicalexaminationAxilasmamasObs);
            SetPhysicalField("txt_torax_examenfisico", "Tórax", ex?.PhysicalexaminationChestObs);
            SetPhysicalField("txt_abdomen_examenfisico", "Abdomen", ex?.PhysicalexaminationAbdomenObs);
            SetPhysicalField("txt_columna_vertebral_examenfisico", "Columna vertebral", ex?.PhysicalexaminationSpineObs);
            SetPhysicalField("txt_ingle_perine_examenfisico", "Ingle y periné", ex?.PhysicalexaminationIngleperineObs);
            SetPhysicalField("txt_miembros_superiores_examenfisico", "Miembros superiores", ex?.PhysicalexaminationUpperlimbsObs);
            SetPhysicalField("txt_miembros_inferiores_examenfisico", "Miembros inferiores", ex?.PhysicalexaminationLowerlimbsObs);

            // Parte “por sistemas” del examen físico (reutiliza OrgansSystem)
            SetPhysicalField("txt_organos_sentidos_examenfisico", "Órganos de los sentidos",
                organs?.OrganssystemsOrgansensesObs);

            SetPhysicalField("txt_respiratorio_examenfisico", "Respiratorio",
                organs?.OrganssystemsRespiratoryObs);

            SetPhysicalField("txt_cardioascular_examenfisico", "Cardio-vascular",
                organs?.OrganssystemsCardiovascularObs);

            SetPhysicalField("txt_digestivo_examenfisico", "Digestivo",
                organs?.OrganssystemsDigestiveObs);

            SetPhysicalField("txt_genital_examenfisico", "Genital",
                organs?.OrganssystemsGenitalObs);

            // Si no tienes campo específico urinario, se puede mapear al mismo “Genito-urinario”
            SetPhysicalField("txt_urinario_examenfisico", "Urinario",
                organs?.OrganssystemsGenitalObs);

            SetPhysicalField("txt_musculo_esqueletico_examenfisico", "Músculo-esquelético",
                organs?.OrganssystemsSkeletalMObs);

            SetPhysicalField("txt_endocrino_examenfisico", "Endocrino",
                organs?.OrganssystemsEndocrine);

            SetPhysicalField("txt_hemo_linfatico_examenfisico", "Hemo-linfático",
                organs?.OrganssystemsLymphaticObs);

            SetPhysicalField("txt_neurologico_examenfisico", "Neurológico",
                organs?.OrganssystemsNervousObs);

            // Resumen horizontal
            var textoExamenFisico = examenFisico.Count > 0
                ? string.Join(" | ", examenFisico)
                : "N/A";

            formFields.SetField("txt_examen_fisico", textoExamenFisico);


            // Obtener todos los IDs de diagnóstico relacionados con la consulta
            var relatedDiagIds = consultation.DiagnosisConsultations
                .Select(dc => dc.DiagnosisDiagnosisid)
                .Distinct()
                .ToList();


            // =======================================
            // 1. CARGA DE DATOS
            // =======================================
            // Carga todos los diagnósticos disponibles para lookup (esto resuelve el error 'diagnosisList' no existe)
            var diagnosisList = await _selectService.GetAllDiagnosisAsync();


            // =======================================
            // 2. PREPARACIÓN DE LA LISTA FINAL (Máx. 6)
            // =======================================

            // Combina el estado de la consulta (Presuntivo/Definitivo) con los detalles del diagnóstico (Nombre/CIE10)
            // Los diagnósticos originales d.DiagnosisName y d.DiagnosisCie10 provienen de la lista cargada.
            var finalDiags = (from dc in consultation.DiagnosisConsultations // Tiene el estado Presuntivo/Definitivo
                              join d in diagnosisList on dc.DiagnosisDiagnosisid equals d.DiagnosisId // Obtiene los detalles
                              select new
                              {
                                  DiagnosisName = d.DiagnosisName,
                                  DiagnosisCie10 = d.DiagnosisCie10,
                                  IsPresumptive = dc.DiagnosisPresumptive,
                                  IsDefinitive = dc.DiagnosisDefinitive
                              })
                              .Take(6) // Limitar a los 6 slots del PDF
                              .ToList();


            // =======================================
            // 3. LLENADO DEL FORMULARIO PDF
            // =======================================

            // Nombres EXACTOS de los campos del PDF
            string[] diagFields = { "txt_diganostico_1", "txt_diganostico_2", "txt_diganostico_3", "txt_diganostico_4", "txt_diganostico_5", "txt_diganostico_6" };
            string[] cie10Fields = { "txt_cie10_1", "txt_cie10_2", "txt_cie10_3", "txt_cie10_4", "txt_cie10_5", "txt_cie10_6" };
            string[] preFields = { "txt_pre_1", "txt_pre_2", "txt_pre_3", "txt_pre_4", "txt_pre_5", "txt_pre_6" };
            string[] defFields = { "txt_def_1", "txt_def_2", "txt_def_3", "txt_def_4", "txt_def_5", "txt_def_6" };

            int totalSlots = diagFields.Length;

            for (int i = 0; i < totalSlots; i++)
            {
                if (i < finalDiags.Count)
                {
                    var d = finalDiags[i];

                    // Rellenar con los datos combinados
                    formFields.SetField(diagFields[i], d.DiagnosisName ?? "");
                    formFields.SetField(cie10Fields[i], d.DiagnosisCie10?.ToString() ?? "");

                    // Presuntivo / Definitivo (marcar con 'X')
                    formFields.SetField(preFields[i], d.IsPresumptive == true ? "X" : "");
                    formFields.SetField(defFields[i], d.IsDefinitive == true ? "X" : "");
                }
                else
                {
                    // Limpiar los slots no utilizados
                    formFields.SetField(diagFields[i], "");
                    formFields.SetField(cie10Fields[i], "");
                    formFields.SetField(preFields[i], "");
                    formFields.SetField(defFields[i], "");
                }
            }

            // PLAN DE TRATAMIENTO
            formFields.SetField("txt_plan_tratamiento", consultation.ConsultationTreatmentplan ?? "N/A");

            // MÉDICO
            // Obtener nombres y apellidos del usuario (médico)
            string fullNames = consultation.UsersNames?.Trim() ?? string.Empty;
            string fullSurnames = consultation.UsersSurcenames?.Trim() ?? string.Empty;

            // --- 1. PROCESAR NOMBRES (txt_primer_nombre_medico) ---
            // Dividir la cadena de nombres por el espacio.
            string[] namesArray = fullNames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Asignar el primer nombre. Si no hay nombres, será vacío.
            string primerNombre = namesArray.Length > 0 ? namesArray[0] : string.Empty;

            // Asignar al PDF (el PDF solo pide el primer nombre, así que ignoramos el segundo nombre)
            formFields.SetField("txt_primer_nombre_medico", primerNombre);

            // --- 2. PROCESAR APELLIDOS (txt_primer_apellido_medico y txt_segundo_apellido_medico) ---
            // Dividir la cadena de apellidos por el espacio.
            string[] surnamesArray = fullSurnames.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Asignar el primer apellido.
            string primerApellido = surnamesArray.Length > 0 ? surnamesArray[0] : string.Empty;

            // Asignar el segundo apellido.
            // Si hay un segundo elemento en el array, tómalo; de lo contrario, deja la cadena vacía.
            string segundoApellido = surnamesArray.Length > 1 ? surnamesArray[1] : string.Empty;

            // Asignar al PDF
            formFields.SetField("txt_primer_apellido_medico", primerApellido);
            formFields.SetField("txt_segundo_apellido_medico", segundoApellido);
            formFields.SetField("txt_email", consultation.UsersEmail ?? "");
            formFields.SetField("txt_telefono", consultation.UsersPhone ?? "");
            formFields.SetField("txt_direccion", consultation.EstablishmentAddress ?? "");
            formFields.SetField("txt_identificacion_medico", consultation.UsersDocumentNumber ?? "");

            // FECHA Y HORA
            formFields.SetField("txt_fecha_final", consultation.ConsultationCreationdate?.ToString("yyyy-MM-dd") ?? "N/A");
            formFields.SetField("txt_horal_final", consultation.ConsultationCreationdate?.ToString("HH:mm") ?? "N/A");

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();

            var randomNumber = new Random().Next(1000, 9999);
            return File(memoryStream.ToArray(), "application/pdf", $"formulario_consulta_{randomNumber}.pdf");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consultationId"></param>
        /// <returns></returns>
        public async Task<IActionResult> MedicationRecipe(int consultationId)
        {
            var consultation = _consultationService.GetConsultationDetails(consultationId);
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var patient = await _patientService.GetPatientFullByIdAsync(consultation.ConsultationPatient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "receta_expermed.pdf");

            byte[] pdfBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var pdfReader = new PdfReader(templatePath))
                using (var pdfStamper = new PdfStamper(pdfReader, memoryStream))
                {
                    AcroFields formFields = pdfStamper.AcroFields;

                    InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo", consultation.UsersEstablishmentLogo);
                    InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo_2", consultation.UsersEstablishmentLogo);

                    // Asignar valores a campos de la plantilla PDF

             
                    string datosMedico = ConstruirDatosMedico(consultation);
                    formFields.SetField("txt_datos_medico", datosMedico);

                    formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                        ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                        : "N/A");

                    formFields.SetField("txt_apellido", patient.PatientFirstsurname + " " + patient.PatientSecondlastname);
                    formFields.SetField("txt_nombres", patient.PatientFirstname + " " + patient.PatientMiddlename);
                    formFields.SetField("txt_edad", patient.PatientAge.ToString());
                    formFields.SetField("txt_cedula", patient.PatientDocumentnumber);
                    formFields.SetField("txt_cedula_medico", consultation.UsersDocumentNumber);

                    // =========================
                    // DIAGNÓSTICOS - LISTAR TODOS
                    // =========================
                    var diagnosisList = await _selectService.GetAllDiagnosisAsync();

                    // Diagnósticos Definitivos (TODOS)
                    var diagnosisDefNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisDefinitive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Diagnósticos Presuntivos (TODOS)
                    var diagnosisPreNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisPresumptive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Formatear diagnósticos
                    string diagnosticosTexto = "";

                    if (diagnosisDefNames.Any() || diagnosisPreNames.Any())
                    {
                        var partes = new List<string>();

                        if (diagnosisDefNames.Any())
                            partes.Add($"Definitivo: {string.Join(", ", diagnosisDefNames)}");

                        if (diagnosisPreNames.Any())
                            partes.Add($"Presuntivo: {string.Join(", ", diagnosisPreNames)}");

                        diagnosticosTexto = string.Join(" | ", partes);
                    }
                    else
                    {
                        diagnosticosTexto = "N/A";
                    }

                    formFields.SetField("txt_diagnosticos", diagnosticosTexto);

                    // =========================
                    // MEDICAMENTOS
                    // =========================
                    var allMedications = await _selectService.GetAllMedicationsAsync();

                    var medicationsInfo = consultation.MedicationsConsultations?.Any() == true
                        ? string.Join("\n", consultation.MedicationsConsultations.Select(mc =>
                        {
                            var medication = allMedications.FirstOrDefault(m => m.MedicationsId == mc.MedicationsMedicationsid);
                            return medication != null
                                ? $"({medication.MedicationsCie10}) {medication.MedicationsDescription} - Cantidad: {mc.MedicationsAmount}"
                                : "N/A";
                        }))
                        : "No se prescribieron medicamentos";

                    formFields.SetField("txt_prescripcion", medicationsInfo);

                    // =========================
                    // INDICACIONES
                    // =========================
                    var indications = consultation.MedicationsConsultations?.Any() == true
                        ? string.Join("\n", consultation.MedicationsConsultations.Select(mc =>
                        {
                            var medication = allMedications.FirstOrDefault(m => m.MedicationsId == mc.MedicationsMedicationsid);
                            return medication != null
                                ? $"({medication.MedicationsCie10}) {medication.MedicationsDescription} - Observaciones: {mc.MedicationsObservation ?? "Sin observaciones"}"
                                : "N/A";
                        }))
                        : "Sin indicaciones";

                    formFields.SetField("txt_observacion", indications);

                    // =========================
                    // ALERGIAS
                    // =========================
                    var allergiesList = await _selectService.GetAllergiesTypeAsync();

                    var consultationAllergyIds = consultation.AllergiesConsultations?
                        .Select(ac => ac.AllergiesCatalogid)
                        .ToList();

                    var filteredAllergies = (consultationAllergyIds != null && consultationAllergyIds.Any())
                        ? allergiesList.Where(c => consultationAllergyIds.Contains(c.CatalogId))
                        : Enumerable.Empty<Catalog>();

                    var allergiesText = filteredAllergies.Any()
                        ? string.Join(", ", filteredAllergies.Select(a => a.CatalogName))
                        : "N/A";

                    formFields.SetField("txt_alergias", allergiesText);

                    // =========================
                    // OTROS CAMPOS
                    // =========================
                    var sequential = consultation.MedicationsConsultations?.FirstOrDefault()?.MedicationsSequential ?? 0;
                    formFields.SetField("txt_secuencial", sequential.ToString());

                    formFields.SetField("txt_rec_no_farma", consultation.ConsultationNonpharmacologycal ?? "N/A");
                    formFields.SetField("txt_direccion", consultation.EstablishmentAddress ?? consultation.UsersEstablishmentAddress ?? "N/A");

                    // Finalizar PDF
                    pdfStamper.FormFlattening = true;
                    pdfStamper.Close();
                }

                pdfBytes = memoryStream.ToArray();
            }

            var randomNumber = new Random().Next(1000, 9999);
            return File(pdfBytes, "application/pdf", $"receta_medicacion_{randomNumber}.pdf");
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="consultationId"></param>
        /// <returns></returns>

        public async Task<IActionResult> LaboratoryDoc(int consultationId)
        {
            var consultation = _consultationService.GetConsultationDetails(consultationId);
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var patient = await _patientService.GetPatientFullByIdAsync(consultation.ConsultationPatient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "laboratorio_expermed.pdf");

            byte[] pdfBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var pdfReader = new PdfReader(templatePath))
                using (var pdfStamper = new PdfStamper(pdfReader, memoryStream))
                {
                    AcroFields formFields = pdfStamper.AcroFields;
                    InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo_2", consultation.UsersEstablishmentLogo);

                    // Asignar valores a campos de la plantilla PDF
                    string datosMedico = ConstruirDatosMedico(consultation);
                    formFields.SetField("txt_datos_medico", datosMedico);

                    formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                        ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                        : "N/A");

                    formFields.SetField("txt_apellido", $"{patient.PatientFirstsurname ?? ""} {patient.PatientSecondlastname ?? ""}".Trim());
                    formFields.SetField("txt_nombres", $"{patient.PatientFirstname ?? ""} {patient.PatientMiddlename ?? ""}".Trim());
                    formFields.SetField("txt_edad", patient.PatientAge.ToString() ?? "N/A");
                    formFields.SetField("txt_cedula", patient.PatientDocumentnumber ?? "N/A");
                    formFields.SetField("txt_cedula_medico", consultation.UsersDocumentNumber ?? "N/A");

                    // =========================
                    // DIAGNÓSTICOS - LISTAR TODOS
                    // =========================
                    var diagnosisList = await _selectService.GetAllDiagnosisAsync();

                    // Diagnósticos Definitivos (TODOS)
                    var diagnosisDefNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisDefinitive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Diagnósticos Presuntivos (TODOS)
                    var diagnosisPreNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisPresumptive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Formatear diagnósticos
                    string diagnosticosTexto = "";

                    if (diagnosisDefNames.Any() || diagnosisPreNames.Any())
                    {
                        var partes = new List<string>();

                        if (diagnosisDefNames.Any())
                            partes.Add($"Definitivo: {string.Join(", ", diagnosisDefNames)}");

                        if (diagnosisPreNames.Any())
                            partes.Add($"Presuntivo: {string.Join(", ", diagnosisPreNames)}");

                        diagnosticosTexto = string.Join(" | ", partes);
                    }
                    else
                    {
                        diagnosticosTexto = "N/A";
                    }

                    formFields.SetField("txt_diagnosticos", diagnosticosTexto);

                    // =========================
                    // LABORATORIOS
                    // =========================
                    var allLabs = await _selectService.GetAllLaboratoriesAsync();

                    var laboratoriesInfo = consultation.LaboratoriesConsultations?.Any() == true
                        ? string.Join("\n", consultation.LaboratoriesConsultations.Select(lc =>
                        {
                            var lab = allLabs.FirstOrDefault(l => l.LaboratoriesId == lc.LaboratoriesLaboratoriesid);
                            return lab != null
                                ? $"({lab.LaboratoriesCie10}) {lab.LaboratoriesName} - Cantidad: {lc.LaboratoriesAmount}"
                                : "N/A";
                        }))
                        : "No se solicitaron laboratorios";

                    formFields.SetField("txt_laboratorios", laboratoriesInfo);

                    // =========================
                    // OBSERVACIONES - TODAS LAS OBSERVACIONES
                    // =========================
                    var observaciones = consultation.LaboratoriesConsultations?
                        .Where(lc => !string.IsNullOrWhiteSpace(lc.LaboratoriesObservation))
                        .Select(lc => lc.LaboratoriesObservation)
                        .ToList() ?? new List<string>();

                    string observacionesTexto = observaciones.Any()
                        ? string.Join("\n", observaciones)
                        : "Sin observaciones";

                    formFields.SetField("txt_observaciones", observacionesTexto);

                    formFields.SetField("txt_direccion", consultation.UsersEstablishmentAddress ?? "N/A");

                    // Finalizar PDF
                    pdfStamper.FormFlattening = true;
                    pdfStamper.Close();
                }

                pdfBytes = memoryStream.ToArray();
            }

            var randomNumber = new Random().Next(1000, 9999);
            return File(pdfBytes, "application/pdf", $"pedido_laboratorio_{randomNumber}.pdf");
        }

        public async Task<IActionResult> ImageDoc(int consultationId)
        {
            var consultation = _consultationService.GetConsultationDetails(consultationId);
            if (consultation == null)
            {
                TempData["ErrorMessage"] = "Consulta no encontrada.";
                return RedirectToAction("Index", "Home");
            }

            var patient = await _patientService.GetPatientFullByIdAsync(consultation.ConsultationPatient);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Paciente no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "plantillas", "imagenologia_expermed.pdf");

            byte[] pdfBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var pdfReader = new PdfReader(templatePath))
                using (var pdfStamper = new PdfStamper(pdfReader, memoryStream))
                {
                    AcroFields formFields = pdfStamper.AcroFields;
                    InsertImageFromField(pdfStamper, formFields, "txt_imagen_logo_2", consultation.UsersEstablishmentLogo);

                    // Asignar valores a campos de la plantilla PDF
                    string datosMedico = ConstruirDatosMedico(consultation);
                    formFields.SetField("txt_datos_medico", datosMedico);

                    formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                        ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                        : "N/A");

                    formFields.SetField("txt_apellido", $"{patient.PatientFirstsurname ?? ""} {patient.PatientSecondlastname ?? ""}".Trim());
                    formFields.SetField("txt_nombres", $"{patient.PatientFirstname ?? ""} {patient.PatientMiddlename ?? ""}".Trim());
                    formFields.SetField("txt_edad", patient.PatientAge.ToString() ?? "N/A");
                    formFields.SetField("txt_cedula", patient.PatientDocumentnumber ?? "N/A");

                    // =========================
                    // DIAGNÓSTICOS - LISTAR TODOS
                    // =========================
                    var diagnosisList = await _selectService.GetAllDiagnosisAsync();

                    // Diagnósticos Definitivos (TODOS)
                    var diagnosisDefNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisDefinitive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Diagnósticos Presuntivos (TODOS)
                    var diagnosisPreNames = consultation.DiagnosisConsultations?
                        .Where(d => d.DiagnosisPresumptive == true)
                        .Select(d => diagnosisList.FirstOrDefault(x => x.DiagnosisId == d.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A")
                        .ToList() ?? new List<string>();

                    // Formatear diagnósticos
                    string diagnosticosTexto = "";

                    if (diagnosisDefNames.Any() || diagnosisPreNames.Any())
                    {
                        var partes = new List<string>();

                        if (diagnosisDefNames.Any())
                            partes.Add($"Definitivo: {string.Join(", ", diagnosisDefNames)}");

                        if (diagnosisPreNames.Any())
                            partes.Add($"Presuntivo: {string.Join(", ", diagnosisPreNames)}");

                        diagnosticosTexto = string.Join(" | ", partes);
                    }
                    else
                    {
                        diagnosticosTexto = "N/A";
                    }

                    formFields.SetField("txt_diagnosticos", diagnosticosTexto);

                    // =========================
                    // IMÁGENES
                    // =========================
                    var allImages = await _selectService.GetAllImagesAsync();

                    var imagesInfo = consultation.ImagesConsultations?.Any() == true
                        ? string.Join("\n", consultation.ImagesConsultations.Select(ic =>
                        {
                            var image = allImages.FirstOrDefault(i => i.ImagesId == ic.ImagesImagesid);
                            return image != null
                                ? $"({image.ImagesCie10}) {image.ImagesName} - Cantidad: {ic.ImagesAmount}"
                                : "N/A";
                        }))
                        : "No se solicitaron imágenes";

                    formFields.SetField("txt_imagenes", imagesInfo);

                    // =========================
                    // OBSERVACIONES - TODAS LAS OBSERVACIONES
                    // =========================
                    var observaciones = consultation.ImagesConsultations?
                        .Where(ic => !string.IsNullOrWhiteSpace(ic.ImagesObservation))
                        .Select(ic => ic.ImagesObservation)
                        .ToList() ?? new List<string>();

                    string observacionesTexto = observaciones.Any()
                        ? string.Join("\n", observaciones)
                        : "Sin observaciones";

                    formFields.SetField("txt_observaciones", observacionesTexto);

                    formFields.SetField("txt_direccion", consultation.UsersEstablishmentAddress ?? "N/A");

                    // Finalizar PDF
                    pdfStamper.FormFlattening = true;
                    pdfStamper.Close();
                }

                pdfBytes = memoryStream.ToArray();
            }

            var randomNumber = new Random().Next(1000, 9999);
            return File(pdfBytes, "application/pdf", $"pedido_imagenes_{randomNumber}.pdf");
        }

        /// <summary>
        /// Construye la información completa del médico en formato texto
        /// </summary>
        private string ConstruirDatosMedico(dynamic consultation)
        {
            var datosMedicoBuilder = new StringBuilder();

            // Nombre completo
            string nombreCompletoMedico = $"Dr(a). {consultation.UsersNames ?? ""} {consultation.UsersSurcenames ?? ""}".Trim();
            if (!string.IsNullOrWhiteSpace(nombreCompletoMedico) && nombreCompletoMedico != "Dr(a).")
                datosMedicoBuilder.Append(nombreCompletoMedico + "\n");

            // Especialidad
            if (!string.IsNullOrWhiteSpace(consultation.SpecialityName))
                datosMedicoBuilder.Append(consultation.SpecialityName + "\n");

            // Cédula Profesional
            if (!string.IsNullOrWhiteSpace(consultation.UsersDocumentNumber))
                datosMedicoBuilder.Append($"{consultation.UsersDocumentNumber}\n");

            // Email
            if (!string.IsNullOrWhiteSpace(consultation.UsersEmail))
                datosMedicoBuilder.Append($"{consultation.UsersEmail} / ");

            // Teléfono
            if (!string.IsNullOrWhiteSpace(consultation.UsersPhone))
                datosMedicoBuilder.Append($"{consultation.UsersPhone}");



            return datosMedicoBuilder.Length > 0
                ? datosMedicoBuilder.ToString().TrimEnd()
                : "Información del médico no disponible";
        }
    }
}
