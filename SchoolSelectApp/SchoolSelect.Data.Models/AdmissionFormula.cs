﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    /// <summary>
    /// Формула за балообразуване за конкретен профил в училище
    /// </summary>
    public class AdmissionFormula
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SchoolProfileId { get; set; }

        [Required]
        [Range(ValidationConstants.Year.Min, ValidationConstants.Year.Max,
              ErrorMessage = ValidationMessages.YearRange)]
        public int Year { get; set; }

        // Формула за балообразуване като текст (напр. "(2*БЕЛ + 2*МАТ) + (1*БЕЛ + 1*М)")
        // Запазваме го за обратна съвместимост и документация
        [Required(ErrorMessage = ValidationMessages.RequiredField)]
        [StringLength(ValidationConstants.AdmissionFormula.ExpressionMaxLength)]
        public string FormulaExpression { get; set; } = null!;

        // Описание на формулата за показване на потребителя
        [StringLength(ValidationConstants.AdmissionFormula.DescriptionMaxLength)]
        public string FormulaDescription { get; set; } = string.Empty;

        // Флаг дали формулата има компоненти и може да бъде използвана
        public bool HasComponents { get; set; } = false;

        // Релация
        [ForeignKey(nameof(SchoolProfileId))]
        public virtual SchoolProfile? SchoolProfile { get; set; }

        // Компоненти, включени във формулата
        public virtual ICollection<FormulaComponent> Components { get; set; } = new List<FormulaComponent>();
    }
}