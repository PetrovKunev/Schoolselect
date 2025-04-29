using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class ComparisonService : IComparisonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ComparisonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Връща всички набори за сравнение на потребителя
        /// </summary>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>Списък с набори за сравнение</returns>
        public async Task<IEnumerable<ComparisonSet>> GetComparisonSetsByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.ComparisonSets.GetComparisonSetsByUserIdAsync(userId);
        }

        /// <summary>
        /// Връща детайли за конкретен набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>Набор за сравнение с детайли или null</returns>
        public async Task<ComparisonViewModel?> GetComparisonDetailsAsync(int comparisonSetId, Guid userId)
        {
            var comparisonSet = await _unitOfWork.ComparisonSets.GetComparisonSetWithItemsAsync(comparisonSetId);

            if (comparisonSet == null || comparisonSet.UserId != userId)
                return null;

            var viewModel = new ComparisonViewModel
            {
                Id = comparisonSet.Id,
                Name = comparisonSet.Name,
                CreatedAt = comparisonSet.CreatedAt,
                UserId = comparisonSet.UserId,
                Items = new List<ComparisonItemViewModel>(),
                ComparisonCriteria = new List<string> { "Рейтинг", "Района", "Транспорт", "Профили", "Минимален бал" }
            };

            foreach (var item in comparisonSet.Items)
            {
                var school = item.School;
                var profile = item.Profile;

                if (school == null)
                    continue;

                var itemViewModel = new ComparisonItemViewModel
                {
                    Id = item.Id,
                    SchoolId = item.SchoolId,
                    SchoolName = school.Name,
                    SchoolAddress = school.Address,
                    City = school.City,
                    District = school.District,
                    AverageRating = school.AverageRating,
                    ProfileId = item.ProfileId,
                    ProfileName = profile?.Name,
                    ProfileCode = profile?.Code,
                    FacilitiesCount = 0
                };

                // Допълнителни данни, ако е необходимо
                if (item.ProfileId.HasValue)
                {
                    // Вземане на среден минимален бал за последните години
                    itemViewModel.AverageMinimumScore = await _unitOfWork.HistoricalRankings
                        .GetAverageMinimumScoreAsync(item.SchoolId, item.ProfileId.Value);

                    // Вземане на брой места
                    var profileInfo = await _unitOfWork.SchoolProfiles.GetByIdAsync(item.ProfileId.Value);
                    itemViewModel.AvailablePlaces = profileInfo?.AvailablePlaces;

                    // Вземане на последно класиране
                    var lastYearRankings = (await _unitOfWork.HistoricalRankings
                        .GetRankingsByProfileIdAsync(item.ProfileId.Value))
                        .OrderByDescending(r => r.Year)
                        .ThenBy(r => r.Round)
                        .FirstOrDefault();

                    itemViewModel.LastYearMinimumScore = lastYearRankings?.MinimumScore;
                }

                // Вземане на брой съоръжения
                var facilities = await _unitOfWork.SchoolFacilities.GetFacilitiesBySchoolIdAsync(item.SchoolId);
                itemViewModel.FacilitiesCount = facilities.Count();

                // Вземане на топ съоръжения (до 3)
                itemViewModel.TopFacilities = facilities.Take(3).ToList();

                viewModel.Items.Add(itemViewModel);
            }

            return viewModel;
        }

        /// <summary>
        /// Създава нов празен набор за сравнение
        /// </summary>
        /// <param name="name">Име на набора</param>
        /// <param name="userId">ID на потребителя</param>
        /// <returns>ID на създадения набор</returns>
        public async Task<int> CreateComparisonSetAsync(string name, Guid userId)
        {
            var comparisonSet = new ComparisonSet
            {
                Name = name,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ComparisonSets.AddAsync(comparisonSet);
            await _unitOfWork.CompleteAsync();

            return comparisonSet.Id;
        }

        /// <summary>
        /// Добавя училище/профил към набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="schoolId">ID на училището</param>
        /// <param name="profileId">ID на профила (ако е избран)</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако добавянето е успешно</returns>
        public async Task<bool> AddItemToComparisonAsync(int comparisonSetId, int schoolId, int? profileId, Guid userId)
        {
            var comparisonSet = await _unitOfWork.ComparisonSets.GetComparisonSetWithItemsAsync(comparisonSetId);

            if (comparisonSet == null || comparisonSet.UserId != userId)
                return false;

            // Проверка дали вече не съществува такъв елемент
            var existingItem = comparisonSet.Items
                .FirstOrDefault(i => i.SchoolId == schoolId && i.ProfileId == profileId);

            if (existingItem != null)
                return true; // Елементът вече съществува

            // Проверка дали училището/профилът съществуват
            var school = await _unitOfWork.Schools.GetByIdAsync(schoolId);

            if (school == null)
                return false;

            if (profileId.HasValue)
            {
                var profile = await _unitOfWork.SchoolProfiles.GetByIdAsync(profileId.Value);
                if (profile == null || profile.SchoolId != schoolId)
                    return false;
            }

            // Добавяне на нов елемент
            var comparisonItem = new ComparisonItem
            {
                ComparisonSetId = comparisonSetId,
                SchoolId = schoolId,
                ProfileId = profileId
            };

            comparisonSet.Items.Add(comparisonItem);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Премахва елемент от набор за сравнение
        /// </summary>
        /// <param name="itemId">ID на елемента за премахване</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако премахването е успешно</returns>
        public async Task<bool> RemoveItemFromComparisonAsync(int itemId, Guid userId)
        {
            // Намиране на елемента
            var comparisonSet = await _unitOfWork.ComparisonSets
                .FindAsync(c => c.Items.Any(i => i.Id == itemId));

            var comparisonItem = comparisonSet.FirstOrDefault();

            if (comparisonItem == null || comparisonItem.UserId != userId)
                return false;

            // Намиране на компонента, който трябва да се премахне
            var item = comparisonItem.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return false;

            // Премахване на елемента
            comparisonItem.Items.Remove(item);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Изтрива набор за сравнение
        /// </summary>
        /// <param name="comparisonSetId">ID на набора за сравнение</param>
        /// <param name="userId">ID на потребителя (за проверка на достъп)</param>
        /// <returns>True ако изтриването е успешно</returns>
        public async Task<bool> DeleteComparisonSetAsync(int comparisonSetId, Guid userId)
        {
            var comparisonSet = await _unitOfWork.ComparisonSets.GetComparisonSetWithItemsAsync(comparisonSetId);

            if (comparisonSet == null || comparisonSet.UserId != userId)
                return false;

            // Изтриване на всички елементи от набора
            foreach (var item in comparisonSet.Items.ToList())
            {
                comparisonSet.Items.Remove(item);
            }

            // Изтриване на самия набор
            _unitOfWork.ComparisonSets.Remove(comparisonSet);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}