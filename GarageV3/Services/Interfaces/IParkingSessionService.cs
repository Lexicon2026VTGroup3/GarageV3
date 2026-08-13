using GarageV3.Models.Entities;

namespace GarageV3.Services.Interfaces
{
    public interface IParkingSessionService
    {
        Task<ParkingSession> StartSessionAsync(int parkingSpotId, int vehicleId);
        Task<ParkingSession?> CompleteSessionAsync(int sessionId);
        Task<ParkingSession?> GetActiveSessionForSpotAsync(int parkingSpotId);
    }
}