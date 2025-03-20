using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Web.Data;

namespace SchoolSelect.Repositories
{
    // Имплементация на Unit of Work
    
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Schools = new SchoolRepository(_context);
            SchoolProfiles = new SchoolProfileRepository(_context);
            HistoricalRankings = new HistoricalRankingRepository(_context);
            SchoolFacilities = new SchoolFacilityRepository(_context);
            Reviews = new ReviewRepository(_context);
            Users = new UserRepository(_context);
            UserPreferences = new UserPreferenceRepository(_context);
            AdmissionFormulas = new AdmissionFormulaRepository(_context);
            UserGrades = new UserGradesRepository(_context);
            ComparisonSets = new ComparisonSetRepository(_context);
            Notifications = new NotificationRepository(_context);
        }

        public ISchoolRepository Schools { get; private set; }
        public ISchoolProfileRepository SchoolProfiles { get; private set; }
        public IHistoricalRankingRepository HistoricalRankings { get; private set; }
        public ISchoolFacilityRepository SchoolFacilities { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IUserRepository Users { get; private set; }
        public IUserPreferenceRepository UserPreferences { get; private set; }
        public IAdmissionFormulaRepository AdmissionFormulas { get; private set; }
        public IUserGradesRepository UserGrades { get; private set; }
        public IComparisonSetRepository ComparisonSets { get; private set; }
        public INotificationRepository Notifications { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
