using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Rotativa.AspNetCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;


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


        [HttpGet]
        public async Task<IActionResult> MedicalCertificate(int consultationId)
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
            var viewModel = new NewPatientViewModel
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

            return new ViewAsPdf("MedicalCertificate", viewModel)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
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

            using var memoryStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(templatePath);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, memoryStream);

            AcroFields formFields = pdfStamper.AcroFields;

            // Asignar valores a campos de la plantilla PDF
            formFields.SetField("txt_nombre_doctor", consultation.UsersNames + " " + consultation.UsersSurcenames);
            formFields.SetField("txt_especialidad", consultation.SpecialityName);
            formFields.SetField("txt_email", consultation.UsersEmail);
            formFields.SetField("txt_telefono", consultation.UsersPhone);

            formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                : "N/A");

            formFields.SetField("txt_apellido", patient.PatientFirstsurname + " " + patient.PatientSecondlastname);
            formFields.SetField("txt_nombres", patient.PatientFirstname + " " + patient.PatientMiddlename);
            formFields.SetField("txt_edad", patient.PatientAge.ToString());
            formFields.SetField("txt_cedula", patient.PatientDocumentnumber);

            var diagnosisList = await _selectService.GetAllDiagnosisAsync();

            // Diagnóstico Definitivo
            var definitiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisDefinitive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisDefName = definitiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == definitiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            // Diagnóstico Presuntivo
            var presumptiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisPresumptive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisPresumptiveName = presumptiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == presumptiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            formFields.SetField("txt_diagnosticos", $"Definitivo: {diagnosisDefName} | Presuntivo: {diagnosisPresumptiveName}");

            // Medicamentos
            var allMedications = await _selectService.GetAllMedicationsAsync();
            var medicationsInfo = string.Join("\n", consultation.MedicationsConsultations.Select(mc =>
            {
                var medication = allMedications.FirstOrDefault(m => m.MedicationsId == mc.MedicationsMedicationsid);
                return medication != null ? $"({medication.MedicationsCie10}) {medication.MedicationsName} - X: {mc.MedicationsAmount}" : "N/A";
            }));

            formFields.SetField("txt_prescripcion", medicationsInfo);

            // Indicaciones
            var indications = string.Join("\n", consultation.MedicationsConsultations.Select(mc =>
            {
                var medication = allMedications.FirstOrDefault(m => m.MedicationsId == mc.MedicationsMedicationsid);
                return medication != null ? $"({medication.MedicationsCie10}) {medication.MedicationsName} - Obs: {mc.MedicationsObservation}" : "N/A";
            }));

            formFields.SetField("txt_observacion", indications);

            var allergiesList = await _selectService.GetAllergiesTypeAsync();

            // Obtenemos los IDs de alergias asociados a la consulta
            var consultationAllergyIds = consultation.AllergiesConsultations?
                .Select(ac => ac.AllergiesCatalogid)
                .ToList();

            // Filtramos el catálogo para quedarnos solo con los que tengan un ID presente en la consulta
            var filteredAllergies = (consultationAllergyIds != null && consultationAllergyIds.Any())
                ? allergiesList.Where(c => consultationAllergyIds.Contains(c.CatalogId))
                : Enumerable.Empty<Catalog>();

            var allergiesText = filteredAllergies.Any()
                ? string.Join(", ", filteredAllergies.Select(a => a.CatalogName))
                : "N/A";

            formFields.SetField("txt_alergias", allergiesText);



            formFields.SetField("txt_rec_no_farma", consultation.ConsultationNonpharmacologycal ?? "N/A");

            formFields.SetField("txt_direccion", consultation.UsersEstablishmentAddress);

            // Finalizar la edición y cerrar el stamper
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();

            // Genera un número aleatorio único
            var randomNumber = new Random().Next(1000, 9999);

            // Devuelve el archivo PDF generado
            return File(memoryStream.ToArray(), "application/pdf", $"receta_medicacion_{randomNumber}.pdf");
        }



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

            using var memoryStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(templatePath);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, memoryStream);

            AcroFields formFields = pdfStamper.AcroFields;

            // Asignar valores a campos de la plantilla PDF
            formFields.SetField("txt_nombre_doctor", consultation.UsersNames + " " + consultation.UsersSurcenames);
            formFields.SetField("txt_especialidad", consultation.SpecialityName);
            formFields.SetField("txt_email", consultation.UsersEmail);
            formFields.SetField("txt_telefono", consultation.UsersPhone);

            formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                : "N/A");

            formFields.SetField("txt_apellido", patient.PatientFirstsurname + " " + patient.PatientSecondlastname);
            formFields.SetField("txt_nombres", patient.PatientFirstname + " " + patient.PatientMiddlename);
            formFields.SetField("txt_edad", patient.PatientAge.ToString());
            formFields.SetField("txt_cedula", patient.PatientDocumentnumber);

            var diagnosisList = await _selectService.GetAllDiagnosisAsync();

            // Diagnóstico Definitivo
            var definitiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisDefinitive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisDefName = definitiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == definitiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            // Diagnóstico Presuntivo
            var presumptiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisPresumptive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisPresumptiveName = presumptiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == presumptiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            formFields.SetField("txt_diagnostico", $"Definitivo: {diagnosisDefName} | Presuntivo: {diagnosisPresumptiveName}");

            // Laboratorios
            var allLabs = await _selectService.GetAllLaboratoriesAsync();
            var laboratoriesInfo = string.Join("\n", consultation.LaboratoriesConsultations.Select(lc =>
            {
                var lab = allLabs.FirstOrDefault(l => l.LaboratoriesId == lc.LaboratoriesLaboratoriesid);
                return lab != null ? $"({lab.LaboratoriesCie10}) {lab.LaboratoriesName} - X: {lc.LaboratoriesAmount}" : "N/A";
            }));

            formFields.SetField("txt_laboratorios", laboratoriesInfo);

            // Observaciones
            var observations = string.Join("\n", consultation.LaboratoriesConsultations.Select(lc =>
            {
                var lab = allLabs.FirstOrDefault(l => l.LaboratoriesId == lc.LaboratoriesLaboratoriesid);
                return lab != null ? $"({lab.LaboratoriesCie10}) {lab.LaboratoriesName} - Obs: {lc.LaboratoriesObservation}" : "N/A";
            }));

            formFields.SetField("txt_observaciones", observations);
            formFields.SetField("txt_direccion", consultation.UsersEstablishmentAddress);

            // Finalizar la edición y cerrar el stamper
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();

            // Genera un número aleatorio único
            var randomNumber = new Random().Next(1000, 9999);

            // Devuelve el archivo PDF generado
            return File(memoryStream.ToArray(), "application/pdf", $"pedido_laboratorio_{randomNumber}.pdf");
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

            using var memoryStream = new MemoryStream();
            PdfReader pdfReader = new PdfReader(templatePath);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, memoryStream);

            AcroFields formFields = pdfStamper.AcroFields;

            // Asignar valores a campos de la plantilla PDF
            formFields.SetField("txt_nombre_doctor", consultation.UsersNames + " " + consultation.UsersSurcenames);
            formFields.SetField("txt_especialidad", consultation.SpecialityName);
            formFields.SetField("txt_email", consultation.UsersEmail);
            formFields.SetField("txt_telefono", consultation.UsersPhone);

            formFields.SetField("txt_fecha", consultation.ConsultationCreationdate.HasValue
                ? consultation.ConsultationCreationdate.Value.ToShortDateString()
                : "N/A");

            formFields.SetField("txt_apellido", patient.PatientFirstsurname + " " + patient.PatientSecondlastname);
            formFields.SetField("txt_nombres", patient.PatientFirstname + " " + patient.PatientMiddlename);
            formFields.SetField("txt_edad", patient.PatientAge.ToString());
            formFields.SetField("txt_cedula", patient.PatientDocumentnumber);

            var diagnosisList = await _selectService.GetAllDiagnosisAsync();

            // Diagnóstico Definitivo
            var definitiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisDefinitive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisDefName = definitiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == definitiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            // Diagnóstico Presuntivo
            var presumptiveDiagnosis = consultation.DiagnosisConsultations?
                .Where(d => d.DiagnosisPresumptive == true)
                .OrderByDescending(d => d.DiagnosisDiagnosisid)
                .FirstOrDefault();

            var diagnosisPresumptiveName = presumptiveDiagnosis != null
                ? diagnosisList.FirstOrDefault(d => d.DiagnosisId == presumptiveDiagnosis.DiagnosisDiagnosisid)?.DiagnosisName ?? "N/A"
                : "N/A";

            formFields.SetField("txt_diagnostico", $"Definitivo: {diagnosisDefName} | Presuntivo: {diagnosisPresumptiveName}");

            // Imágenes
            var allImages = await _selectService.GetAllImagesAsync();
            var imagesInfo = string.Join("\n", consultation.ImagesConsultations.Select(ic =>
            {
                var image = allImages.FirstOrDefault(i => i.ImagesId == ic.ImagesImagesid);
                return image != null ? $"({image.ImagesCie10}) {image.ImagesName} - X: {ic.ImagesAmount}" : "N/A";
            }));

            formFields.SetField("txt_imagenes", imagesInfo);

            // Observaciones
            var observations = string.Join("\n", consultation.ImagesConsultations.Select(ic =>
            {
                var image = allImages.FirstOrDefault(i => i.ImagesId == ic.ImagesImagesid);
                return image != null ? $"({image.ImagesCie10}) {image.ImagesName} - Obs: {ic.ImagesObservation}" : "N/A";
            }));

            formFields.SetField("txt_observaciones", observations);
            formFields.SetField("txt_direccion", consultation.UsersEstablishmentAddress);

            // Finalizar la edición y cerrar el stamper
            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();

            // Genera un número aleatorio único
            var randomNumber = new Random().Next(1000, 9999);

            // Devuelve el archivo PDF generado
            return File(memoryStream.ToArray(), "application/pdf", $"pedido_imagenes_{randomNumber}.pdf");
        }


    }
}
