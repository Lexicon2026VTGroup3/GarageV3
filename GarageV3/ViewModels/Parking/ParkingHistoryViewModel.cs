namespace GarageV3.ViewModels.Parking;

public class ParkingHistoryViewModel
{
    public int SessionId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string VehicleTypeName { get; set; } = string.Empty;
    public int ParkingSpotId { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime CheckOutTime { get; set; }
    public TimeSpan ParkedDuration => CheckOutTime - ArrivalTime;
    public decimal HourlyRateAtCheckIn { get; set; }
    public decimal TotalPrice { get; set; }
}
