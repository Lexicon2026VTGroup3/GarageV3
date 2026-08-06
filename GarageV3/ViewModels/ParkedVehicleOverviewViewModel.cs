namespace GarageV3.ViewModels
{
    public class ParkedVehicleOverviewViewModel
    {
        public int Id { get; set; }
        public Models.Enums.VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }

        public int? AssignedSpotNumber { get; set; }
    }
}