using GarageV3.Models.Parking;

namespace GarageV3.ViewModels
{
    public class ParkingOverviewViewModel
    {
        public int TotalSpots { get; set; }

        public int FreeSpotCount { get; set; }

        public IReadOnlyList<ParkingSpotInfo> Spots { get; set; } = new List<ParkingSpotInfo>();
    }
}