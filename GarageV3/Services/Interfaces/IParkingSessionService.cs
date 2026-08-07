using GarageV3.Models.Entities;

namespace GarageV3.Services.Interfaces
{
    public interface IParkingSessionService
    {
        Task<ParkingSession> StartSessionAsync(int parkingSpotNumber, int parkedVehicleId);
        Task<ParkingSession?> CompleteSessionAsync(int sessionId);
        Task<ParkingSession?> GetActiveSessionForSpotAsync(int parkingSpotNumber);
    }
}