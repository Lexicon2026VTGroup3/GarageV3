using GarageV3.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.ViewModels
{
    public class ParkedVehicleFormViewModel
    {
        public int? Id { get; set; } // null for Create, value for Edit

        [Required(ErrorMessage = "Registration Number is required.")]
        [Remote(action: "CheckDuplicate", 
            controller: "ParkedVehicles", 
            AdditionalFields = nameof(Id), 
            ErrorMessage = "This registration number already exists!")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Arrival Time")]
        [DisplayFormat(DataFormatString = "{0:dddd, yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime ArrivalTime { get; set; } // readonly in Edit

        [StringLength(20, ErrorMessage = "Color cannot be longer than 20 characters.")]
        [Display(Name = "Color (Max 20 Character)")]
        public string? Color { get; set; }

        [StringLength(30, ErrorMessage = "Brand cannot be longer than 30 characters.")]
        [Display(Name = "Brand (Max 30 Character)")]
        public string? Brand { get; set; }

        [StringLength(40, ErrorMessage = "Model cannot be longer than 40 characters.")]
        [Display(Name = "Model (Max 40 Character)")]
        public string? Model { get; set; }

        [Required(ErrorMessage = "Number of wheels is required.")]
        [Range(0, 18, ErrorMessage = "Number of wheels must be between 0 and 18.")]
        [Display(Name = "Number of Wheels")]
        public int? NumberOfWheels { get; set; }

        [Required]
        [Display(Name = "Vehicle Type")]
        public VehicleType VehicleType { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem>? VehicleTypes { get; set; }

        [Display(Name = "Parking Spot")]
        public int? AssignedSpotNumber { get; set; }
    }
}
