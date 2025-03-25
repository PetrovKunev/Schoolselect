using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Web.ViewModels
{
    public class UserGradesViewModel
    {
        public int Id { get; set; }
        public string ConfigurationName { get; set; } = string.Empty;
        public double BulgarianGrade { get; set; }
        public double MathGrade { get; set; }
        public double BulgarianExamPoints { get; set; }
        public double MathExamPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserAdditionalGradeViewModel> AdditionalGrades { get; set; } = new List<UserAdditionalGradeViewModel>();
    }
}
