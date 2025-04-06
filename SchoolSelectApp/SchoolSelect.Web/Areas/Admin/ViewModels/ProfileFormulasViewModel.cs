namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class ProfileFormulasViewModel
    {
        public int ProfileId { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public List<AdmissionFormulaViewModel> Formulas { get; set; } = new List<AdmissionFormulaViewModel>();
    }
}
