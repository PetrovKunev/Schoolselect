namespace SchoolSelect.Web.ViewModels
{
    public class ProfileChanceViewModel
    {
        public int ProfileId { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public double CalculatedScore { get; set; }
        public double MinimumScoreLastYear { get; set; }
        public double ChancePercentage { get; set; }
        public int AvailablePlaces { get; set; }
        public bool IsNewProfile { get; set; }
    }
}
