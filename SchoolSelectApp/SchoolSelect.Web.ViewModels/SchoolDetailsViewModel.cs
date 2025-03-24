using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.ViewModels
{
    public class SchoolDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public List<SchoolProfile> Profiles { get; set; } = new List<SchoolProfile>();
        public List<SchoolFacility> Facilities { get; set; } = new List<SchoolFacility>();
        public List<TransportFacility> TransportFacilities { get; set; } = new List<TransportFacility>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<HistoricalRanking> Rankings { get; set; } = new List<HistoricalRanking>();
    }
}
