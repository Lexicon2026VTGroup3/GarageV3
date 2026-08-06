namespace GarageV3.Services.Interfaces
{
    public interface IVehicleHandler
    {
        Task<bool> IsExistingAsync(string regNumber, int? excludeId = null);
    }
}