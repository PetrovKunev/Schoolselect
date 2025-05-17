namespace SchoolSelect.Services.Helpers
{
   
    public static class GeoHelper
    {
        // Радиус на Земята в километри
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Изчислява разстоянието между две точки по формулата на Хаверсин
        /// </summary>
        /// <param name="lat1">Ширина на първата точка</param>
        /// <param name="lon1">Дължина на първата точка</param>
        /// <param name="lat2">Ширина на втората точка</param>
        /// <param name="lon2">Дължина на втората точка</param>
        /// <returns>Разстояние в километри</returns>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Проверка за валидни координати
            if (Math.Abs(lat1) > 90 || Math.Abs(lat2) > 90 || Math.Abs(lon1) > 180 || Math.Abs(lon2) > 180)
            {
                // Връщаме голямо разстояние при невалидни координати
                return 1000.0;
            }

            // Преобразуване от градуси в радиани
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            // Формула на Хаверсин
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = EarthRadiusKm * c;

            return distance;
        }

        /// <summary>
        /// Проверява дали дадена точка е в определен радиус от друга точка
        /// </summary>
        /// <param name="centerLat">Ширина на централната точка</param>
        /// <param name="centerLon">Дължина на централната точка</param>
        /// <param name="pointLat">Ширина на проверяваната точка</param>
        /// <param name="pointLon">Дължина на проверяваната точка</param>
        /// <param name="radiusKm">Радиус в километри</param>
        /// <returns>true ако точката е в радиуса, false иначе</returns>
        public static bool IsPointInRadius(double centerLat, double centerLon,
                                          double pointLat, double pointLon,
                                          double radiusKm)
        {
            var distance = CalculateDistance(centerLat, centerLon, pointLat, pointLon);
            return distance <= radiusKm;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
