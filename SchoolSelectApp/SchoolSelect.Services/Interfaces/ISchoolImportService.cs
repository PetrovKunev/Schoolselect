using Microsoft.AspNetCore.Http;
using SchoolSelect.Services.Models;

namespace SchoolSelect.Services.Interfaces
{
    /// <summary>
    /// Интерфейс за услуга за импортиране на училища
    /// </summary>
    public interface ISchoolImportService
    {
        /// <summary>
        /// Импортира училища от CSV файл
        /// </summary>
        /// <param name="file">CSV файл</param>
        /// <returns>Резултат от импортирането</returns>
        Task<ImportResult> ImportSchoolsFromCsvAsync(IFormFile file);

        /// <summary>
        /// Импортира училища от Excel файл
        /// </summary>
        /// <param name="file">Excel файл</param>
        /// <returns>Резултат от импортирането</returns>
        Task<ImportResult> ImportSchoolsFromExcelAsync(IFormFile file);

        /// <summary>
        /// Импортира профили от CSV файл
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<ImportResult> ImportProfilesFromCsvAsync(IFormFile file);
    }
}

