using System.ComponentModel.DataAnnotations;
using SchoolSelect.Common;

namespace SchoolSelect.Data.Models
{
    /// <summary>
    /// Информация за транспортни връзки около училището
    /// </summary>
    public class TransportFacility : SchoolFacility
    {
        public TransportFacility()
        {
            // Задаване на типа по подразбиране
            FacilityType = FacilityTypes.Transport;
        }

        /// <summary>
        /// Вид транспорт (Метро, Автобус, Трамвай, Тролей)
        /// </summary>
        [StringLength(ValidationConstants.TransportFacility.TransportModeMaxLength)]
        public string TransportMode { get; set; } = string.Empty;

        /// <summary>
        /// Номер/име на линията (М1, 280, 6)
        /// </summary>
        [StringLength(ValidationConstants.TransportFacility.LineNumberMaxLength)]
        public string LineNumber { get; set; } = string.Empty;

        /// <summary>
        /// Разстояние до спирката в метри
        /// </summary>
        [Range(0, 2000)]
        public int DistanceToStop { get; set; }
    }
}