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
        /// Друг компонент
        /// </summary>
        public const int OtherComponent = 4;
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
    }
}