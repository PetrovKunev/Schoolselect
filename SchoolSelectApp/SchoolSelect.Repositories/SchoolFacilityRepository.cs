using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    public class SchoolFacilityRepository : Repository<SchoolFacility>, ISchoolFacilityRepository
    {
        public SchoolFacilityRepository(ApplicationDbContext context) : base(context) { }

        private ApplicationDbContext AppContext => _context as ApplicationDbContext;

        /// <summary>
        /// Връща всички съоръжения на дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък със съоръжения</returns>
        public async Task<IEnumerable<SchoolFacility>> GetFacilitiesBySchoolIdAsync(int schoolId)
        {
            return await AppContext.SchoolFacilities
                .Where(f => f.SchoolId == schoolId)
                .ToListAsync();
        }

        /// <summary>
        /// Връща всички съоръжения от определен тип
        /// </summary>
        /// <param name="facilityType">Тип на съоръжението</param>
        /// <returns>Списък със съоръжения от посочения тип</returns>
        public async Task<IEnumerable<SchoolFacility>> GetFacilitiesByTypeAsync(string facilityType)
        {
            return await AppContext.SchoolFacilities
                .Where(f => f.FacilityType == facilityType)
                .ToListAsync();
        }
    }
}