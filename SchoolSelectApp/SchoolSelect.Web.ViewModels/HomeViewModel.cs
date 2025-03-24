namespace SchoolSelect.Web.ViewModels
{
    public class HomeViewModel
    {
        public List<SchoolViewModel> TopRatedSchools { get; set; } = new List<SchoolViewModel>();
        public int TotalSchoolsCount { get; set; }
        public int TotalProfilesCount { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}
