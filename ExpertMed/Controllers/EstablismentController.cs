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


     

        [HttpGet]
        public async Task<ActionResult<List<EstablishmentDto>>> ListEstablishments()
        {
            try
            {
                var result = await _service.ListEstablishmentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

    }
}
