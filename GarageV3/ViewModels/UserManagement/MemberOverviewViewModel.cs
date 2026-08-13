namespace GarageV3.ViewModels.UserManagement
{
    public class MemberOverviewViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PersonalIdentityNumber { get; set; } = string.Empty;
        public int RegisteredVehiclesCount { get; set; }
        public decimal ActiveParkingTotalCost { get; set; }

    }
}
