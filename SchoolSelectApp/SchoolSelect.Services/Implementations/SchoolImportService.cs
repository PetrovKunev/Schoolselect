using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class SchoolImportService : ISchoolImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SchoolImportService> _logger;

        public SchoolImportService(IUnitOfWork unitOfWork, ILogger<SchoolImportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ImportResult> ImportSchoolsFromCsvAsync(IFormFile file)
        {
            var result = new ImportResult
            {
                IsSuccess = false,
                Errors = new List<string>()
            };

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    // Skip header line
                    await reader.ReadLineAsync();

                    string line;
                    int lineCount = 0;
                    int batchSize = 0;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineCount++;
                        try
                        {
                            // Use more robust CSV parsing
                            string[] values = ParseCsvLine(line);

                            if (values.Length < 5)
                            {
                                result.Errors.Add($"Ред {lineCount}: Недостатъчно колони");
                                result.FailureCount++;
                                continue;
                            }

                            // Extract the correct values from CSV based on the format we've seen
                            // Format is Област,Община,Населено място,Код по НЕИСПУО,Наименование,Основен адрес,Основен телефон,Имейл,Интернет страница,Директор
                            var schoolName = values.Length > 4 ? values[4].Trim() : string.Empty;
                            var address = values.Length > 5 ? values[5].Trim() : string.Empty;
                            var phone = values.Length > 6 ? values[6].Trim() : string.Empty;
                            var email = values.Length > 7 ? values[7].Trim() : string.Empty;
                            var website = values.Length > 8 ? values[8].Trim() : string.Empty;
                            var city = values.Length > 2 ? values[2].Trim() : "София";

                            if (string.IsNullOrEmpty(schoolName) || string.IsNullOrEmpty(address))
                            {
                                result.Errors.Add($"Ред {lineCount}: Липсва име на училище или адрес");
                                result.FailureCount++;
                                continue;
                            }

                            var district = ExtractDistrict(address);

                            var school = new School
                            {
                                Name = schoolName,
                                Address = address,
                                District = district,
                                City = !string.IsNullOrEmpty(city) ? city : "София",
                                // Make sure phone doesn't exceed max length
                                Phone = phone?.Length > ValidationConstants.Common.PhoneMaxLength
                                    ? phone.Substring(0, ValidationConstants.Common.PhoneMaxLength)
                                    : phone ?? string.Empty,
                                Email = email?.Length > ValidationConstants.Common.EmailMaxLength
                                    ? email.Substring(0, ValidationConstants.Common.EmailMaxLength)
                                    : email ?? string.Empty,
                                Website = website?.Length > ValidationConstants.Common.UrlMaxLength
                                    ? website.Substring(0, ValidationConstants.Common.UrlMaxLength)
                                    : website ?? string.Empty,
                                Description = string.Empty,
                                GeoLatitude = null,
                                GeoLongitude = null,
                                MapsFormattedAddress = address,
                                AverageRating = 0,
                                RatingsCount = 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                CreatedBy = ValidationConstants.Common.DefaultCreator,
                                UpdatedBy = ValidationConstants.Common.DefaultUpdater
                            };

                            await _unitOfWork.Schools.AddAsync(school);
                            result.SuccessCount++;
                            batchSize++;

                            // Commit in batches of 50 to avoid large transactions
                            if (batchSize >= 50)
                            {
                                await _unitOfWork.CompleteAsync();
                                batchSize = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Грешка при импортиране на ред {lineCount}");
                            result.Errors.Add($"Грешка при импортиране на ред {lineCount}: {ex.Message}");
                            result.FailureCount++;
                        }
                    }

                    // Save any remaining records
                    if (batchSize > 0)
                    {
                        await _unitOfWork.CompleteAsync();
                    }

                    result.IsSuccess = true;

                    _logger.LogInformation($"Импортирани успешно {result.SuccessCount} училища, {result.FailureCount} грешки");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при импортиране на училища от CSV");
                result.ErrorMessage = $"Грешка при импортиране: {ex.Message}";
            }

            return result;
        }

        public async Task<ImportResult> ImportSchoolsFromExcelAsync(IFormFile file)
        {
            var result = new ImportResult
            {
                IsSuccess = false,
                Errors = new List<string>()
            };

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Setting the License for EPPlus 8+
                    // This is the new way to set the license
#pragma warning disable CS0618
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // First worksheet

                        int rowCount = worksheet.Dimension?.Rows ?? 0;
                        int colCount = worksheet.Dimension?.Columns ?? 0;

                        if (rowCount <= 1 || colCount == 0)
                        {
                            throw new Exception("Файлът е празен или има невалиден формат");
                        }

                        // Get headers (assuming first row contains headers)
                        var headers = new Dictionary<string, int>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            string header = worksheet.Cells[1, col].Value?.ToString();
                            if (!string.IsNullOrEmpty(header))
                            {
                                headers[header.Trim()] = col;
                            }
                        }

                        // Check required columns exist
                        if (!headers.ContainsKey("Наименование") || !headers.ContainsKey("Основен адрес"))
                        {
                            throw new Exception("Файлът не съдържа задължителните колони 'Наименование' и 'Основен адрес'");
                        }

                        _logger.LogInformation($"Намерени {rowCount - 1} записа в Excel файла");

                        // Process data rows (skip header)
                        int batchSize = 0;
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                string name = GetCellValue(worksheet, row, headers, "Наименование");
                                string address = GetCellValue(worksheet, row, headers, "Основен адрес");

                                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(address))
                                {
                                    continue; // Skip rows with missing required data
                                }

                                var district = ExtractDistrict(address);
                                var city = GetCellValue(worksheet, row, headers, "Населено място") ?? "София";
                                var phone = GetCellValue(worksheet, row, headers, "Основен телефон") ?? string.Empty;
                                var email = GetCellValue(worksheet, row, headers, "Имейл") ?? string.Empty;
                                var website = GetCellValue(worksheet, row, headers, "Интернет страница") ?? string.Empty;

                                var school = new School
                                {
                                    Name = name,
                                    Address = address,
                                    District = district,
                                    City = city,
                                    // Make sure values don't exceed max length
                                    Phone = phone.Length > ValidationConstants.Common.PhoneMaxLength
                                        ? phone.Substring(0, ValidationConstants.Common.PhoneMaxLength)
                                        : phone,
                                    Email = email.Length > ValidationConstants.Common.EmailMaxLength
                                        ? email.Substring(0, ValidationConstants.Common.EmailMaxLength)
                                        : email,
                                    Website = website.Length > ValidationConstants.Common.UrlMaxLength
                                        ? website.Substring(0, ValidationConstants.Common.UrlMaxLength)
                                        : website,
                                    Description = string.Empty,
                                    GeoLatitude = null,
                                    GeoLongitude = null,
                                    MapsFormattedAddress = address,
                                    AverageRating = 0,
                                    RatingsCount = 0,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow,
                                    CreatedBy = ValidationConstants.Common.DefaultCreator,
                                    UpdatedBy = ValidationConstants.Common.DefaultUpdater
                                };

                                await _unitOfWork.Schools.AddAsync(school);
                                result.SuccessCount++;
                                batchSize++;

                                // Commit in batches of 50 to avoid large transactions
                                if (batchSize >= 50)
                                {
                                    await _unitOfWork.CompleteAsync();
                                    batchSize = 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Грешка при импортиране на ред {row}");
                                result.Errors.Add($"Грешка при импортиране на ред {row}: {ex.Message}");
                                result.FailureCount++;
                            }
                        }

                        // Save any remaining records
                        if (batchSize > 0)
                        {
                            await _unitOfWork.CompleteAsync();
                        }

                        result.IsSuccess = true;

                        _logger.LogInformation($"Импортирани успешно {result.SuccessCount} училища, {result.FailureCount} грешки");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при импортиране на училища от Excel");
                result.ErrorMessage = $"Грешка при импортиране: {ex.Message}";
            }

            return result;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, string columnName)
        {
            if (headers.TryGetValue(columnName, out int col))
            {
                return worksheet.Cells[row, col].Value?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        private string ExtractDistrict(string address)
        {
            if (string.IsNullOrEmpty(address))
                return string.Empty;

            if (address.Contains("район"))
            {
                var match = Regex.Match(address, @"район\s+[""']?([^,""'\)]+)[""']?", RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return string.Empty;
        }

        private string[] ParseCsvLine(string line)
        {
            List<string> result = new List<string>();
            StringBuilder currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') // Escaped quote
                    {
                        currentField.Append('"');
                        i++; // Skip the next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes; // Toggle quote state
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field
            result.Add(currentField.ToString());

            return result.ToArray();
        }
    }
}