using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    public class UserGradesService : IUserGradesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserGradesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserGradesViewModel>> GetUserGradesAsync(Guid userId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesByUserIdAsync(userId);

            return userGrades.Select(g => new UserGradesViewModel
            {
                Id = g.Id,
                ConfigurationName = g.ConfigurationName,
                BulgarianGrade = g.BulgarianGrade,
                MathGrade = g.MathGrade,
                BulgarianExamPoints = g.BulgarianExamPoints,
                MathExamPoints = g.MathExamPoints,
                CreatedAt = g.CreatedAt,
                AdditionalGrades = g.AdditionalGrades.Select(ag => new UserAdditionalGradeViewModel
                {
                    Id = ag.Id,
                    SubjectCode = ag.SubjectCode,
                    SubjectName = ag.SubjectName,
                    ComponentType = ag.ComponentType,
                    Value = ag.Value
                }).ToList()
            }).ToList();
        }

        public async Task<UserGradesViewModel?> GetUserGradeByIdAsync(int gradesId)
        {
            var grades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);

            if (grades == null)
            {
                return null;
            }

            return new UserGradesViewModel
            {
                Id = grades.Id,
                ConfigurationName = grades.ConfigurationName,
                BulgarianGrade = grades.BulgarianGrade,
                MathGrade = grades.MathGrade,
                BulgarianExamPoints = grades.BulgarianExamPoints,
                MathExamPoints = grades.MathExamPoints,
                CreatedAt = grades.CreatedAt,
                AdditionalGrades = grades.AdditionalGrades.Select(ag => new UserAdditionalGradeViewModel
                {
                    Id = ag.Id,
                    SubjectCode = ag.SubjectCode,
                    SubjectName = ag.SubjectName,
                    ComponentType = ag.ComponentType,
                    Value = ag.Value
                }).ToList()
            };
        }

        public async Task<int> CreateUserGradesAsync(UserGradesInputModel model, Guid userId)
        {
            var userGrades = new UserGrades
            {
                UserId = userId,
                ConfigurationName = model.ConfigurationName,
                BulgarianGrade = model.BulgarianGrade,
                MathGrade = model.MathGrade,
                BulgarianExamPoints = model.BulgarianExamPoints,
                MathExamPoints = model.MathExamPoints,
                CreatedAt = DateTime.UtcNow
            };

            // Добавяне на допълнителни оценки, ако има такива
            if (model.AdditionalGrades != null && model.AdditionalGrades.Any())
            {
                foreach (var additionalGrade in model.AdditionalGrades)
                {
                    userGrades.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = additionalGrade.SubjectCode,
                        SubjectName = additionalGrade.SubjectName,
                        ComponentType = additionalGrade.ComponentType,
                        Value = additionalGrade.Value
                    });
                }
            }

            await _unitOfWork.UserGrades.AddAsync(userGrades);
            await _unitOfWork.CompleteAsync();

            return userGrades.Id;
        }

        public async Task UpdateUserGradesAsync(UserGradesInputModel model, int gradesId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);

            if (userGrades == null)
            {
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");
            }

            userGrades.ConfigurationName = model.ConfigurationName;
            userGrades.BulgarianGrade = model.BulgarianGrade;
            userGrades.MathGrade = model.MathGrade;
            userGrades.BulgarianExamPoints = model.BulgarianExamPoints;
            userGrades.MathExamPoints = model.MathExamPoints;

            // Изтриване на всички стари допълнителни оценки
            userGrades.AdditionalGrades.Clear();

            // Добавяне на нови допълнителни оценки
            if (model.AdditionalGrades != null && model.AdditionalGrades.Any())
            {
                foreach (var additionalGrade in model.AdditionalGrades)
                {
                    userGrades.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = additionalGrade.SubjectCode,
                        SubjectName = additionalGrade.SubjectName,
                        ComponentType = additionalGrade.ComponentType,
                        Value = additionalGrade.Value
                    });
                }
            }

            _unitOfWork.UserGrades.Update(userGrades);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteUserGradesAsync(int gradesId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetByIdAsync(gradesId);

            if (userGrades == null)
            {
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");
            }

            _unitOfWork.UserGrades.Remove(userGrades);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<SchoolChanceViewModel> CalculateChanceAsync(int gradesId, int schoolId)
        {
            var userGrades = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);
            var school = await _unitOfWork.Schools.GetSchoolWithDetailsAsync(schoolId);

            if (userGrades == null || school == null)
            {
                throw new InvalidOperationException("Невалидни данни за изчисление");
            }

            var profiles = await _unitOfWork.SchoolProfiles.GetProfilesBySchoolIdAsync(schoolId);
            var result = new SchoolChanceViewModel
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
                    Id = userGrades.Id,
                    ConfigurationName = userGrades.ConfigurationName,
                    BulgarianGrade = userGrades.BulgarianGrade,
                    MathGrade = userGrades.MathGrade,
                    BulgarianExamPoints = userGrades.BulgarianExamPoints,
                    MathExamPoints = userGrades.MathExamPoints
                },
                ProfileChances = new List<ProfileChanceViewModel>()
            };

            // Изчисляване на шанс за всеки профил
            foreach (var profile in profiles)
            {
                // Получаване на последната формула за балообразуване за профила
                var admissionFormula = await _unitOfWork.AdmissionFormulas.GetCurrentFormulaForProfileAsync(profile.Id);

                if (admissionFormula == null)
                {
                    continue;
                }

                // Получаване на историческите данни за минимален бал
                var historicalRankings = await _unitOfWork.HistoricalRankings.GetRankingsByProfileIdAsync(profile.Id);
                var latestRanking = historicalRankings.OrderByDescending(r => r.Year).FirstOrDefault();

                // Изчисляване на бал според формулата
                double calculatedScore = CalculateScore(userGrades, admissionFormula);

                // Изчисляване на шанс за прием
                double chancePercentage = CalculateChancePercentage(calculatedScore, latestRanking?.MinimumScore ?? 0);

                result.ProfileChances.Add(new ProfileChanceViewModel
                {
                    ProfileId = profile.Id,
                    ProfileName = profile.Name,
                    CalculatedScore = calculatedScore,
                    MinimumScoreLastYear = latestRanking?.MinimumScore ?? 0,
                    ChancePercentage = chancePercentage,
                    AvailablePlaces = profile.AvailablePlaces
                });
            }

            return result;
        }

        // Изчисляване на бал според формулата за прием
        private double CalculateScore(UserGrades userGrades, AdmissionFormula formula)
        {
            double score = 0;

            foreach (var component in formula.Components)
            {
                double componentValue = 0;

                // Проверка за основните предмети
                if (component.SubjectCode == "БЕЛ")
                {
                    if (component.ComponentType == 1) // Годишна оценка
                    {
                        componentValue = userGrades.BulgarianGrade;
                    }
                    else if (component.ComponentType == 2) // НВО
                    {
                        componentValue = userGrades.BulgarianExamPoints;
                    }
                }
                else if (component.SubjectCode == "МАТ")
                {
                    if (component.ComponentType == 1) // Годишна оценка
                    {
                        componentValue = userGrades.MathGrade;
                    }
                    else if (component.ComponentType == 2) // НВО
                    {
                        componentValue = userGrades.MathExamPoints;
                    }
                }
                else
                {
                    // Проверка за допълнителни предмети
                    var additionalGrade = userGrades.AdditionalGrades
                        .FirstOrDefault(ag => ag.SubjectCode == component.SubjectCode && ag.ComponentType == component.ComponentType);

                    if (additionalGrade != null)
                    {
                        componentValue = additionalGrade.Value;
                    }
                }

                // Добавяне на компонента към общия бал
                score += componentValue * component.Multiplier;
            }

            return Math.Round(score, 2);
        }

        // Изчисляване на процент шанс за прием
        private double CalculateChancePercentage(double calculatedScore, double minimumScore)
        {
            if (minimumScore <= 0)
            {
                return 50; // Няма данни за минимален бал, връщаме 50%
            }

            if (calculatedScore >= minimumScore + 20)
            {
                return 99; // Много висок бал
            }
            else if (calculatedScore >= minimumScore)
            {
                // Линейна интерполация между 75% и 95%
                double ratio = (calculatedScore - minimumScore) / 20.0;
                return 75 + (ratio * 20);
            }
            else if (calculatedScore >= minimumScore - 20)
            {
                // Линейна интерполация между 15% и 75%
                double ratio = (calculatedScore - (minimumScore - 20)) / 20.0;
                return 15 + (ratio * 60);
            }
            else
            {
                return 10; // Много нисък бал
            }
        }
    }
}