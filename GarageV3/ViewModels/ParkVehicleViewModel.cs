using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GarageV3.ViewModels
{
    public class ParkVehicleViewModel
    {
        [Required]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [Display(Name = "Parking Spot")]
        public int ParkingSpotId { get; set; }

        public IEnumerable<SelectListItem>? Vehicles { get; set; }

        public IEnumerable<SelectListItem>? ParkingSpots { get; set; }
    }
}