using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Implementations
{
    /// <summary>
    /// Отговорен единствено за CRUD операции върху оценки на потребителя
    /// </summary>
    public class UserGradesService : IUserGradesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserGradesService> _logger;

        public UserGradesService(
            IUnitOfWork unitOfWork,
            ILogger<UserGradesService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<UserGradesViewModel>> GetUserGradesAsync(Guid userId)
        {
            var entities = await _unitOfWork.UserGrades.GetGradesByUserIdAsync(userId);
            return entities.Select(MapToViewModel).ToList();
        }

        public async Task<UserGradesViewModel?> GetUserGradeByIdAsync(int gradesId)
        {
            var entity = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);
            return entity == null ? null : MapToViewModel(entity);
        }

        public async Task<int> CreateUserGradesAsync(UserGradesInputModel model, Guid userId)
        {
            var entity = new UserGrades
            {
                UserId = userId,
                ConfigurationName = model.ConfigurationName,
                BulgarianGrade = model.BulgarianGrade,
                MathGrade = model.MathGrade,
                BulgarianExamPoints = model.BulgarianExamPoints,
                MathExamPoints = model.MathExamPoints,
                CreatedAt = DateTime.UtcNow
            };

            if (model.AdditionalGrades != null)
            {
                foreach (var ag in model.AdditionalGrades)
                {
                    entity.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = ag.SubjectCode,
                        SubjectName = ag.SubjectName,
                        ComponentType = ag.ComponentType,
                        Value = ag.Value
                    });
                }
            }

            await _unitOfWork.UserGrades.AddAsync(entity);
            await _unitOfWork.CompleteAsync();
            return entity.Id;
        }

        public async Task UpdateUserGradesAsync(UserGradesInputModel model, int gradesId)
        {
            var entity = await _unitOfWork.UserGrades.GetGradesWithAdditionalGradesAsync(gradesId);
            if (entity == null)
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");

            // Обновяване на основни полета
            entity.ConfigurationName = model.ConfigurationName;
            entity.BulgarianGrade = model.BulgarianGrade;
            entity.MathGrade = model.MathGrade;
            entity.BulgarianExamPoints = model.BulgarianExamPoints;
            entity.MathExamPoints = model.MathExamPoints;

            // Рефреш на допълнителни оценки
            entity.AdditionalGrades.Clear();
            if (model.AdditionalGrades != null)
            {
                foreach (var ag in model.AdditionalGrades)
                {
                    entity.AdditionalGrades.Add(new UserAdditionalGrade
                    {
                        SubjectCode = ag.SubjectCode,
                        SubjectName = ag.SubjectName,
                        ComponentType = ag.ComponentType,
                        Value = ag.Value
                    });
                }
            }

            _unitOfWork.UserGrades.Update(entity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteUserGradesAsync(int gradesId)
        {
            var entity = await _unitOfWork.UserGrades.GetByIdAsync(gradesId);
            if (entity == null)
                throw new InvalidOperationException($"Не е намерен набор от оценки с ID {gradesId}");

            _unitOfWork.UserGrades.Remove(entity);
            await _unitOfWork.CompleteAsync();
        }

        // Махаме логиката за формули и шанс - тези отговорности са прехвърлени на AdmissionService

        private static UserGradesViewModel MapToViewModel(UserGrades g)
        {
            return new UserGradesViewModel
            {
                Id = g.Id,
                ConfigurationName = g.ConfigurationName,
                BulgarianGrade = g.BulgarianGrade ?? 0,
                MathGrade = g.MathGrade ?? 0,
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
            };
        }
    }
}
