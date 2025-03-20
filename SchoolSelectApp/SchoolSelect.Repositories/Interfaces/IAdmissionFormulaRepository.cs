// IAdmissionFormulaRepository.cs
using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IAdmissionFormulaRepository : IRepository<AdmissionFormula>
    {
        /// <summary>
        /// Взима формула с всички нейни компоненти
        /// </summary>
        /// <param name="formulaId">ID на формулата</param>
        /// <returns>Формулата с компоненти или null ако не съществува</returns>
        Task<AdmissionFormula?> GetFormulaWithComponentsAsync(int formulaId);

        /// <summary>
        /// Взима текущата формула за даден профил
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Текущата формула или null ако не съществува</returns>
        Task<AdmissionFormula?> GetCurrentFormulaForProfileAsync(int profileId);

        /// <summary>
        /// Взима всички формули за даден профил
        /// </summary>
        /// <param name="profileId">ID на профила</param>
        /// <returns>Списък с формули</returns>
        Task<IEnumerable<AdmissionFormula>> GetFormulasByProfileIdAsync(int profileId);
    }
}