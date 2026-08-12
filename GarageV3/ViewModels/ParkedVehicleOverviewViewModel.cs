namespace GarageV3.ViewModels
{
    public class ParkedVehicleOverviewViewModel
    {
        public int Id { get; set; }

        // Human-readable vehicle type (e.g. "Car", "Motorcycle")
        public string VehicleTypeName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public DateTime ArrivalTime { get; set; }

        public int? AssignedSpotNumber { get; set; }
    }
}