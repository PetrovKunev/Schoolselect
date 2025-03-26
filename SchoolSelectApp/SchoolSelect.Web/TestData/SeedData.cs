using Microsoft.EntityFrameworkCore;
using SchoolSelect.Data.Models;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Web.TestData
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Прилагаме миграциите, ако има нови
            context.Database.Migrate();

            // Проверяваме дали вече има училища
            if (context.Schools.Any())
            {
                return; // Базата вече е заредена
            }

            // Добавяме училища
            var schools = new List<School>
            {
                new School
                {
                    Name = "Математическа гимназия 'Акад. Кирил Попов'",
                    Address = "ул. Чемшир 11",
                    District = "Център",
                    City = "Пловдив",
                    Phone = "032/643-157",
                    Email = "omg_plovdiv@abv.bg",
                    Website = "https://omg-plovdiv.com",
                    Description = "Математическа гимназия с дългогодишна история и традиции.",
                    GeoLatitude = 42.1472,
                    GeoLongitude = 24.7493,
                    AverageRating = 4.8,
                    RatingsCount = 45
                },
                new School
                {
                    Name = "Национална търговска гимназия",
                    Address = "бул. Руски 50",
                    District = "Център",
                    City = "Пловдив",
                    Phone = "032/642-673",
                    Email = "ntg.plovdiv@gmail.com",
                    Website = "https://ntgplovdiv.com",
                    Description = "Профилирана гимназия в обучението по икономика и финанси.",
                    GeoLatitude = 42.1465,
                    GeoLongitude = 24.7538,
                    AverageRating = 4.5,
                    RatingsCount = 32
                },
                new School
                {
                    Name = "Езикова гимназия 'Иван Вазов'",
                    Address = "ул. Стоян Михайловски 2",
                    District = "Център",
                    City = "Пловдив",
                    Phone = "032/642-289",
                    Email = "eg_iv_plovdiv@abv.bg",
                    Website = "https://egivplovdiv.com",
                    Description = "Водещо езиково училище с дългогодишни традиции.",
                    GeoLatitude = 42.1447,
                    GeoLongitude = 24.7462,
                    AverageRating = 4.7,
                    RatingsCount = 38
                }
            };

            context.Schools.AddRange(schools);
            context.SaveChanges();

            // Добавяме профили към училищата
            var profiles = new List<SchoolProfile>
            {
                new SchoolProfile
                {
                    SchoolId = schools[0].Id,
                    Name = "Софтуерни и хардуерни науки",
                    Code = "СХН",
                    Description = "Профилът включва интензивно обучение по математика и информатика.",
                    Subjects = "Математика, Информатика, Английски език",
                    AvailablePlaces = 26
                },
                new SchoolProfile
                {
                    SchoolId = schools[0].Id,
                    Name = "Природо-математически",
                    Code = "ПМ",
                    Description = "Профилът включва интензивно обучение по математика и природни науки.",
                    Subjects = "Математика, Физика, Химия, Английски език",
                    AvailablePlaces = 26
                },
                new SchoolProfile
                {
                    SchoolId = schools[1].Id,
                    Name = "Икономика и мениджмънт",
                    Code = "ИМ",
                    Description = "Профил за подготовка на бъдещи икономисти и мениджъри.",
                    Subjects = "Икономика, Мениджмънт, Счетоводство, Английски език",
                    AvailablePlaces = 26
                },
                new SchoolProfile
                {
                    SchoolId = schools[2].Id,
                    Name = "Английски език",
                    Code = "АЕ",
                    Description = "Интензивно изучаване на английски език и култура.",
                    Subjects = "Английски език, Втори език, Български език и литература",
                    AvailablePlaces = 26
                },
                new SchoolProfile
                {
                    SchoolId = schools[2].Id,
                    Name = "Немски език",
                    Code = "НЕ",
                    Description = "Интензивно изучаване на немски език и култура.",
                    Subjects = "Немски език, Втори език, Български език и литература",
                    AvailablePlaces = 26
                }
            };

            context.SchoolProfiles.AddRange(profiles);
            context.SaveChanges();

            // Добавяме историческо класиране
            var rankings = new List<HistoricalRanking>
            {
                new HistoricalRanking
                {
                    SchoolId = schools[0].Id,
                    ProfileId = profiles[0].Id,
                    Year = 2024,
                    MinimumScore = 485.5,
                    Round = 1,
                    StudentsAdmitted = 26
                },
                new HistoricalRanking
                {
                    SchoolId = schools[0].Id,
                    ProfileId = profiles[1].Id,
                    Year = 2024,
                    MinimumScore = 465.25,
                    Round = 1,
                    StudentsAdmitted = 26
                },
                new HistoricalRanking
                {
                    SchoolId = schools[1].Id,
                    ProfileId = profiles[2].Id,
                    Year = 2024,
                    MinimumScore = 430.75,
                    Round = 1,
                    StudentsAdmitted = 26
                },
                new HistoricalRanking
                {
                    SchoolId = schools[2].Id,
                    ProfileId = profiles[3].Id,
                    Year = 2024,
                    MinimumScore = 468.5,
                    Round = 1,
                    StudentsAdmitted = 26
                }
            };

            context.HistoricalRankings.AddRange(rankings);
            context.SaveChanges();

            // Можете да добавите и други данни: отзиви, формули за прием и т.н.
        }
    }
}
