using GarageV3.Models.Enums;

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

        public IReadOnlyDictionary<VehicleType, int> VehicleCountsByType { get; set; }
            = new Dictionary<VehicleType, int>();

        public VehicleType? MostCommonType { get; set; }

        public TimeSpan AverageParkedDuration { get; set; }

        public DateTime? LongestParkedArrivalTime { get; set; }

        public string? LongestParkedRegistrationNumber { get; set; }
    }
}