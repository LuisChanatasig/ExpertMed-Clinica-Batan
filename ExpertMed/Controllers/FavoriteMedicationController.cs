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

        [HttpPost("add")]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteMedicationDto dto)
        {
            var (success, message) = await _service.AddFavoriteMedicationAsync(
                dto.DoctorUserId,
                dto.MedicationId,
                dto.Amount,
                dto.Observation
            );

            if (success)
                return Ok(new { success = true, message });
            else
                return BadRequest(new { success = false, message });
        }
    }
}
