
using CampusFix.Data;
using CampusFix.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Repositories
{
    public class MaintenanceRequestRepository
        : IMaintenanceRequestRepository
    {
        private readonly CampusFixDbContext _context;

        // Dependency injection supplies the database context.
        public MaintenanceRequestRepository(
            CampusFixDbContext context)
        {
            _context = context;
        }

        // Returns all requests, including their user and category.
        public async Task<List<MaintenanceRequest>> GetAllAsync()
        {
            return await _context.MaintenanceRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        // Returns one request by its ID, or null if not found.
        public async Task<MaintenanceRequest?> GetByIdAsync(int id)
        {
            return await _context.MaintenanceRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Returns only requests belonging to a specific user.
        public async Task<List<MaintenanceRequest>> GetByUserIdAsync(
            string userId)
        {
            return await _context.MaintenanceRequests
                .Include(r => r.Category)
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        // Adds a new request to the database.
        public async Task AddAsync(MaintenanceRequest request)
        {
            await _context.MaintenanceRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        // Updates a request that is already being tracked by EF Core.
        public async Task UpdateAsync(MaintenanceRequest request)
        {
            _context.MaintenanceRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        // Deletes a request if it exists.
        public async Task DeleteAsync(int id)
        {
            var request = await _context.MaintenanceRequests
                .FindAsync(id);

            if (request != null)
            {
                _context.MaintenanceRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}
