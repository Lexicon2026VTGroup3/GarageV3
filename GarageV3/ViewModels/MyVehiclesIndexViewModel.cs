namespace GarageV3.ViewModels
{
    public class MyVehiclesIndexViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }

        public string VehicleTypeName { get; set; } = string.Empty;
        public string VehicleTypeIcon { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = string.Empty;
        public string BadgeTextColor { get; set; } = string.Empty;
        public int RequiredSpots { get; set; }

        public int? ParkingSpotId { get; set; }

        public int? ActiveParkingSessionId { get; set; }

        public string DisplayVehicleType => $"{VehicleTypeIcon} {VehicleTypeName}";
    }
}
