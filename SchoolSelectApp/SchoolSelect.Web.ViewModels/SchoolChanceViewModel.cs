using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Web.ViewModels
{
    public class SchoolChanceViewModel
    {
        public SchoolViewModel School { get; set; }
        public UserGradesViewModel UserGrades { get; set; }
        public List<ProfileChanceViewModel> ProfileChances { get; set; } = new List<ProfileChanceViewModel>();
    }

}
