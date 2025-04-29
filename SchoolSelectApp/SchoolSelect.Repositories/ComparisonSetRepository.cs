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
                .Include(c => c.Items)
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

        public async Task<ComparisonItem?> GetComparisonItemByIdAsync(int itemId)
        {
            return await AppContext.ComparisonItems.FindAsync(itemId);
        }

        public async Task<ComparisonSet?> GetComparisonSetByItemIdAsync(int itemId)
        {
            return await AppContext.ComparisonSets
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Items.Any(i => i.Id == itemId));
        }

        public async Task<bool> RemoveComparisonItemAsync(int itemId)
        {
            var item = await AppContext.ComparisonItems.FindAsync(itemId);
            if (item == null)
                return false;

            AppContext.ComparisonItems.Remove(item);
            await AppContext.SaveChangesAsync();
            return true;
        }
    }
}