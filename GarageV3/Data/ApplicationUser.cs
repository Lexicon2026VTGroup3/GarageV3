using GarageV3.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace GarageV3.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    [Required]
    [StringLength(13)] // "YYYYMMDD-XXXX"
    public string PersonalIdentityNumber { get; set; } = string.Empty;

    public DateTime DateOfBirth => DateTime.ParseExact(PersonalIdentityNumber.Substring(0, 8), "yyyyMMdd", null);
    public MembershipType MembershipType { get; set; } = MembershipType.Standard;
    public DateTime? MembershipStartDate { get; set; }
    public DateTime? MembershipEndDate { get; set; }
    public bool IsProMember =>
        MembershipType == MembershipType.Pro &&
        MembershipEndDate.HasValue &&
        MembershipEndDate.Value > DateTime.UtcNow;
}
