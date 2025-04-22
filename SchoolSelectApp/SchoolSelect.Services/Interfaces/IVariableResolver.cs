// 2. Интерфейс за подготовка на речник с променливи

using SchoolSelect.Data.Models;

namespace SchoolSelect.Services.Interfaces
{
    public interface IVariableResolver
    {
        /// <summary>
        /// Превръща UserGrades ентити в речник от ключове и стойности.
        /// </summary>
        IDictionary<string, double> Resolve(UserGrades userGrades);
    }
}