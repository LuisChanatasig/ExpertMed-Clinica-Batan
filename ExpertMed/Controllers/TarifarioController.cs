using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpertMed.Controllers
{
    public class TarifarioController : Controller
    {
        private readonly TarifarioService _tariffService;
        private readonly DbExpertmedContext _dbContext;

        public TarifarioController(TarifarioService tariffService, DbExpertmedContext dbExpertmed)
        {
            _tariffService = tariffService;
            _dbContext = dbExpertmed;
        }

        [HttpGet("GetByDescripcion")]
        public async Task<IActionResult> GetByDescripcion(string descripcion, int insuranceCompanyId)
        {
            if (string.IsNullOrWhiteSpace(descripcion) || insuranceCompanyId <= 0)
                return BadRequest();

            var tarifa = await _tariffService.GetTariffByDescriptionAsync(descripcion, insuranceCompanyId);

            if (tarifa == null)
                return Json(new { precio = 0 });

            return Json(new
            {
                codigo = tarifa.Codigo,
                descripcion = tarifa.Descripcion,
                precio_aseguradora = tarifa.PrecioAseguradora,
                precio = tarifa.PrecioAseguradora // puedes modificar si hay precio app diferente
            });
        }

        [HttpGet("BuscarProcedimientos")]
        public async Task<IActionResult> BuscarProcedimientos(string termino, int insuranceCompanyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || insuranceCompanyId <= 0)
                    return Json(new List<object>());

                var result = await _dbContext.InsuranceTariff
                    .Where(t => t.insurance_company_id == insuranceCompanyId &&
                                t.insurance_tariff_description.Contains(termino))
                    .Select(t => new
                    {
                        id = t.insurance_tariff_id,
                        text = t.insurance_tariff_description,
                        precio = t.insurance_tariff_price,
                        precio_aseguradora = t.insurance_tariff_price // 👈 lo devolvemos duplicado para compatibilidad JS
                    })
                    .ToListAsync();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


    }
}
