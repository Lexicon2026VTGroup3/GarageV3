namespace GarageV3.Models.ViewModels
{
    public class CheckOutViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; }
    }
}
