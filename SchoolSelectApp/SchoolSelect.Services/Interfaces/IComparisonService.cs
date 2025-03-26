using SchoolSelect.Data.Models;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Interfaces
{
    public interface IComparisonService
    {
        /// <summary>
        /// Връща всички набори за сравнение на потребителя
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с набори за сравнение</returns>
        Task<IEnumerable<ComparisonSet>> GetComparisonSetsByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща детайли за конкретен набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>Набор за сравнение с детайли или null</returns>
        Task<ComparisonViewModel?> GetComparisonDetailsAsync(int comparisonSetId, Guid userId);

        /// <summary>
        /// Създава нов празен набор за сравнение
        /// </summary>
        /// <param name="name">Име на набора</param>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>ID на създадения набор</returns>
        Task<int> CreateComparisonSetAsync(string name, Guid userId);

        /// <summary>
        /// Добавя училище/профил към набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="schoolId">ID на училището</param>
        /// <param name="profileId">ID на профила (ако е избран)</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако добавянето е успешно</returns>
        Task<bool> AddItemToComparisonAsync(int comparisonSetId, int schoolId, int? profileId, Guid userId);

        /// <summary>
        /// Премахва елемент от набор за сравнение
        /// </summary>
        /// <param name="itemId">ID на елемента за премахване</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако премахването е успешно</returns>
        Task<bool> RemoveItemFromComparisonAsync(int itemId, Guid userId);

        /// <summary>
        /// Изтрива набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако изтриването е успешно</returns>
        Task<bool> DeleteComparisonSetAsync(int comparisonSetId, Guid userId);
    }
}