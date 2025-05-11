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
        /// Брой точки от Олимпиада по физика – областен кръг, над 60% от максимално постигнатите точки или Национално състезание по Природни науки и география „Акад. Л. Чакалов" – модул Талантлив физик
        /// </summary>
        public const int PhysicsOlympiad60Percent = 4;

        /// <summary>
        /// Брой точки от Олимпиада по биология – областен кръг, над 60% от максимално постигнатите точки или Национално състезание по Природни науки и география „Акад. Л. Чакалов" – модул Талантлив биолог
        /// </summary>
        public const int BiologyOlympiad60Percent = 5;

        /// <summary>
        /// Брой точки от Олимпиада по химия – областен кръг или Национално състезание, над 60% от максимално постигнатите точки или Национално състезание по Природни науки и география „Акад. Л. Чакалов" – модул Талантлив химик
        /// </summary>
        public const int ChemistryOlympiad60Percent = 6;

        /// <summary>
        /// Национално състезание по Природни науки и география „Акад. Л. Чакалов" – модул Талантлив биолог
        /// </summary>
        public const int ChakalovTalentedBiologist = 7;

        /// <summary>
        /// Национално състезание по Природни науки и география „Акад. Л. Чакалов" – модул Талантлив химик
        /// </summary>
        public const int ChakalovTalentedChemist = 8;

        /// <summary>
        /// Брой точки от олимпиадата по география – областен кръг, над 60% от максимално постигнатите точки или Национално състезание по Природни науки и география „Акад. Л. Чакалов"– модул Талантлив географ
        /// </summary>
        public const int GeographyOlympiad60Percent = 9;
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