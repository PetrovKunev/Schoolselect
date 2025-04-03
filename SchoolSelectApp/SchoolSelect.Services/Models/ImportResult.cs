using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolSelect.Services.Models
{
    /// <summary>
    /// Резултат от импортирането
    /// </summary>
    public class ImportResult
    {
        public bool IsSuccess { get; set; }
        public int SuccessCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
