namespace SchoolSelect.Repositories.Interfaces
{
    // Интерфейс за Unit of Work
    public interface IUnitOfWork : IDisposable
    {
        ISchoolRepository Schools { get; }
        ISchoolProfileRepository SchoolProfiles { get; }
        IHistoricalRankingRepository HistoricalRankings { get; }
        ISchoolFacilityRepository SchoolFacilities { get; }
        IReviewRepository Reviews { get; }
        IUserRepository Users { get; }
        IUserPreferenceRepository UserPreferences { get; }
        IAdmissionFormulaRepository AdmissionFormulas { get; }
        IUserGradesRepository UserGrades { get; }
        IComparisonSetRepository ComparisonSets { get; }
        INotificationRepository Notifications { get; }

        Task<int> CompleteAsync();
    }
}