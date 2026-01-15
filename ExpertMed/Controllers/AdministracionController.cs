using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly ILogger<AdministracionController> _logger;
        private readonly AdminService _adminService;

        private readonly SelectsService _selectService;
        public AdministracionController(ILogger<AdministracionController> logger, AdminService adminService, SelectsService selectService)
        {
            _logger = logger;
            _adminService = adminService;
            _selectService = selectService;
        }
        public IActionResult Agregar_Items()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InsertarDatoGenerico([FromBody] CreateCatalogDto dto)
        {
            // Validación de nulidad y contenido básico
            if (dto == null || string.IsNullOrWhiteSpace(dto.CatalogName))
            {
                return Json(new { success = false, message = "El nombre del catálogo es obligatorio." });
            }

            // Ejecución del servicio que invoca el SP sp_InsertCatalogItem
            var result = await _selectService.CreateCatalogItemAsync(dto);

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }

            // Retorno del error capturado del RAISERROR del SP
            return Json(new { success = false, message = result.message });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedication([FromBody] CreateMedicationDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.medications_name))
            {
                return Json(new
                {
                    success = false,
                    message = "El nombre del medicamento es obligatorio"
                });
            }

            var result = await _selectService.CreateMedicationAsync(dto);

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }

            return Json(new
            {
                success = false,
                message = result.message
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLaboratory([FromBody] CreateLaboratoryDto dto)
        {
            // Si el DTO completo es null, es un problema de formato JSON
            if (dto == null)
            {
                return Json(new { success = false, message = "No se recibieron datos o el formato JSON es incorrecto" });
            }

            // Validación específica del campo
            if (string.IsNullOrWhiteSpace(dto.laboratories_name))
            {
                return Json(new
                {
                    success = false,
                    message = "El nombre del examen es obligatorio (No capturado por el servidor)"
                });
            }

            // 2. Llamada al servicio
            var result = await _selectService.CreateLaboratoryAsync(dto);

            // 3. Respuesta
            return Json(new
            {
                success = result.success,
                message = result.message,
                data = result.data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiagnosis([FromBody] CreateDiagnosisDto dto)
        {
            // Validación de integridad básica
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.diagnosis_name) || string.IsNullOrWhiteSpace(dto.diagnosis_cie10))
            {
                return Json(new
                {
                    success = false,
                    message = "El nombre y el código CIE-10 son obligatorios"
                });
            }

            // Llamada al servicio que ejecuta el SP
            var result = await _selectService.CreateDiagnosisAsync(dto);

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }

            return Json(new
            {
                success = false,
                message = result.message
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateImage([FromBody] CreateImageDto dto)
        {
            // Validación básica de entrada
            if (string.IsNullOrWhiteSpace(dto.images_name))
            {
                return Json(new { success = false, message = "El nombre del estudio es obligatorio." });
            }

            // Llamada al servicio que ejecuta el SP
            var result = await _selectService.CreateImageAsync(dto);

            if (result.success)
            {
                return Json(new
                {
                    success = true,
                    message = result.message,
                    data = result.data
                });
            }

            return Json(new { success = false, message = result.message });
        }
    }
}
