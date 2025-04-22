using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Interfaces
{
    // Интерфейс за AdmissionService
    public interface IAdmissionService
    {
        /// <summary>
        /// Изчислява шанса за прием на потребител според набор от оценки и училище
        /// </summary>
        Task<SchoolChanceViewModel> CalculateChanceAsync(int gradesId, int schoolId);
    }
}
