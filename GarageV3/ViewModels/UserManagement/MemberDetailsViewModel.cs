namespace GarageV3.ViewModels.UserManagement
{
    public class MemberDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PersonalIdentityNumber { get; set; } = string.Empty;
        public List<MemberVehicleViewModel> Vehicles { get; set; } = new();
    }
}
