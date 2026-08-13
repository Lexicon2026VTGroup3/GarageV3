namespace GarageV3.ViewModels.Parking
{
    public class SpotMapViewModel
    {
        public int TotalSpots { get; set; }
        public int FreeSpots { get; set; }
        public List<SpotMapSpotInfo> Spots { get; set; } = new();
    }

    public class SpotMapSpotInfo
    {
        public int SpotNumber { get; set; }
        public string? Location { get; set; }
        public bool IsOutOfService { get; set; }
        public int CapacityUnits { get; set; }
        public int UsedUnits { get; set; }
        public int FreeUnits => CapacityUnits - UsedUnits;
        public List<string> OccupyingRegistrationNumbers { get; set; } = new();

        /// <summary>
        /// Whole = a normal spot on its own. Left/Middle/Right = part of a
        /// contiguous multi-spot allocation for one large vehicle (US12).
        /// </summary>
        public string Position { get; set; } = "Whole";
    }
}