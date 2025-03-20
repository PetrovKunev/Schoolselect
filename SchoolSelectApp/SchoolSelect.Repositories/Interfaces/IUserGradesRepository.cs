using SchoolSelect.Data.Models;

namespace SchoolSelect.Repositories.Interfaces
{
    public interface IUserGradesRepository : IRepository<UserGrades>
    {
        /// <summary>
        /// Връща всички набори от оценки на потребител
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с набори от оценки</returns>
        Task<IEnumerable<UserGrades>> GetGradesByUserIdAsync(Guid userId);

        /// <summary>
        /// Връща набор от оценки заедно с допълнителните оценки
        /// </summary>
        /// <param name="gradesId">ID на набора от оценки</param>
        /// <returns>Набор от оценки с допълнителните оценки или null ако не съществува</returns>
        Task<UserGrades?> GetGradesWithAdditionalGradesAsync(int gradesId);
    }
}