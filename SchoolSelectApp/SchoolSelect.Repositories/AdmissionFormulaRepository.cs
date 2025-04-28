using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class AdmissionFormulaRepository : Repository<AdmissionFormula>, IAdmissionFormulaRepository
    {
        public AdmissionFormulaRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Взима формула с всички нейни компоненти
        /// </summary>
        /// <param name="formulaId">ID на формулата</param>
        /// <returns>Формулата с компоненти или null ако не съществува</returns>
        public async Task<AdmissionFormula?> GetFormulaWithComponentsAsync(int formulaId)
        {
            return await AppContext.AdmissionFormulas
                .Include(f => f.Components)
                .SingleOrDefaultAsync(f => f.Id == formulaId);
        }

        /// <summary>
        /// Взима текущата формула за даден профил
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Текущата формула или null ако не съществува</returns>
        public async Task<AdmissionFormula?> GetCurrentFormulaForProfileAsync(int profileId)
        {
            return await AppContext.AdmissionFormulas
                .Include(f => f.Components)
                .Where(f => f.SchoolProfileId == profileId)
                .OrderByDescending(f => f.Year) // Първо сортираме по година в низходящ ред
                .FirstOrDefaultAsync(); // Взимаме най-новата формула
        }

        /// <summary>
        /// Взима всички формули за даден профил
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Списък с формули</returns>
        public async Task<IEnumerable<AdmissionFormula>> GetFormulasByProfileIdAsync(int profileId)
        {
            return await AppContext.AdmissionFormulas
                .Where(f => f.SchoolProfileId == profileId)
                .OrderByDescending(f => f.Year)
                .ToListAsync();
        }
    }
}