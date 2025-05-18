// SchoolSelect.Services/Implementations/GoogleGeocodingService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Services.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolSelect.Services.Implementations
{
    public class GoogleGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleGeocodingService> _logger;
        private readonly string _apiKey; // Non-nullable field

        public GoogleGeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleGeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Fix the warning by using null-coalescing operator to assign empty string if null
            _apiKey = configuration["GoogleMaps:ApiKey"] ?? string.Empty;

            // Check if API key is empty and log warning
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Google Maps API ключ не е конфигуриран!");
            }
        }

        public async Task<(double? Latitude, double? Longitude, string FormattedAddress)> GeocodeAddressAsync(string address, string city = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    _logger.LogWarning("Не може да се геокодира празен адрес");
                    return (null, null, string.Empty);
                }

                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogError("Не може да се използва геокодиране без валиден API ключ");
                    return (null, null, string.Empty);
                }

                // Добавяме града към адреса, ако е предоставен
                string fullAddress = address;
                if (!string.IsNullOrWhiteSpace(city))
                {
                    fullAddress += $", {city}, България";
                }

                // Encode the address for URL
                var encodedAddress = Uri.EscapeDataString(fullAddress);

                // Create the request URL
                var requestUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

                // Send the request
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Грешка при получаване на геокодинг данни: {StatusCode}", response.StatusCode);
                    return (null, null, string.Empty);
                }

                // Parse the response
                var content = await response.Content.ReadAsStringAsync();
                var geocodingResponse = JsonSerializer.Deserialize<GoogleGeocodingResponse>(content);

                if (geocodingResponse == null ||
                    geocodingResponse.Status != "OK" ||
                    !geocodingResponse.Results.Any())
                {
                    _logger.LogWarning("Не са намерени резултати за адрес: {Address}", fullAddress);
                    return (null, null, string.Empty);
                }

                var result = geocodingResponse.Results.First();
                return (
                    result.Geometry.Location.Latitude,
                    result.Geometry.Location.Longitude,
                    result.FormattedAddress
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при геокодиране на адрес: {Address}", address);
                return (null, null, string.Empty);
            }
        }
    }
}