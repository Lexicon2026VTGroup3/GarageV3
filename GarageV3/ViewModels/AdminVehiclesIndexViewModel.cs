using GarageV3.Models.Enums;

namespace GarageV3.ViewModels
{
    public class AdminVehiclesIndexViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }

        public VehicleType VehicleType { get; set; }

        public string VehicleTypeName { get; set; } = string.Empty;

        public int? AssignedSpotNumber { get; set; }

        public string OwnerEmail { get; set; } = string.Empty;
    }
}
