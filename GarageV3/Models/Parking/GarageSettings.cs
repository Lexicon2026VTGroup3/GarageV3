namespace GarageV3.Models.Parking
{
    /// <summary>
    /// Strongly-typed configuration for garage-wide parking settings,
    /// bound from the "GarageSettings" section in appsettings.json.
    /// </summary>
    public class GarageSettings
    {
        public const string SectionName = "GarageSettings";

        public int TotalParkingSpots { get; set; }
    }
}