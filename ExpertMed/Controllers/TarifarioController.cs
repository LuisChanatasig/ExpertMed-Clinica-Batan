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
        private readonly ProcedimientoService _procedimientoService;

        public TarifarioController(TarifarioService tariffService, DbExpertmedContext dbExpertmed, ProcedimientoService procedimientoService)
        {
            _tariffService = tariffService;
            _dbContext = dbExpertmed;
            _procedimientoService = procedimientoService;
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
                var procedimientos = await _procedimientoService.BuscarProcedimientosAdoAsync(termino, insuranceCompanyId);

                // Transformar a formato compatible con JavaScript (manteniendo la estructura original)
                var result = procedimientos.Select(p => new
                {
                    id = p.Id,
                    text = p.Text,
                    precio = p.Precio,
                    precio_aseguradora = p.PrecioAseguradora
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


    }
}
