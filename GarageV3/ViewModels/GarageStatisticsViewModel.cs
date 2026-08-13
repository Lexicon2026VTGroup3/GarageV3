namespace GarageV3.ViewModels
{
    public class GarageStatisticsViewModel
    {
        public int FreeSpots { get; set; }
        public int OccupiedSpots { get; set; }
        public int OutOfServiceSpots { get; set; }

        /// <summary>
        /// Number of currently active vehicles, grouped by vehicle type name.
        /// </summary>
        public List<VehicleTypeCount> ActiveVehiclesByType { get; set; }

        public List<TopUserViewModel> TopUsers { get; set; } = new();
    }

    public class VehicleTypeCount
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}