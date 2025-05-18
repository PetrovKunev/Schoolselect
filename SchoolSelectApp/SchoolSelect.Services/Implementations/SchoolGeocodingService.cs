// SchoolSelect.Services/Implementations/SchoolGeocodingService.cs
using Microsoft.Extensions.Logging;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;

namespace SchoolSelect.Services.Implementations
{
    public class SchoolGeocodingService : ISchoolGeocodingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeocodingService _geocodingService;
        private readonly ILogger<SchoolGeocodingService> _logger;

        public SchoolGeocodingService(
            IUnitOfWork unitOfWork,
            IGeocodingService geocodingService,
            ILogger<SchoolGeocodingService> logger)
        {
            _unitOfWork = unitOfWork;
            _geocodingService = geocodingService;
            _logger = logger;
        }

        public async Task<int> UpdateAllSchoolCoordinatesAsync()
        {
            int successCount = 0;
            int failureCount = 0;

            // Взимаме само училищата без координати
            var schools = await _unitOfWork.Schools.FindAsync(s =>
                !s.GeoLatitude.HasValue || !s.GeoLongitude.HasValue);

            _logger.LogInformation("Намерени {Count} училища без координати", schools.Count());

            foreach (var school in schools)
            {
                // Използваме MapsFormattedAddress ако е налично, иначе използваме обикновения адрес
                string addressToGeocode = !string.IsNullOrEmpty(school.MapsFormattedAddress)
                    ? school.MapsFormattedAddress
                    : school.Address;

                // Проверка дали има адрес за геокодиране
                if (string.IsNullOrWhiteSpace(addressToGeocode))
                {
                    _logger.LogWarning("Училище с ID {SchoolId} и име {SchoolName} няма адрес за геокодиране",
                        school.Id, school.Name);
                    failureCount++;
                    continue;
                }

                try
                {
                    // Геокодиране на адреса
                    var geocodeResult = await _geocodingService.GeocodeAddressAsync(addressToGeocode, school.City);
                    double? latitude = geocodeResult.Latitude;
                    double? longitude = geocodeResult.Longitude;

                    // Ако геокодирането е успешно, обновяваме училището
                    if (latitude.HasValue && longitude.HasValue)
                    {
                        school.GeoLatitude = latitude.Value;
                        school.GeoLongitude = longitude.Value;

                        // Ако имаме форматиран адрес от Google, запазваме го
                        if (!string.IsNullOrEmpty(geocodeResult.FormattedAddress))
                        {
                            school.MapsFormattedAddress = geocodeResult.FormattedAddress;
                        }

                        _unitOfWork.Schools.Update(school);
                        await _unitOfWork.CompleteAsync();

                        _logger.LogInformation(
                            "Успешно обновени координати за училище {SchoolName} (ID: {SchoolId}): {Lat}, {Lon}",
                            school.Name, school.Id, latitude.Value, longitude.Value);

                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Неуспешно геокодиране за училище {SchoolName} (ID: {SchoolId}) с адрес: {Address}",
                            school.Name, school.Id, addressToGeocode);

                        failureCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при обновяване на координати за училище {SchoolName} (ID: {SchoolId})",
                        school.Name, school.Id);

                    failureCount++;
                }

                // Пауза между заявките, за да избегнем надвишаване на лимита на API
                await Task.Delay(TimeSpan.FromSeconds(1.5));
            }

            _logger.LogInformation("Обновяване на координати завършено. Успешни: {SuccessCount}, Неуспешни: {FailureCount}",
                successCount, failureCount);

            return successCount;
        }

        public async Task<bool> UpdateSchoolCoordinatesAsync(int schoolId)
        {
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

            if (school == null)
            {
                _logger.LogWarning("Училище с ID {SchoolId} не е намерено", schoolId);
                return false;
            }

            string addressToGeocode = !string.IsNullOrEmpty(school.MapsFormattedAddress)
                ? school.MapsFormattedAddress
                : school.Address;

            if (string.IsNullOrWhiteSpace(addressToGeocode))
            {
                _logger.LogWarning("Училище с ID {SchoolId} и име {SchoolName} няма адрес за геокодиране",
                    schoolId, school.Name);
                return false;
            }

            try
            {
                // Геокодиране на адреса и правилно получаване на резултата
                var geocodeResult = await _geocodingService.GeocodeAddressAsync(addressToGeocode, school.City);
                double? latitude = geocodeResult.Latitude;
                double? longitude = geocodeResult.Longitude;
                string formattedAddress = geocodeResult.FormattedAddress;

                if (latitude.HasValue && longitude.HasValue)
                {
                    school.GeoLatitude = latitude.Value;
                    school.GeoLongitude = longitude.Value;

                    // Ако имаме форматиран адрес от Google, запазваме го
                    if (!string.IsNullOrEmpty(formattedAddress))
                    {
                        school.MapsFormattedAddress = formattedAddress;
                    }

                    _unitOfWork.Schools.Update(school);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation(
                        "Успешно обновени координати за училище {SchoolName} (ID: {SchoolId}): {Lat}, {Lon}",
                        school.Name, schoolId, latitude.Value, longitude.Value);

                    return true;
                }

                _logger.LogWarning(
                    "Неуспешно геокодиране за училище {SchoolName} (ID: {SchoolId}) с адрес: {Address}",
                    school.Name, schoolId, addressToGeocode);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при обновяване на координати за училище {SchoolName} (ID: {SchoolId})",
                    school.Name, schoolId);

                return false;
            }
        }
    }
}