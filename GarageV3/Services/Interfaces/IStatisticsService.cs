using GarageV3.ViewModels;

namespace GarageV3.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<GarageStatisticsViewModel> GetGarageStatisticsAsync();
    }
}