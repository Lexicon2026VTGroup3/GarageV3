using GarageV3.Models.Entities;

namespace GarageV3.ViewModels
{
    // ViewModel used to display a parking receipt after a vehicle is checked out.
    public class ReceiptViewModel
    {
        public string VehicleTypeName { get; set; } = string.Empty;

        public string RegistrationNumber { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int NumberOfWheels { get; set; }

        public int? AssignedSpotNumber { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime CheckOutTime { get; set; }

        public TimeSpan ParkingDuration => CheckOutTime - ArrivalTime;

        public decimal TotalPrice { get; set; }
    }
}