namespace SchoolSelect.Web.ViewModels
{
    public class FormulaDisplayViewModel
    {
        public int ProfileId { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public int Year { get; set; }
        public string FormulaExpression { get; set; } = string.Empty;
        public string FormulaDescription { get; set; } = string.Empty;
        public List<FormulaComponentViewModel> Components { get; set; } = new List<FormulaComponentViewModel>();
    }
}
