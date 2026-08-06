namespace GarageV3.Models.ViewModels
{
    public class ParkedVehicleOverviewViewModel
    {
        public int Id { get; set; }
        public GarageV3.Models.Enums.VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }

        public int? AssignedSpotNumber { get; set; }
    }
}