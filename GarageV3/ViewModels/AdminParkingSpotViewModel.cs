using System.ComponentModel.DataAnnotations;

namespace GarageV3.ViewModels
{
    public class AdminParkingSpotViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Spot Number")]
        public int Number { get; set; }

        [MaxLength(100)]
        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Out of Service")]
        public bool IsOutOfService { get; set; }
    }
}
