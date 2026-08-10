using GarageV3.Models.Entities;
using GarageV3.Models.Enums;
using VehicleType = GarageV3.Models.Enums.VehicleType;

namespace GarageV3.ViewModels.Parking
{
    // ViewModel used to display a parking receipt after a vehicle is checked out.
    public class ReceiptViewModel
    {
        public string OwnerEmail { get; set; } = string.Empty;

        public VehicleType VehicleType { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int NumberOfWheels { get; set; }
        public int? AssignedSpotNumber { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime CheckOutTime { get; set; }

        //public TimeSpan ParkingDuration { get; set; }
        public TimeSpan ParkingDuration => CheckOutTime - ArrivalTime;

        public decimal HourlyRateAtCheckIn { get; set; }

        public decimal TotalPrice { get; set; }
    }
}