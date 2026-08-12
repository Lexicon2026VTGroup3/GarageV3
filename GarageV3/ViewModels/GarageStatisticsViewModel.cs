namespace GarageV3.ViewModels
{
    public class GarageStatisticsViewModel
    {
        public int TotalVehicles { get; set; }

        public int TotalWheels { get; set; }

        /// <summary>
        /// What the currently parked vehicles have generated in fees so far,
        /// calculated as if each checked out right now.
        /// </summary>
        public decimal EstimatedCurrentRevenue { get; set; }

        public IReadOnlyDictionary<string, int> VehicleCountsByType { get; set; }
            = new Dictionary<string, int>();

        public string? MostCommonType { get; set; }

        public TimeSpan AverageParkedDuration { get; set; }

        public DateTime? LongestParkedArrivalTime { get; set; }

        public string? LongestParkedRegistrationNumber { get; set; }
    }
}