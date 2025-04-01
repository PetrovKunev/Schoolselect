using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}

/// <summary>
/// Резултат от импортирането
/// </summary>
public class ImportResult
{
    public bool IsSuccess { get; set; }
    public int SuccessCount { get; set; } = 0;
    public int FailureCount { get; set; } = 0;
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new List<string>();
}