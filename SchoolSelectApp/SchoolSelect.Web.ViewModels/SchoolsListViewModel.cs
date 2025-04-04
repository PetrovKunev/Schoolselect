namespace SchoolSelect.Web.ViewModels
{
    public class SchoolsListViewModel
    {
        public List<SchoolViewModel> Schools { get; set; } = new List<SchoolViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public string SelectedDistrict { get; set; } = string.Empty;
        public string SelectedProfileType { get; set; } = string.Empty;
        public List<string> Districts { get; set; } = new List<string>();
        public List<string> ProfileTypes { get; set; } = new List<string>() { "Профилирана", "Професионална" };
    }
}

