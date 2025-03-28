namespace SchoolSelect.Web.ViewModels
{
    public class DeleteReviewViewModel
    {
        public int Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
