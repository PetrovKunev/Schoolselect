// IComparisonSetRepository.cs
using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IComparisonSetRepository : IRepository<ComparisonSet>
    {
        /// <summary>
        /// Взима всички набори за сравнение на даден потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък със сравнения</returns>
        Task<IEnumerable<ComparisonSet>> GetComparisonSetsByUserIdAsync(Guid userId);

        /// <summary>
        /// Взима конкретно сравнение заедно с елементите му
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <returns>Набор за сравнение или null ако не съществува</returns>
        Task<ComparisonSet?> GetComparisonSetWithItemsAsync(int comparisonSetId);
    }
}