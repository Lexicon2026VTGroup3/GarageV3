using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.ViewModels
{
    public class AdminVehicleCreateViewModel : ParkedVehicleFormViewModel
    {
        [Required(ErrorMessage = "Owner is required.")]
        [Display(Name = "Vehicle Owner")]
        public string OwnerId { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? Users { get; set; }
    }
}
