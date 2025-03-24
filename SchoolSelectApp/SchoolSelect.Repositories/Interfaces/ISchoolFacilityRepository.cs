using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface ISchoolFacilityRepository : IRepository<SchoolFacility>
    {
        /// <summary>
        /// Връща всички съоръжения на дадено училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>Списък със съоръжения</returns>
        Task<IEnumerable<SchoolFacility>> GetFacilitiesBySchoolIdAsync(int schoolId);

        /// <summary>
        /// Връща всички съоръжения от определен тип
        /// </summary>
        /// <param name="facilityType">Тип на съоръжението</param>
        /// <returns>Списък със съоръжения от посочения тип</returns>
        Task<IEnumerable<SchoolFacility>> GetFacilitiesByTypeAsync(string facilityType);

        /// <summary>
        /// Връща всички транспортни връзки около училището
        /// </summary>
        /// <param name="schoolId"></param>
        /// <returns></returns>
        Task<IEnumerable<TransportFacility>> GetTransportFacilitiesBySchoolIdAsync(int schoolId);
    }
}