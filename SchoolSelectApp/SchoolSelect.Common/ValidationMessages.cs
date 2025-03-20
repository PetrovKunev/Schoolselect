using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Common
{
    /// <summary>
    /// Съобщения за грешки при валидация
    /// </summary>
    public static class ValidationMessages
    {
        // Общи съобщения
        public const string RequiredField = "Полето е задължително";
        public const string MaxLengthExceeded = "Превишена е максималната дължина на полето";
        public const string MinLengthRequired = "Полето трябва да съдържа поне {1} символа";

        // Форматни съобщения
        public const string InvalidEmail = "Невалиден имейл адрес";
        public const string InvalidPhone = "Невалиден телефонен номер";
        public const string InvalidUrl = "Невалиден URL адрес";

        // Географски координати
        public const string LatitudeRange = "Географската ширина трябва да е между -90 и 90";
        public const string LongitudeRange = "Географската дължина трябва да е между -180 и 180";

        // Рейтинг и оценки
        public const string RatingRange = "Оценката трябва да е между 1 и 5";
        public const string GradeRange = "Оценката трябва да е между 2 и 6";
        public const string ExamPointsRange = "Точките от изпита трябва да са между 0 и 100";
        public const string ValueRange = "Стойността трябва да е в допустимия диапазон";

        // Училище и профили
        public const string YearRange = "Годината трябва да е между 2000 и 2100";
        public const string ScoreRange = "Балът трябва да е между 0 и 500";
        public const string RoundRange = "Кръгът на класиране трябва да е между 1 и 5";
        public const string StudentsRange = "Броят приети ученици трябва да е положително число";
        public const string PlacesRange = "Броят места трябва да е положително число";

        // Отзиви
        public const string ReviewLengthExceeded = "Отзивът не може да е по-дълъг от 2000 символа";

        // Пароли и потребители
        public const string PasswordTooShort = "Паролата трябва да е поне 8 символа";
        public const string PasswordRequiresDigit = "Паролата трябва да съдържа поне една цифра";
        public const string PasswordRequiresUpper = "Паролата трябва да съдържа поне една главна буква";
        public const string PasswordRequiresLower = "Паролата трябва да съдържа поне една малка буква";
        public const string PasswordRequiresNonAlphanumeric = "Паролата трябва да съдържа поне един специален символ";

        // Сравнения
        public const string MaxComparisonItems = "Не може да сравнявате повече от 10 училища едновременно";
    }
}
