using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.ViewModels
{
    public class AdminVehicleTypeViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Icon { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string BadgeColor { get; set; } = "#006AA7";

        [Required]
        [MaxLength(20)]
        public string BadgeTextColor { get; set; } = "#ffffff";

        [Required]
        public int RequiredSpots { get; set; } = 1;

        [Required]
        public int MaxVehiclesPerSpot { get; set; } = 1;
    }
}