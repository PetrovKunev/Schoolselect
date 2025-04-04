using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SchoolSelect.Common;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Services.Models;

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
                    // Прочитаме хедъра
                    string headerLine = await reader.ReadLineAsync() ?? string.Empty;
                    string[] headers = ParseCsvLine(headerLine);

                    // Създаваме речник с индексите на колоните
                    var headerIndices = new Dictionary<string, int>();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        headerIndices[headers[i].Trim()] = i;
                    }

                    string? line;
                    int lineCount = 0;
                    int batchSize = 0;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineCount++;
                        try
                        {
                            // Използваме по-надеждно CSV парсване
                            string[] values = ParseCsvLine(line!);

                            if (values.Length < 5)
                            {
                                result.Errors.Add($"Ред {lineCount}: Недостатъчно колони");
                                result.FailureCount++;
                                continue;
                            }

                            // Извличаме правилните стойности от CSV
                            var schoolName = GetValueByHeader(headerIndices, values, "Наименование");
                            var address = GetValueByHeader(headerIndices, values, "Основен адрес");
                            var phone = GetValueByHeader(headerIndices, values, "Основен телефон");
                            var email = GetValueByHeader(headerIndices, values, "Имейл");
                            var website = GetValueByHeader(headerIndices, values, "Интернет страница");
                            var city = GetValueByHeader(headerIndices, values, "Населено място") ?? "София";

                            if (string.IsNullOrEmpty(schoolName) || string.IsNullOrEmpty(address))
                            {
                                result.Errors.Add($"Ред {lineCount}: Липсва име на училище или адрес");
                                result.FailureCount++;
                                continue;
                            }

                            // Извличане на района - първо търсим директно в колоната "Район"
                            string district = GetValueByHeader(headerIndices, values, "Район");

                            // Ако колоната "Район" липсва или е празна, опитваме се да извлечем района от адреса
                            if (string.IsNullOrWhiteSpace(district))
                            {
                                district = ExtractDistrict(address);
                            }

                            var school = new School
                            {
                                Name = schoolName,
                                Address = address,
                                District = district,
                                City = !string.IsNullOrEmpty(city) ? city : "София",
                                // Уверяваме се, че телефонът не надвишава максималната дължина
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

                            // Записваме на партиди от 50 за избягване на големи транзакции
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

                    // Запазваме останалите записи
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

        private string GetValueByHeader(Dictionary<string, int> headerIndices, string[] values, string headerName)
        {
            if (headerIndices.TryGetValue(headerName, out int index) && index < values.Length)
            {
                return values[index].Trim();
            }
            return string.Empty;
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
                            string header = worksheet.Cells[1, col].Value?.ToString() ?? string.Empty;
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

                                // Извличане на района - първо търсим директно в колоната "Район"
                                string district = GetCellValue(worksheet, row, headers, "Район");

                                // Ако колоната "Район" липсва или е празна, опитваме се да извлечем района от адреса
                                if (string.IsNullOrWhiteSpace(district))
                                {
                                    district = ExtractDistrict(address);
                                }

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
            if (string.IsNullOrEmpty(line))
            {
                return Array.Empty<string>();
            }


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

        public async Task<ImportResult> ImportProfilesFromCsvAsync(IFormFile file)
        {
            var result = new ImportResult();
            var errors = new List<string>();
            int successCount = 0;
            int failureCount = 0;

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Read the CSV file header
                    var header = await reader.ReadLineAsync();
                    if (header == null)
                    {
                        result.ErrorMessage = "Файлът е празен или има невалиден формат.";
                        return result;
                    }

                    // Parse the header to determine column indices
                    var columns = header.Split(',');
                    int schoolIdOrNameIdx = Array.FindIndex(columns, c => c.Contains("Училище", StringComparison.OrdinalIgnoreCase) || c.Contains("SchoolId", StringComparison.OrdinalIgnoreCase));
                    int nameIdx = Array.FindIndex(columns, c => c.Contains("Наименование", StringComparison.OrdinalIgnoreCase) || c.Contains("Name", StringComparison.OrdinalIgnoreCase));
                    int codeIdx = Array.FindIndex(columns, c => c.Contains("Код", StringComparison.OrdinalIgnoreCase) || c.Contains("Code", StringComparison.OrdinalIgnoreCase));
                    int descriptionIdx = Array.FindIndex(columns, c => c.Contains("Описание", StringComparison.OrdinalIgnoreCase) || c.Contains("Description", StringComparison.OrdinalIgnoreCase));
                    int subjectsIdx = Array.FindIndex(columns, c => c.Contains("Предмети", StringComparison.OrdinalIgnoreCase) || c.Contains("Subjects", StringComparison.OrdinalIgnoreCase));
                    int placesIdx = Array.FindIndex(columns, c => c.Contains("Брой места", StringComparison.OrdinalIgnoreCase) || c.Contains("AvailablePlaces", StringComparison.OrdinalIgnoreCase));
                    int typeIdx = Array.FindIndex(columns, c => c.Contains("Тип", StringComparison.OrdinalIgnoreCase) || c.Contains("Type", StringComparison.OrdinalIgnoreCase));
                    int specialtyIdx = Array.FindIndex(columns, c => c.Contains("Специалност", StringComparison.OrdinalIgnoreCase) || c.Contains("Specialty", StringComparison.OrdinalIgnoreCase));
                    int qualificationIdx = Array.FindIndex(columns, c => c.Contains("Квалификация", StringComparison.OrdinalIgnoreCase) || c.Contains("Qualification", StringComparison.OrdinalIgnoreCase));

                    // Check required columns
                    if (schoolIdOrNameIdx == -1 || nameIdx == -1)
                    {
                        result.ErrorMessage = "Файлът трябва да съдържа задължителните колони: 'Училище' и 'Наименование'.";
                        return result;
                    }

                    // Read each line and create profiles
                    string? line;
                    int lineNumber = 1;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        try
                        {
                            var values = SplitCsvLine(line);
                            if (values.Length <= Math.Max(schoolIdOrNameIdx, nameIdx))
                            {
                                errors.Add($"Ред {lineNumber}: Липсват задължителни полета.");
                                failureCount++;
                                continue;
                            }

                            // Find the school by ID or name
                            string schoolIdentifier = values[schoolIdOrNameIdx].Trim();
                            School? school = null;

                            if (int.TryParse(schoolIdentifier, out int schoolId))
                            {
                                school = await _unitOfWork.Schools.GetByIdAsync(schoolId);
                            }
                            else
                            {
                                var schools = await _unitOfWork.Schools.FindAsync(s => s.Name == schoolIdentifier);
                                school = schools.FirstOrDefault();
                            }

                            if (school == null)
                            {
                                errors.Add($"Ред {lineNumber}: Училището '{schoolIdentifier}' не е намерено.");
                                failureCount++;
                                continue;
                            }

                            // Create the profile
                            var profile = new SchoolProfile
                            {
                                SchoolId = school.Id,
                                Name = values[nameIdx].Trim(),
                                Code = codeIdx >= 0 && codeIdx < values.Length ? values[codeIdx].Trim() : string.Empty,
                                Description = descriptionIdx >= 0 && descriptionIdx < values.Length ? values[descriptionIdx].Trim() : string.Empty,
                                Subjects = subjectsIdx >= 0 && subjectsIdx < values.Length ? values[subjectsIdx].Trim() : string.Empty,
                                AvailablePlaces = placesIdx >= 0 && placesIdx < values.Length && int.TryParse(values[placesIdx].Trim(), out int places) ? places : 0
                            };

                            // Set the type and specialty if provided
                            if (typeIdx >= 0 && typeIdx < values.Length && !string.IsNullOrWhiteSpace(values[typeIdx]))
                            {
                                string typeName = values[typeIdx].Trim();

                                // Determine if it's professional or profiled
                                if (typeName.ToLower().Contains("професионал"))
                                {
                                    profile.Type = ProfileType.Професионална;

                                    // For professional profiles, also set the specialty if not already specified
                                    if ((specialtyIdx < 0 || specialtyIdx >= values.Length ||
                                        string.IsNullOrWhiteSpace(values[specialtyIdx])) &&
                                        profile.Specialty == null)
                                    {
                                        profile.Specialty = typeName; // Use the type name as specialty if none provided
                                    }
                                }
                                else
                                {
                                    profile.Type = ProfileType.Профилирана;

                                    // For profiled classes, store the academic focus in Specialty if not provided separately
                                    if ((specialtyIdx < 0 || specialtyIdx >= values.Length ||
                                        string.IsNullOrWhiteSpace(values[specialtyIdx])) &&
                                        profile.Specialty == null)
                                    {
                                        profile.Specialty = typeName; // e.g., "Математически", "Чужди езици"
                                    }
                                }
                            }
                            
                            // Set specialty if provided in the file (this will override any default set above)
                            if (specialtyIdx >= 0 && specialtyIdx < values.Length &&
                                !string.IsNullOrWhiteSpace(values[specialtyIdx]))
                            {
                                profile.Specialty = values[specialtyIdx].Trim();
                            }

                            // Set qualification if provided
                            if (qualificationIdx >= 0 && qualificationIdx < values.Length &&
                                !string.IsNullOrWhiteSpace(values[qualificationIdx]))
                            {
                                profile.ProfessionalQualification = values[qualificationIdx].Trim();
                            }

                            // Check if profile with the same name already exists for this school
                            var existingProfiles = await _unitOfWork.SchoolProfiles.FindAsync(p =>
                                p.SchoolId == school.Id && p.Name == profile.Name);

                            if (existingProfiles.Any())
                            {
                                errors.Add($"Ред {lineNumber}: Профил с име '{profile.Name}' вече съществува за училище '{school.Name}'.");
                                failureCount++;
                                continue;
                            }

                            // Save the profile
                            await _unitOfWork.SchoolProfiles.AddAsync(profile);
                            await _unitOfWork.CompleteAsync();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Ред {lineNumber}: {ex.Message}");
                            failureCount++;
                        }
                    }
                }

                result.IsSuccess = true;
                result.SuccessCount = successCount;
                result.FailureCount = failureCount;
                result.Errors = errors;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Грешка при четене на файла: {ex.Message}";
                result.Errors = errors;
                return result;
            }
        }

       
        private string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    result.Add(line.Substring(startIndex, i - startIndex).Trim('"', ' '));
                    startIndex = i + 1;
                }
            }

            result.Add(line.Substring(startIndex).Trim('"', ' '));
            return result.ToArray();
        }
    }
}