using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class EstablismentController : Controller
    {
        private readonly EstablishmentService _service;

        public EstablismentController(EstablishmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult CreateEstablishment()
        {
            return View();
        }

        /// <summary>
        /// Crear un nuevo establecimiento
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateEstablishment(CreateEstablishmentDto form)
        {
            if (form == null || string.IsNullOrWhiteSpace(form.Name))
                return BadRequest("Nombre es obligatorio.");

            byte[] logoBytes = null;

            if (form.Logo != null && form.Logo.Length > 0)
            {
                using var ms = new MemoryStream();
                await form.Logo.CopyToAsync(ms);
                logoBytes = ms.ToArray();
            }

            var result = await _service.CreateEstablishmentAsync(
                name: form.Name,
                address: form.Address,
                emissionPoint: form.EmissionPoint,
                pointOfSale: form.PointOfSale,
                creationDate: DateTime.Now, // si quieres ponerlo automático
                modificationDate: null,
                sequentialBilling: form.SequentialBilling,
                logo: logoBytes
            );

            TempData["Message"] = result;

            return RedirectToAction("ListEstablishments"); // o a una vista de éxito
        }



        /// <summary>
        /// Metodo para listar los establecimientos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListEstablishment()
        {
            try
            {
                var result = await _service.ListEstablishmentsAsync();
                return View(result); // <- aquí devolvés la vista y le pasás el modelo
            }
            catch (Exception ex)
            {
                // Podés hacer logging, o redirigir a una vista de error más bonita
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Metodo para traer los datos a la vista del establecimineto.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateEstablishment(int id)
        {
            try
            {
                var result = await _service.GetEstablishmentByIdAsync(id);
                if (result == null)
                    return NotFound($"No se encontró el establecimiento con ID {id}");

                return View(result); // O return Ok(result); si es API
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="Logo"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> UpdateEstablishment(EstablishmentDto model, IFormFile? Logo)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Verifica los datos del formulario.";
                return RedirectToAction("UpdateEstablishment", new { id = model.Id });
            }

            try
            {
                // Si no se sube un nuevo logo, obtenemos el logo actual
                if (Logo == null || Logo.Length == 0)
                {
                    var existing = await _service.GetEstablishmentByIdAsync(model.Id);
                    if (existing?.Logo != null)
                        model.Logo = existing.Logo;
                }
                else
                {
                    using var memoryStream = new MemoryStream();
                    await Logo.CopyToAsync(memoryStream);
                    model.Logo = memoryStream.ToArray();
                }

                var resultMessage = await _service.UpdateEstablishmentAsync(model);
                TempData["SuccessMessage"] = resultMessage;
            }
            catch (ApplicationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al actualizar el establecimiento.";
            }

            return RedirectToAction("ListEstablishment", new { id = model.Id });
        }

    }
}
