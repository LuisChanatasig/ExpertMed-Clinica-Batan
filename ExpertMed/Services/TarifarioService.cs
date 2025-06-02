using ExpertMed.Models;

namespace ExpertMed.Services
{
    public class TarifarioService
    {
        private readonly DbExpertmedContext _context;

        //public TarifarioDto ObtenerPorDescripcionYSeguro(string descripcion, int insuranceCompanyId)
        //{
        //    // Ajusta a tu tabla y columnas reales
        //    var item = _context.Tarifarios
        //        .FirstOrDefault(t =>
        //            t.Descripcion.ToLower() == descripcion.ToLower() &&
        //            t.InsuranceCompanyId == insuranceCompanyId);

        //    if (item == null)
        //        return null;

        //    return new TarifarioDto
        //    {
        //        PrecioUnitario = item.Precio, // El valor total
        //        ValorAseguradora = item.ValorAseguradora // Lo que cubre el seguro
        //    };
        //}
    }
}
