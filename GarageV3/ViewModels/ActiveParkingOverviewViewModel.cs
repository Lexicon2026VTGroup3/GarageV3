namespace GarageV3.ViewModels
{
    public class ActiveParkingOverviewViewModel
    {
        public int ParkingSessionId { get; set; }

        public string OwnerName { get; set; } = string.Empty;

        public string VehicleTypeName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public int ParkingSpotNumber { get; set; }

        public string Location { get; set; } = string.Empty;

        public DateTime CheckInTime { get; set; }

        public int ParkingDurationMinutes { get; set; }
    }
}