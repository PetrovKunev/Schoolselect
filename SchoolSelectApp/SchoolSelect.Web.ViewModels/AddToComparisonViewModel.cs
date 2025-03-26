using SchoolSelect.Data.Models;

namespace SchoolSelect.ViewModels
{
    public class AddToComparisonViewModel
    {
        /// <summary>
        /// ID на училището, което ще се добавя към сравнение
        /// </summary>
        public int SchoolId { get; set; }

        /// <summary>
        /// ID на профила, ако е избран такъв
        /// </summary>
        public int? ProfileId { get; set; }

        /// <summary>
        /// Списък със съществуващите набори за сравнение на потребителя
        /// </summary>
        public IEnumerable<ComparisonSet> UserComparisonSets { get; set; } = Enumerable.Empty<ComparisonSet>();
    }
}