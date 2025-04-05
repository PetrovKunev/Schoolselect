using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class SchoolProfilesManageViewModel
    {
        public required School School { get; set; }
        public List<SchoolProfile> Profiles { get; set; } = new List<SchoolProfile>();
    }
}
