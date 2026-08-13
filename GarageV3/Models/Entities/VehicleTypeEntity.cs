using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.Models.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class VehicleTypeEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(10)]
        public required string ShortName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Icon { get; set; }

        [Required]
        [MaxLength(20)]
        public required string BadgeColor { get; set; }

        [Required]
        [MaxLength(20)]
        public required string BadgeTextColor { get; set; }

        [Required]
        public int RequiredSpots { get; set; } = 1;

        [Required]
        public int MaxVehiclesPerSpot { get; set; } = 1;

        /// <summary>
        /// How many capacity units of a single ParkingSpot this vehicle type
        /// consumes (US12). If this exceeds one spot's CapacityUnits, the
        /// vehicle needs multiple contiguous spots.
        /// </summary>
        [Required]
        public int RequiredSpaceUnits { get; set; } = 3;
    }
}