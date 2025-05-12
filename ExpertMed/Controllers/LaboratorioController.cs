using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExpertMed.Controllers
{
    public class LaboratorioController : Controller
    {

        private readonly LaboratoryService _laboratorioService;
        private readonly SelectsService _selectsService;

        private readonly ILogger<LaboratorioController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly DbExpertmedContext _dbContext;

        public LaboratorioController(LaboratoryService laboratorioService, SelectsService selectsService, IWebHostEnvironment webHostEnvironment, ILogger<LaboratorioController> logger, IHttpContextAccessor httpContextAccessor, DbExpertmedContext dbContext)
        {
            _laboratorioService = laboratorioService;
            _selectsService = selectsService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Método para cargar la vista de solicitudes de laboratorio.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LaboratoryList()
        {
            try
            {
                var perfil = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                var solicitudes = await _laboratorioService.GetPendingLabRequestsAsync(perfil, usuarioId);

                return View(solicitudes);
            }
            catch (Exception ex)
            {
                // Log o manejo de errores
                TempData["ErrorMessage"] = "Hubo un error al cargar las solicitudes." + ex.Message;
                return View(new List<PendingLabRequest>());
            }
        }

        /// <summary>
        /// Metodo para cargar el historial de laboratorio.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> LaboratoryHistory()
        {
            try
            {
                var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                var historial = await _laboratorioService.GetLabHistoryAsync(perfilId, usuarioId);

                return View(historial);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el historial de laboratorio");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el historial de laboratorio." + ex.Message;
                return View(new List<PendingLabRequest>());
            }
        }


        /// <summary>
        /// Método para cambiar el estado de una solicitud de laboratorio.
        /// </summary>
        /// <param name="laboratorioId"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int laboratorioId, int nuevoEstado)
        {
            try
            {
                await _laboratorioService.ChangeStatusLaboratory(laboratorioId, nuevoEstado);
                TempData["SuccessMessage"] = "Estado actualizado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del laboratorio");
                TempData["ErrorMessage"] = "No se pudo actualizar el estado." + ex.Message;
            }

            return RedirectToAction("LaboratoryList");
        }


        /// <summary>
        /// Método para mostrar la vista de registrar el resultado de una solicitud de laboratorio. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> RecordResult()
        {
            try
            {
                var perfil = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                // 1. Obtener todas las solicitudes de laboratorio
                var todasSolicitudes = await _laboratorioService.GetAllLabRequestsAsync(perfil, usuarioId);

                // 2. Filtrar solo las que están con estado 3 (Pendiente de resultado)
                var solicitudesFiltradas = todasSolicitudes
                    .Where(x => x.LaboratoriesStatus == 3)
                    .ToList();

                // 3. Obtener médicos
                var medics = await _selectsService.GetAllMedicsAsync(perfil, usuarioId);
                ViewBag.Medicos = medics;

                // 4. Retornar a la vista
                return View(solicitudesFiltradas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la vista de registrar resultados.");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los resultados pendientes." + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Método para cargar los resultados de laboratorio.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarResultadoYArchivo(RegistrarResultadosViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var lista = new List<ResultadoLaboratorioInput>();

            foreach (var item in model.Resultados)
            {
                IFormFile? archivo = item.Archivo;

                lista.Add(new ResultadoLaboratorioInput
                {
                    LaboratoryId = item.ExamenId,
                    Resultado = item.Resultado,
                    Archivo = archivo  // Este se convertirá a byte[] en el servicio
                });
            }

            await _laboratorioService.InsertarResultadosLaboratorioAsync(lista, usuarioId);

            TempData["SuccessMessage"] = "Resultados registrados correctamente.";
            return RedirectToAction("RecordResult");
        }



        /// <summary>
        /// Metodo que muestra la vista para descargar los resultados
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadResults()
        {
            try
            {
                var perfil = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                // 1. Obtener todas las solicitudes de laboratorio
                var todasSolicitudes = await _laboratorioService.GetAllLabRequestsAsync(perfil, usuarioId);

                // 2. Filtrar solo las que están con estado 4 (Finalizado)
                var solicitudesFiltradas = todasSolicitudes
                    .Where(x => x.LaboratoriesStatus == 4)
                    .ToList();

                // 3. Obtener médicos solo si el perfil lo requiere
                if (perfil == 1 || perfil == 4 || perfil == 6)
                {
                    var medics = await _selectsService.GetAllMedicsAsync(perfil, usuarioId);
                    ViewBag.Medicos = medics;
                }
                else
                {
                    ViewBag.Medicos = null;
                }

                // 4. Retornar la vista
                return View(solicitudesFiltradas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la vista de registrar resultados.");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar los resultados pendientes. " + ex.Message;
                return RedirectToAction("DownloadResults", "Laboratorio");
            }
        }


        /// <summary>
        /// Metodo para descargar un archivo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DescargarArchivoResultado(int id)
        {
            try
            {
                // Llama al servicio que obtiene el archivo (varbinary) y el nombre
                var (fileData, fileName) = await _laboratorioService.GetResultadoArchivoPorLaboratoryIdAsync(id);

                if (fileData == null || fileData.Length == 0)
                {
                    TempData["ErrorMessage"] = "El archivo no está disponible o está vacío.";
                    return RedirectToAction("DownloadResults");
                }

                // Devuelve el archivo como descarga
                return File(fileData, "application/pdf", fileName); // Ajusta el MIME si guardas imágenes u otro formato
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al intentar descargar el archivo con ID {id}");
                TempData["ErrorMessage"] = "Ocurrió un error al intentar descargar el archivo." +ex.Message;
                return RedirectToAction("DownloadResults");
            }
        }

        /// <summary>
        /// Reporte
        /// </summary>
        /// <param name="fechaDesde"></param>
        /// <param name="fechaHasta"></param>
        /// <param name="medicoId"></param>
        /// <param name="seguroId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Laboratoryreport(DateTime? fechaDesde, DateTime? fechaHasta, int? medicoId, int? seguroId)
        {
            try
            {
                // Obtener perfil y usuario desde sesión
                var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                // Rango de fechas por defecto: último mes
                fechaHasta ??= DateTime.Today;
                fechaDesde ??= DateTime.Today.AddMonths(-1);

                // Obtener datos desde el servicio
                var resultados = await _laboratorioService.GetReporteResultadosAsync(
                    fechaDesde, fechaHasta, medicoId, seguroId, perfilId, usuarioId);

                var chartData = await _laboratorioService.GetChartDataAsync(
                    fechaDesde, fechaHasta, medicoId, seguroId, perfilId, usuarioId);

                // Obtener selects
                var medicos = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);
                var seguros = await _selectsService.GetSureHealtTypeAsync();

                // ViewBag para filtros y datos del gráfico
                ViewBag.Medicos = medicos;
                ViewBag.Seguros = seguros;
                ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
                ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
                ViewBag.MedicoId = medicoId;
                ViewBag.SeguroId = seguroId;
                ViewBag.ChartDataJson = JsonConvert.SerializeObject(chartData);

                return View(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el reporte de laboratorio.");
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el reporte." +ex.Message;
                return View(new List<ReporteLaboratorioDto>());
            }
        }



    }
}
