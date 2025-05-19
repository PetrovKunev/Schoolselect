namespace SchoolSelect.Web.ViewModels
{
    public class SchoolRecommendationsViewModel
    {
        public int PreferenceId { get; set; }
        public string PreferenceName { get; set; } = string.Empty;
        public string UserDistrict { get; set; } = string.Empty;
        public string UserCoordinates { get; set; } = string.Empty;
        public List<string> PreferredProfiles { get; set; } = new List<string>();
        public List<SchoolRecommendationViewModel> RecommendedSchools { get; set; } = new List<SchoolRecommendationViewModel>();

        // Нови свойства за набори от оценки
        public List<UserGradesViewModel> AvailableGradesSets { get; set; } = new List<UserGradesViewModel>();
        public int? SelectedGradesId { get; set; }
    }
}