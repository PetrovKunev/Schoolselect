// SchoolSelect.Services/Models/GoogleGeocodingResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SchoolSelect.Services.Models
{
    public class GoogleGeocodingResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("results")]
        public List<GeocodingResult> Results { get; set; } = new List<GeocodingResult>();
    }

    public class GeocodingResult
    {
        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; } = new Geometry();

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = string.Empty;
    }

    public class Geometry
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; } = new Location();
    }

    public class Location
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }
}