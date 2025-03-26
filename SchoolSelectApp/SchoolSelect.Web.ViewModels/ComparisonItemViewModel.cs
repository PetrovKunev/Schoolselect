using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.ViewModels
{
    public class ComparisonItemViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public double AverageRating { get; set; }

        // Данни за профила (ако е избран)
        public int? ProfileId { get; set; }
        public string? ProfileName { get; set; }
        public string? ProfileCode { get; set; }

        // Допълнителни данни за сравнение
        public double? AverageMinimumScore { get; set; }
        public int? AvailablePlaces { get; set; }

        // Данни за съоръжения и транспорт
        public int FacilitiesCount { get; set; }
        public List<SchoolFacility> TopFacilities { get; set; } = new List<SchoolFacility>();

        // Образователни резултати
        public double? LastYearMinimumScore { get; set; }
    }
}
