using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.Models.Entities
{
    [Index(nameof(Number), IsUnique = true)]
    public class ParkingSpot
    {
        public int Id { get; set; }

        public required int Number { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public bool IsOutOfService { get; set; }

        /// <summary>
        /// How many capacity units this physical spot provides. A standard
        /// spot holds enough units for either one car (whole spot) or up to
        /// several motorcycles sharing it.
        /// </summary>
        public int CapacityUnits { get; set; } = 3;
    }
}