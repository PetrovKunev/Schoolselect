namespace SchoolSelect.Common
{
    /// <summary>
    /// Константи за валидация в приложението
    /// </summary>
    public static class ValidationConstants
    {
        public static class Common
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 2000;
            public const int EmailMaxLength = 100;
            public const int PhoneMaxLength = 20;
            public const int UrlMaxLength = 255;

            public const string DefaultCreator = "System";
            public const string DefaultUpdater = "System";
        }

        public static class School
        {
            public const int NameMaxLength = 200;
            public const int AddressMaxLength = 500;
            public const int DistrictMaxLength = 100;
            public const int CityMaxLength = 100;
            public const int MapsFormattedAddressMaxLength = 500;
        }

        public static class SchoolProfile
        {
            public const int NameMaxLength = 200;
            public const int CodeMaxLength = 20;
            public const int SubjectsMaxLength = 500;
            public const int MaxPlaces = 1000;

            // Типове профили
            public const string MathematicsProfile = "Математически";
            public const string ScienceProfile = "Природни науки";
            public const string HumanitiesProfile = "Хуманитарни науки";
            public const string LanguagesProfile = "Чужди езици";
            public const string TechnologyProfile = "Софтуерни и хардуерни науки";
            public const string EntrepreneurshipProfile = "Предприемачески";
            public const string ArtsProfile = "Изкуства";
            public const string SportsProfile = "Спорт";
        }

        public static class SchoolFacility
        {
            public const int NameMaxLength = 200;
            public const int TypeMaxLength = 50;
        }

        public static class Review
        {
            public const int ContentMaxLength = 2000;
        }

        public static class GeoCoordinates
        {
            public const double MinLatitude = -90;
            public const double MaxLatitude = 90;
            public const double MinLongitude = -180;
            public const double MaxLongitude = 180;
        }

        public static class Rating
        {
            public const int MinValue = 1;
            public const int MaxValue = 5;
        }

        public static class Year
        {
            public const int Min = 2000;
            public const int Max = 2100;
        }

        public static class Score
        {
            public const double Min = 0;
            public const double Max = 500;
        }

        public static class Ranking
        {
            public const int MaxRound = 5;
        }

        public static class Grade
        {
            public const double Min = 2;
            public const double Max = 6;
        }

        public static class ExamPoints
        {
            public const double Min = 0;
            public const double Max = 100;
        }

        public static class AdmissionFormula
        {
            public const int ExpressionMaxLength = 500;
            public const int DescriptionMaxLength = 1000;
        }

        public static class FormulaComponent
        {
            public const int SubjectCodeMaxLength = 50;
            public const int SubjectNameMaxLength = 200;
            public const int DescriptionMaxLength = 500;
            public const double MinMultiplier = 0;
            public const double MaxMultiplier = 10;
        }

        public static class UserGrades
        {
            public const int ConfigurationNameMaxLength = 100;
        }

        public static class UserPreference
        {
            public const int NameMaxLength = 100;
            public const int CriteriaWeightsMaxLength = 2000;
            public const int PreferredProfilesMaxLength = 500;
        }

        public static class Comparison
        {
            public const int NameMaxLength = 100;
            public const int MaxItems = 10; // Максимален брой училища за сравнение
        }

        public static class Notification
        {
            public const int TitleMaxLength = 200;
            public const int ContentMaxLength = 1000;

            // Типове известия
            public const int SchoolChange = 1;
            public const int NewRanking = 2;
            public const int SystemMessage = 3;

            // Други константи
            public const bool DefaultIsRead = false;
            public const int DefaultReferenceId = 0;
        }
    }
}