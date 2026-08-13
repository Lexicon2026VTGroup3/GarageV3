using GarageV3.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GarageV3.Models.Entities
{
    [Index(nameof(RegistrationNumber), IsUnique = true)]
    public class Vehicle
    {
        public int Id { get; set; }

        public required int VehicleTypeRefId { get; set; }
        public VehicleTypeEntity? VehicleTypeRef { get; set; }

        public required string OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }

        public required string RegistrationNumber { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int NumberOfWheels { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}