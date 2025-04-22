using SchoolSelect.Web.ViewModels;


namespace SchoolSelect.Services.Interfaces
{
    public interface IUserGradesService
    {
        /// <summary>
        /// Получава всички набори с оценки на потребителя
        /// </summary>
        Task<List<UserGradesViewModel>> GetUserGradesAsync(Guid userId);

        /// <summary>
        /// Получава конкретен набор от оценки
        /// </summary>
        Task<UserGradesViewModel?> GetUserGradeByIdAsync(int gradesId);

        /// <summary>
        /// Създава нов набор от оценки
        /// </summary>
        Task<int> CreateUserGradesAsync(UserGradesInputModel model, Guid userId);

        /// <summary>
        /// Обновява съществуващ набор от оценки
        /// </summary>
        Task UpdateUserGradesAsync(UserGradesInputModel model, int gradesId);

        /// <summary>
        /// Изтрива набор от оценки
        /// </summary>
        Task DeleteUserGradesAsync(int gradesId);


    }
}