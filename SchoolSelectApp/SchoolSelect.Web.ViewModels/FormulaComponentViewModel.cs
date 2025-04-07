namespace SchoolSelect.Web.ViewModels
{
    public class FormulaComponentViewModel
    {
        public string SubjectCode { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public int ComponentType { get; set; }

        public double Multiplier { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}