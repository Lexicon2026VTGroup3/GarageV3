using GarageV3.Models.Enums;

namespace GarageV3.Models.Entities
{
    public class ParkingSession
    {
        public int Id { get; set; }

        public required int ParkingSpotNumber { get; set; }

        public required int ParkedVehicleId { get; set; }
        public ParkedVehicle ParkedVehicle { get; set; } = null!;

        public ParkingSessionStatus Status { get; set; }

        public DateTime ArriveTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
    }
}