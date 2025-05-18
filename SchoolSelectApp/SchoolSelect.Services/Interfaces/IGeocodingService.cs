// SchoolSelect.Services/Interfaces/IGeocodingService.cs
using System.Threading.Tasks;

namespace SchoolSelect.Services.Interfaces
{
    public interface IGeocodingService
    {
        /// <summary>
        /// Геокодира адрес до географски координати
        /// </summary>
        /// <param name="address">Адрес за геокодиране</param>
        /// <param name="city">Град (по желание)</param>
        /// <returns>Latitude, Longitude и форматиран адрес</returns>
        Task<(double? Latitude, double? Longitude, string FormattedAddress)> GeocodeAddressAsync(string address, string city = "");
    }
}