using GarageV3.Models.Entities;
using GarageV3.Services;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.ViewModels.Parking
{
    public class CheckOutViewModel
    {
        public string Title = "Check Out Parked Vehicle";

        // ParkingSession
        public int ParkingSessionId { get; set; } 

        // Owner
        [Display(Name = "Owner Email")]
        public string OwnerEmail { get; set; } = string.Empty;

        // Vehicle
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Wheels")]
        public int NumberOfWheels { get; set; } = 0;

        // Vehicle Type
        [Required]
        public VehicleTypeEntity? VehicleType { get; set; }

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        // Parking Spot
        [Display(Name = "Parking Spot")]
        public int ParkingSpotId { get; set; }

        // Parking Session
        [Display(Name = "Check-In Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
        [Display(Name = "Check-Out Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? CheckOutTime { get; set; }
        public bool isActive => CheckOutTime == null;

        public TimeSpan TotalDuration
        {
            get
            {
                var start = DateTime.SpecifyKind(CheckInTime, DateTimeKind.Utc);
                var end = CheckOutTime.HasValue
                    ? DateTime.SpecifyKind(CheckOutTime.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow;

                return end - start;
            }
        }

        public string FormattedDuration =>
            $"{TotalDuration.Days}d {TotalDuration.Hours}h {TotalDuration.Minutes}m";

        [Display(Name = "Hourly Rate")]
        [DataType(DataType.Currency)]
        public decimal HourlyRateAtCheckIn { get; set; }

        // Estimated Total Price
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; } = 0.00m;

        public bool IsProMember { get; set; } = false;
        public decimal AppliedDiscountPercentage { get; set; } = 0.00m;
    }
}
