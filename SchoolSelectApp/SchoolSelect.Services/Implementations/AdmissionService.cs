using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;


public class AdmissionService : IAdmissionService
{
    private readonly IUserGradesService _gradesService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExpressionParser _parser;
    private readonly IVariableResolver _resolver;
    private readonly IChanceCalculator _chanceCalc;

    public AdmissionService(
        IUserGradesService gradesService,
        IUnitOfWork unitOfWork,
        IExpressionParser parser,
        IVariableResolver resolver,
        IChanceCalculator chanceCalc)
    {
        _gradesService = gradesService;
        _unitOfWork = unitOfWork;
        _parser = parser;
        _resolver = resolver;
        _chanceCalc = chanceCalc;
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
        var historical = await _unitOfWork.HistoricalRankings.GetRankingsByProfileIdsAsync(profiles.Select(p => p.Id));

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
                AdditionalGrades = userGradesEntity.AdditionalGrades.Select(ag => new UserAdditionalGradeViewModel
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
            var formula = await _unitOfWork.AdmissionFormulas.GetCurrentFormulaForProfileAsync(profile.Id);
            var latest = historical
                .Where(r => r.ProfileId == profile.Id)
                .OrderByDescending(r => r.Year)
                .FirstOrDefault();

            double minScore = latest?.MinimumScore ?? 0;
            var vars = _resolver.Resolve(userGradesEntity);

            // Изчисляване на бал
            double score = formula != null
                ? _parser.Evaluate(formula.FormulaExpression, vars)
                : _chanceCalc.FallbackScore(userGradesEntity);

            // Изчисляване на % шанс
            double chance = _chanceCalc.Calculate(score, minScore);

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

        return vm;
    }
}