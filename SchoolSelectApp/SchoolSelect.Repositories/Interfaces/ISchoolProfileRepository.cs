using SchoolSelect.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface ISchoolProfileRepository : IRepository<SchoolProfile>
    {
        /// <summary>
        /// Връща всички профили за дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък с профили</returns>
        Task<IEnumerable<SchoolProfile>> GetProfilesBySchoolIdAsync(int schoolId);

        /// <summary>
        /// Връща профил заедно с историческите класирания
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Профил с историческите класирания или null ако не съществува</returns>
        Task<SchoolProfile?> GetProfileWithHistoricalRankingsAsync(int profileId);

        /// <summary>
        /// Връща профил заедно с формулите за балообразуване
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Профил с формулите за балообразуване или null ако не съществува</returns>
        Task<SchoolProfile?> GetProfileWithAdmissionFormulasAsync(int profileId);
    }
}