using SchoolSelect.Data.Models;

namespace SchoolSelect.Web.Areas.Admin.ViewModels
{
    public class HistoricalRankingsViewModel
    {
        public required School School { get; set; }
        public List<HistoricalRanking> Rankings { get; set; } = new List<HistoricalRanking>();
        public List<SchoolProfile> Profiles { get; set; } = new List<SchoolProfile>();
    }

}
