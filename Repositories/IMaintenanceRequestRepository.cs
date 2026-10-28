
using CampusFix.Models;

namespace CampusFix.Repositories
{
    public interface IMaintenanceRequestRepository
    {
        Task<List<MaintenanceRequest>> GetAllAsync();

        Task<MaintenanceRequest?> GetByIdAsync(int id);

        Task<List<MaintenanceRequest>> GetByUserIdAsync(
            string userId);

        Task AddAsync(MaintenanceRequest request);

        Task UpdateAsync(MaintenanceRequest request);

        Task DeleteAsync(int id);
    }
}
