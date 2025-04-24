using SchoolSelect.Data.Models;

namespace SchoolSelect.Services.Interfaces
{
    /// <summary>
    /// Интерфейс за услуга за изчисляване на бал по формула
    /// </summary>
    public interface IScoreCalculationService
    {
        /// <summary>
        /// Изчислява бал по формула за конкретен профил и оценки на ученик
        /// </summary>
        /// <param name="formulaId">ID на формулата</param>
        /// <param name="userGrades">Оценки на ученика</param>
        /// <returns>Изчислен бал</returns>
        Task<double> CalculateScoreAsync(int formulaId, UserGrades userGrades);
    }
}