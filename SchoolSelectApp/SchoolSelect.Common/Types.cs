namespace SchoolSelect.Common
{
    /// <summary>
    /// Типове компоненти във формулата за балообразуване
    /// </summary>
    public static class ComponentTypes
    {
        /// <summary>
        /// Годишна оценка от училище
        /// </summary>
        public const int YearlyGrade = 1;

        /// <summary>
        /// Точки от Национално външно оценяване (НВО)
        /// </summary>
        public const int NationalExam = 2;

        /// <summary>
        /// Точки от приемен изпит (конкурсен)
        /// </summary>
        public const int EntranceExam = 3;

        /// <summary>
        /// Годишна оценка, преобразувана в точки
        /// </summary>
        public const int YearlyGradeAsPoints = 4;
    }

    /// <summary>
    /// Типове нотификации в системата
    /// </summary>
    public static class NotificationTypes
    {
        /// <summary>
        /// Нотификация за промяна в училище/профил
        /// </summary>
        public const int SchoolChange = 1;

        /// <summary>
        /// Нотификация за ново класиране
        /// </summary>
        public const int NewRanking = 2;

        /// <summary>
        /// Системно съобщение
        /// </summary>
        public const int SystemMessage = 3;
    }

    /// <summary>
    /// Типове съоръжения в училищата
    /// </summary>
    public static class FacilityTypes
    {
        /// <summary>
        /// Спортна база
        /// </summary>
        public const string Sports = "Sports";

        /// <summary>
        /// Лаборатория
        /// </summary>
        public const string Laboratory = "Laboratory";

        /// <summary>
        /// Клуб по интереси
        /// </summary>
        public const string Club = "Club";

        /// <summary>
        /// Библиотека
        /// </summary>
        public const string Library = "Library";

        /// <summary>
        /// Столова
        /// </summary>
        public const string Cafeteria = "Cafeteria";

        /// <summary>
        /// Общежитие
        /// </summary>
        public const string Dormitory = "Dormitory";

        /// <summary>
        /// Транспортна връзка
        /// </summary>
        public const string Transport = "Transport";
    }

    /// <summary>
    /// Видове транспорт
    /// </summary>
    public static class TransportModes
    {
        /// <summary>
        /// Метро
        /// </summary>
        public const string Metro = "Metro";

        /// <summary>
        /// Автобус
        /// </summary>
        public const string Bus = "Bus";

        /// <summary>
        /// Трамвай
        /// </summary>
        public const string Tram = "Tram";

        /// <summary>
        /// Тролейбус
        /// </summary>
        public const string Trolley = "Trolley";

        /// <summary>
        /// Влак
        /// </summary>
        public const string Train = "Train";
    }
}