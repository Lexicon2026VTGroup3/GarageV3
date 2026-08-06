namespace GarageV3.Models.Parking
{
    /// <summary>
    /// Result returned when trying to assign parking spot(s) to a vehicle.
    /// </summary>
    public class ParkingAssignmentResult
    {
        public bool Success { get; set; }

        /// <summary>
        /// The spot number(s) assigned to the vehicle.
        /// One entry for normal vehicles, multiple for trucks/airplanes/boats,
        /// a single (shared) spot number for motorcycles.
        /// </summary>
        public List<int> AssignedSpotNumbers { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public static ParkingAssignmentResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };

        public static ParkingAssignmentResult Ok(List<int> spots) =>
            new() { Success = true, AssignedSpotNumbers = spots };
    }
}