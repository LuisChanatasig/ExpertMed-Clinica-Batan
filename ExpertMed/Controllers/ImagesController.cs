using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class ImagesController : Controller
    {
        private readonly ImagesService _imagenesService;
        private readonly SelectsService _selectsService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(ImagesService imagesService, SelectsService selectsService, ILogger<ImagesController> logger)
        {
            _imagenesService = imagesService;
            _selectsService = selectsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ImagesList()
        {
            try
            {
                // Obtener perfil y usuario desde sesión (ajústalo según cómo almacenes estos valores)
                var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                var lista = await _imagenesService.GetImagenesPendientesAsync(perfilId, usuarioId);

                return View(lista);
            }
            catch (Exception ex)
            {
                // Puedes loguear el error o redirigir a una vista de error
                TempData["ErrorMessage"] = "Ocurrió un error al cargar las imágenes pendientes.";
                return View(new List<ImagenPendienteDto>()); // Retorna una lista vacía en caso de fallo
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImagesHistory(int patientId)
        {
            try
            {
                var perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

                var historial = await _imagenesService.GetHistorialImagenesAsync(perfilId, usuarioId);
                return View(historial);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el historial de imágenes.";
                return View(new List<ImagenPendienteDto>());
            }
        }

        /// <summary>
        /// Metodo para cambiar el estado.
        /// </summary>
        /// <param name="imagenId"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int imagenId, int nuevoEstado)
        {
            try
            {
                var resultado = await _imagenesService.ChangeEstadoImagenAsync(imagenId, nuevoEstado);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Estado actualizado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo actualizar el estado.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cambiar el estado: {ex.Message}";
            }

            return RedirectToAction("ImagesHistory");
        }

        /// <summary>
        /// Metodo para 
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
                var todasSolicitudes = await _imagenesService.GetAllImageRequestsAsync(perfil, usuarioId);

                // 2. Filtrar solo las que están con estado 3 (Pendiente de resultado)
                var solicitudesFiltradas = todasSolicitudes
                    .Where(x => x.ImagesStatus == 3)
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
        /// Metodo para guardar los archivos y resultados
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GuardarResultadoYArchivo(RegistrarResultadosViewModel model)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            var lista = new List<ResultadoImagenInput>();

            foreach (var item in model.Resultados)
            {
                lista.Add(new ResultadoImagenInput
                {
                    ImageId = item.ExamenId,
                    Resultado = item.Resultado,
                    Archivo = item.Archivo  // Se convierte a byte[] en el servicio
                });
            }

            await _imagenesService.InsertarResultadosImagenesAsync(lista, usuarioId);

            TempData["SuccessMessage"] = "Resultados de imágenes registrados correctamente.";
            return RedirectToAction("RecordResult");
        }

        /// <summary>
        /// Metodo que trae la vista para descargar 
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
                var todasSolicitudes = await _imagenesService.GetAllImageRequestsAsync(perfil, usuarioId);

                // 2. Filtrar solo las que están con estado 4 (Finalizado)
                var solicitudesFiltradas = todasSolicitudes
                    .Where(x => x.ImagesStatus == 4)
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
        /// Metodo que descarga el archivo
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DescargarArchivoResultado(int id)
        {
            try
            {
                // Llama al servicio que obtiene el archivo de imagen (varbinary) y el nombre
                var (fileData, fileName) = await _imagenesService.GetResultadoArchivoPorImageIdAsync(id);

                if (fileData == null || fileData.Length == 0)
                {
                    TempData["ErrorMessage"] = "El archivo no está disponible o está vacío.";
                    return RedirectToAction("DownloadImageResults");
                }

                // Devuelve el archivo como descarga
                return File(fileData, "application/pdf", fileName); // Ajusta el MIME si se espera imagen, ej. "image/jpeg"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al intentar descargar el archivo de imagen con ID {id}");
                TempData["ErrorMessage"] = "Ocurrió un error al intentar descargar el archivo. " + ex.Message;
                return RedirectToAction("DownloadImageResults");
            }
        }

    }
}
