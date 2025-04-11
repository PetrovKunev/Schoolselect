using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class UserGradesRepository : Repository<UserGrades>, IUserGradesRepository
    {
        public UserGradesRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща всички набори от оценки на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с набори от оценки</returns>
        public async Task<IEnumerable<UserGrades>> GetGradesByUserIdAsync(Guid userId)
        {
            return await AppContext.UserGrades
                .Where(g => g.UserId == userId)
                .Include(g => g.AdditionalGrades)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Връща набор от оценки заедно с допълнителните оценки
        /// </summary>
        /// <param name="gradesId">ID на набора от оценки</param>
        /// <returns>Набор от оценки с допълнителните оценки или null ако не съществува</returns>
        public async Task<UserGrades?> GetGradesWithAdditionalGradesAsync(int gradesId)
        {
            return await AppContext.UserGrades
                .Include(g => g.AdditionalGrades)
                .SingleOrDefaultAsync(g => g.Id == gradesId);
        }
    }
}