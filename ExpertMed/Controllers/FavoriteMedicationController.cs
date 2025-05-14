using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class FavoriteMedicationController : Controller
    {
        private readonly FavoriteMedicationService _service;

        public FavoriteMedicationController(FavoriteMedicationService service)
        {
            _service = service;
        }


        // POST: api/FavoriteMedication/AddOrUpdate
        [HttpPost("AddOrUpdate")]
        public async Task<IActionResult> AddOrUpdate([FromBody] FavoriteMedicationInputModel model)
        {
            if (model == null) return BadRequest("Datos inválidos.");

            await _service.AddOrUpdateFavoriteAsync(
                model.UsersId,
                model.MedicationsId,
                model.Cantidad,
                model.Observacion
            );

            return Ok(new { message = "Guardado correctamente." });
        }

        // DELETE: api/FavoriteMedication/Delete?userId=1&medicationId=2
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int userId, int medicationId)
        {
            await _service.RemoveFavoriteAsync(userId, medicationId);
            return Ok(new { message = "Eliminado correctamente." });
        }

        // GET: api/FavoriteMedication/List?userId=1
        [HttpGet("List")]
        public async Task<ActionResult<List<FavoriteMedicationViewModel>>> List(int userId)
        {
            var favoritos = await _service.GetFavoritesAsync(userId);
            return Ok(favoritos);
        }
    }
}
