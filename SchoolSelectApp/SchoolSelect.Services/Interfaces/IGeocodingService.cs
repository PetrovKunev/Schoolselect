// SchoolSelect.Services/Interfaces/IGeocodingService.cs
using System.Threading.Tasks;

namespace SchoolSelect.Services.Interfaces
{
    public interface IGeocodingService
    {
        // Метод, който връща Task<(double? Latitude, double? Longitude, string FormattedAddress)>
        Task<(double? Latitude, double? Longitude, string FormattedAddress)> GeocodeAddressAsync(string address);
    }
}