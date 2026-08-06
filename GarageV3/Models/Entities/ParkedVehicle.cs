using GarageV3.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.Models.Entities
{
    [Index(nameof(RegistrationNumber), IsUnique = true)]
    public class ParkedVehicle
    {
        public int Id { get; set; }
        public VehicleType VehicleType { get; set; }
        public string RegistrationNumber { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// The starting parking spot number assigned to this vehicle (del 2).
        /// For trucks/airplanes/boats this is the first of several contiguous spots.
        /// Null until a spot has been assigned.
        /// </summary>
        public int? AssignedSpotNumber { get; set; }
    }
}