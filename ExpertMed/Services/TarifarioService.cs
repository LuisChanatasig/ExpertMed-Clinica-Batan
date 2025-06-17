using ExpertMed.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExpertMed.Services
{
    public class TarifarioService
    {
        private readonly DbExpertmedContext _context;

        public async Task<InsuranceTariffDTO?> GetTariffByDescriptionAsync(string descripcion, int insuranceCompanyId)
        {
            var parameters = new[]
            {
        new SqlParameter("@descripcion", descripcion),
        new SqlParameter("@insurance_company_id", insuranceCompanyId)
    };

            var result = await _context
     .Set<InsuranceTariffDTO>()
     .FromSqlRaw("EXEC sp_GetInsuranceTariffByDescription @descripcion, @insurance_company_id", parameters)
     .ToListAsync();

            return result.FirstOrDefault();


        }

    }
}
