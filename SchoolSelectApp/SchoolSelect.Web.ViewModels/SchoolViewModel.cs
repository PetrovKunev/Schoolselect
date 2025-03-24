namespace SchoolSelect.Web.ViewModels
{
    public class SchoolViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public List<string> ProfileTypes { get; set; } = new List<string>();
        public double? MinimumScore { get; set; }
    }
}
