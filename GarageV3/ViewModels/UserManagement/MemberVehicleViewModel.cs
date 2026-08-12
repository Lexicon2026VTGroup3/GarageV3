namespace GarageV3.ViewModels.UserManagement
{
    public class MemberVehicleViewModel
    {
        public int Id { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }

        public DateTime ArrivalTime { get; set; }
        public int? ParkingSpotNumber { get; set; }
    }
}
