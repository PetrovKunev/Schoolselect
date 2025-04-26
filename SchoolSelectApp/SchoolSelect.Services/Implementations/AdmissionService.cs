using Microsoft.Extensions.Logging;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class AdmissionService : IAdmissionService
    {
        private readonly IUserGradesService _gradesService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChanceCalculator _chanceCalc;
        private readonly ILogger<AdmissionService> _logger;
        private readonly IScoreCalculationService _scoreCalculator;

        public AdmissionService(
            IUserGradesService gradesService,
            IUnitOfWork unitOfWork,
            IChanceCalculator chanceCalc,
            ILogger<AdmissionService> logger,
            IScoreCalculationService scoreCalculator)
        {
            _gradesService = gradesService;
            _unitOfWork = unitOfWork;
            _chanceCalc = chanceCalc;
            _logger = logger;
            _scoreCalculator = scoreCalculator;
        }

        public async Task<SchoolChanceViewModel> CalculateChanceAsync(int gradesId, int schoolId)
        {
            // Извличане на данни
            var userGradesEntity = await _unitOfWork.UserGrades
                .GetGradesWithAdditionalGradesAsync(gradesId);

            if (userGradesEntity == null)
                throw new InvalidOperationException($"Не са намерени оценки с ID {gradesId}");

            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(schoolId);
            if (school == null)
                throw new InvalidOperationException($"Не е намерено училище с ID {schoolId}");

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(schoolId);
            var historical = await _unitOfWork.HistoricalRankings
                .GetRankingsByProfileIdsAsync(profiles.Select(p => p.Id));

            // Начално попълване на ViewModel
            var vm = new SchoolChanceViewModel
            {
                School = new SchoolViewModel
                {
                    Id = school.Id,
                    Name = school.Name,
                    District = school.District,
                    City = school.City,
                    AverageRating = school.AverageRating,
                    ReviewsCount = school.RatingsCount
                },
                UserGrades = new UserGradesViewModel
                {
                    Id = userGradesEntity.Id,
                    ConfigurationName = userGradesEntity.ConfigurationName,
                    BulgarianGrade = userGradesEntity.BulgarianGrade ?? 0,
                    MathGrade = userGradesEntity.MathGrade ?? 0,
                    BulgarianExamPoints = userGradesEntity.BulgarianExamPoints,
                    MathExamPoints = userGradesEntity.MathExamPoints,
                    AdditionalGrades = userGradesEntity.AdditionalGrades.Select(ag =>
                        new UserAdditionalGradeViewModel
                        {
                            Id = ag.Id,
                            SubjectCode = ag.SubjectCode,
                            SubjectName = ag.SubjectName,
                            ComponentType = ag.ComponentType,
                            Value = ag.Value
                        }).ToList()
                },
                ProfileChances = new List<ProfileChanceViewModel>()
            };

            // Цикъл през профилите
            foreach (var profile in profiles)
            {
                try
                {
                    var formula = await _unitOfWork.AdmissionFormulas
                        .GetCurrentFormulaForProfileAsync(profile.Id);

                    var latest = historical
                        .Where(r => r.ProfileId == profile.Id)
                        .OrderByDescending(r => r.Year)
                        .FirstOrDefault();

                    double minScore = latest?.MinimumScore ?? 0;
                    double score = 0;

                    // Променено от IsStructured на HasComponents
                    if (formula != null && formula.HasComponents)
                    {
                        _logger.LogInformation("Изчисляване на бал по компоненти за профил {ProfileId} ({ProfileName})",
                            profile.Id, profile.Name);

                        // Използваме новата услуга за изчисление
                        score = await _scoreCalculator.CalculateScoreAsync(formula.Id, userGradesEntity);
                    }
                    else
                    {
                        _logger.LogWarning("Липсва формула или формулата няма компоненти за профил {ProfileId} ({ProfileName})",
                            profile.Id, profile.Name);

                        // Използваме резервен метод, ако няма формула
                        score = _chanceCalc.FallbackScore(userGradesEntity);
                    }

                    _logger.LogInformation("Изчислен бал за профил {ProfileId}: {Score}", profile.Id, score);
                    _logger.LogInformation("Минимален бал от минала година: {MinScore}", minScore);

                    // Изчисляване на % шанс
                    double chance = _chanceCalc.Calculate(score, minScore);
                    _logger.LogInformation("Изчислен шанс: {Chance}%", chance);

                    // Добавяне в резултатите
                    vm.ProfileChances.Add(new ProfileChanceViewModel
                    {
                        ProfileId = profile.Id,
                        ProfileName = profile.Name,
                        CalculatedScore = Math.Round(score, 2),
                        MinimumScoreLastYear = minScore,
                        ChancePercentage = Math.Round(chance, 2),
                        AvailablePlaces = profile.AvailablePlaces
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при изчисляване на шанс за профил {ProfileId}", profile.Id);

                    // Добавяме профила с нулеви стойности и съобщение за грешка
                    vm.ProfileChances.Add(new ProfileChanceViewModel
                    {
                        ProfileId = profile.Id,
                        ProfileName = profile.Name + " (Грешка при изчисляване)",
                        CalculatedScore = 0,
                        MinimumScoreLastYear = 0,
                        ChancePercentage = 0,
                        AvailablePlaces = profile.AvailablePlaces
                    });
                }
            }

            return vm;
        }
    }
}