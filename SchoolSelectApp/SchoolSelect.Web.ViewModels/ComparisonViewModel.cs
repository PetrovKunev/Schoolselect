using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Web.ViewModels
{
    public class ComparisonViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }

        // Списък със сравнявани елементи
        public List<ComparisonItemViewModel> Items { get; set; } = new List<ComparisonItemViewModel>();

        // Обобщени данни за сравнение
        public List<string> ComparisonCriteria { get; set; } = new List<string>();
    }

}
