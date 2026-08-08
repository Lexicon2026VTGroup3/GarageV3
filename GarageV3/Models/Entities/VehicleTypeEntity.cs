using GarageV3.Models.Enums;
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

        /// <summary>
        /// Maps this database row to the legacy enum value, so existing
        /// business logic (spot allocation rules, display names) keeps working
        /// unchanged while the data itself is now relational.
        /// </summary>
        public VehicleType EnumValue { get; set; }
    }
}