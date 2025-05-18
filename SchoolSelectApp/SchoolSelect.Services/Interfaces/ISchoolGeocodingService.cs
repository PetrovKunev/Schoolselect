// SchoolSelect.Services/Interfaces/ISchoolGeocodingService.cs
using System.Threading.Tasks;

namespace SchoolSelect.Services.Interfaces
{
    public interface ISchoolGeocodingService
    {
        /// <summary>
        /// Обновява географските координати на всички училища без координати
        /// </summary>
        /// <returns>Брой на успешно обновените училища</returns>
        Task<int> UpdateAllSchoolCoordinatesAsync();

        /// <summary>
        /// Обновява географските координати на конкретно училище
        /// </summary>
        /// <param name="schoolId">ID на училището</param>
        /// <returns>true ако координатите са обновени успешно, false в противен случай</returns>
        Task<bool> UpdateSchoolCoordinatesAsync(int schoolId);
    }
}