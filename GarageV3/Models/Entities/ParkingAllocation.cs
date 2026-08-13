namespace GarageV3.Models.Entities
{
    public class ParkingAllocation
    {
        public int Id { get; set; }

        public required int ParkingSessionId { get; set; }
        public ParkingSession? ParkingSession { get; set; }

        public required int ParkingSpotId { get; set; }
        public ParkingSpot? ParkingSpot { get; set; }

        /// <summary>
        /// How many of the spot's CapacityUnits this allocation consumes.
        /// A car takes the whole spot's capacity; a motorcycle takes a
        /// fraction, allowing several motorcycles to share one spot.
        /// </summary>
        public required int UnitsUsed { get; set; }
    }
}