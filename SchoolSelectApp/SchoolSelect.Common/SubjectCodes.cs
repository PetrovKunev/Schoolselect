namespace SchoolSelect.Common
{
    /// <summary>
    /// Кодове на предмети, използвани във формулите за балообразуване
    /// </summary>
    public static class SubjectCodes
    {
        /// <summary>
        /// Български език и литература
        /// </summary>
        public const string BulgarianLanguage = "БЕЛ";
        /// <summary>
        /// Математика
        /// </summary>
        public const string Mathematics = "МАТ";
        /// <summary>
        /// Чужд език
        /// </summary>
        public const string ForeignLanguage = "ЧЕз";
        /// <summary>
        /// Компютърно моделиране и информационни технологии
        /// </summary>
        public const string InformationTechnology = "КМИТ";
        /// <summary>
        /// Физика
        /// </summary>
        public const string Physics = "ФИЗ";
        /// <summary>
        /// Химия
        /// </summary>
        public const string Chemistry = "ХИМ";
        /// <summary>
        /// Биология
        /// </summary>
        public const string Biology = "БИО";
        /// <summary>
        /// История
        /// </summary>
        public const string History = "ИСТ";
        /// <summary>
        /// География
        /// </summary>
        public const string Geography = "ГЕО";
        /// <summary>
        /// Философия
        /// </summary>
        public const string Philosophy = "ФИЛ";
        /// <summary>
        /// Физическо възпитание и спорт
        /// </summary>
        public const string PhysicalEducation = "ФВС";
        /// <summary>
        /// Английски език
        /// </summary>
        public const string EnglishLanguage = "АЕ";
        /// <summary>
        /// Немски език
        /// </summary>
        public const string GermanLanguage = "НЕ";
        /// <summary>
        /// Френски език
        /// </summary>
        public const string FrenchLanguage = "ФЕ";
        /// <summary>
        /// Испански език
        /// </summary>
        public const string SpanishLanguage = "ИЕ";
        /// <summary>
        /// Италиански език
        /// </summary>
        public const string ItalianLanguage = "ИТ";
        /// <summary>
        /// Руски език
        /// </summary>
        public const string RussianLanguage = "РЕ";
        /// <summary>
        /// Изобразително изкуство
        /// </summary>
        public const string Art = "ИИ";
        /// <summary>
        /// Музика
        /// </summary>
        public const string Music = "МУЗ";
        /// <summary>
        /// Технологии и предприемачество
        /// </summary>
        public const string Technology = "ТП";
        /// <summary>
        /// Човек и природа
        /// </summary>
        public const string NatureStudies = "ЧП";
        /// <summary>
        /// Човек и общество
        /// </summary>
        public const string SocialStudies = "ЧО";

        public const string NPMGCompetitions = "НПМГ-СЪСТЕЗ";

        /// <summary>
        /// Речник с пълните имена на предметите
        /// </summary>
        public static readonly Dictionary<string, string> SubjectNames = new Dictionary<string, string>
    {
        { BulgarianLanguage, "Български език и литература" },
        { Mathematics, "Математика" },
        { ForeignLanguage, "Чужд език" },
        { InformationTechnology, "Компютърно моделиране и ИТ" },
        { Physics, "Физика и астрономия" },
        { Chemistry, "Химия и опазване на околната среда" },
        { Biology, "Биология и здравно образование" },
        { History, "История и цивилизации" },
        { Geography, "География и икономика" },
        { Philosophy, "Философия" },
        { PhysicalEducation, "Физическо възпитание и спорт" },
        { EnglishLanguage, "Английски език" },
        { GermanLanguage, "Немски език" },
        { FrenchLanguage, "Френски език" },
        { SpanishLanguage, "Испански език" },
        { ItalianLanguage, "Италиански език" },
        { RussianLanguage, "Руски език" },
        { Art, "Изобразително изкуство" },
        { Music, "Музика" },
        { Technology, "Технологии и предприемачество" },
        { NatureStudies, "Човек и природа" },
        { SocialStudies, "Човек и общество" },
        { NPMGCompetitions, "Състезания и олимпиади (НПМГ)" }
    };

        /// <summary>
        /// Проверява дали даден код на предмет е валиден
        /// </summary>
        /// <param name="code">Код на предмет</param>
        /// <returns>true, ако кодът е валиден; false в противен случай</returns>
        public static bool IsValidSubjectCode(string code)
        {
            return SubjectNames.ContainsKey(code);
        }

        /// <summary>
        /// Връща пълното име на предмет по код
        /// </summary>
        /// <param name="code">Код на предмет</param>
        /// <returns>Пълното име на предмета или празен низ, ако кодът е невалиден</returns>
        public static string GetSubjectName(string code)
        {
            return SubjectNames.TryGetValue(code, out string? name) ? name : string.Empty;
        }
    }

    /// <summary>
    /// Профили в средните училища
    /// </summary>
    public static class ProfileTypes
    {
        /// <summary>
        /// Профил Математически
        /// </summary>
        public const string Mathematics = "Математически";

        /// <summary>
        /// Профил Природни науки
        /// </summary>
        public const string NaturalSciences = "Природни науки";

        /// <summary>
        /// Профил Хуманитарни науки
        /// </summary>
        public const string Humanities = "Хуманитарни науки";

        /// <summary>
        /// Профил Чужди езици
        /// </summary>
        public const string ForeignLanguages = "Чужди езици";

        /// <summary>
        /// Профил Софтуерни и хардуерни науки
        /// </summary>
        public const string ComputerSciences = "Софтуерни и хардуерни науки";

        /// <summary>
        /// Профил Предприемачески
        /// </summary>
        public const string Entrepreneurship = "Предприемачески";

        /// <summary>
        /// Профил Изкуства
        /// </summary>
        public const string Arts = "Изкуства";

        /// <summary>
        /// Профил Спорт
        /// </summary>
        public const string Sports = "Спорт";

        /// <summary>
        /// Речник с кратките обозначения на профилите
        /// </summary>
        public static readonly Dictionary<string, string> ProfileShortNames = new Dictionary<string, string>
        {
            { Mathematics, "МАТ" },
            { NaturalSciences, "ПН" },
            { Humanities, "ХН" },
            { ForeignLanguages, "ЧЕ" },
            { ComputerSciences, "СТЕМ" },
            { Entrepreneurship, "ПРЕП" },
            { Arts, "ИЗК" },
            { Sports, "СПОРТ" }
        };
    }
}