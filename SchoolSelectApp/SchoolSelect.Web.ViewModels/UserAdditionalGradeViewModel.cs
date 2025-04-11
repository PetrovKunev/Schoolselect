namespace SchoolSelect.Web.ViewModels
{
    public class UserAdditionalGradeViewModel
    {
        public int Id { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int ComponentType { get; set; }
        public double Value { get; set; }
    }
}
