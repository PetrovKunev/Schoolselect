using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class ComparisonSetRepository : Repository<ComparisonSet>, IComparisonSetRepository
    {
        public ComparisonSetRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        public async Task<IEnumerable<ComparisonSet>> GetComparisonSetsByUserIdAsync(Guid userId)
        {
            return await AppContext.ComparisonSets
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<ComparisonSet?> GetComparisonSetWithItemsAsync(int comparisonSetId)
        {
            
            var result = await AppContext.ComparisonSets
                .Include(c => c.Items)
                    .ThenInclude(i => i.School)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Profile)
                .SingleOrDefaultAsync(c => c.Id == comparisonSetId);

            return result;
        }
    }
}