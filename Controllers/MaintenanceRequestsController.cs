using System.Security.Claims;
using CampusFix.Data;
using CampusFix.Models;
using CampusFix.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Controllers
{
    [Authorize]
    public class MaintenanceRequestsController : Controller
    {
        private readonly IMaintenanceRequestRepository _repository;
        private readonly CampusFixDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceRequestsController(
            IMaintenanceRequestRepository repository,
            CampusFixDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _context = context;
            _userManager = userManager;
        }

        // A staff-only URL that can be used to demonstrate role-based denial.
        [Authorize(Roles = "Maintenance,Administrator")]
        public async Task<IActionResult> MaintenanceDashboard()
        {
            return View("Index", await _repository.GetAllAsync());
        }

        // Students see their own requests. Maintenance/Admin see all requests.
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Maintenance") || User.IsInRole("Administrator"))
                return View(await _repository.GetAllAsync());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Challenge();

            return View(await _repository.GetByUserIdAsync(userId));
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();
            if (!CanViewRequest(request)) return Forbid();
            return View(request);
        }

        [Authorize(Roles = "Student,Administrator")]
        public async Task<IActionResult> Create()
        {
            await PopulateCategoriesAsync();
            return View(new MaintenanceRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student,Administrator")]
        public async Task<IActionResult> Create(MaintenanceRequest request)
        {
            // These values are controlled by the server, not the submitted form.
            ModelState.Remove(nameof(request.UserId));
            ModelState.Remove(nameof(request.User));
            ModelState.Remove(nameof(request.Status));
            ModelState.Remove(nameof(request.Priority));
            ModelState.Remove(nameof(request.DateCreated));

            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync();
                return View(request);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            request.UserId = user.Id;
            request.User = null;
            request.DateCreated = DateTime.UtcNow;
            request.Status = RequestStatus.Submitted;
            if (!Enum.IsDefined(typeof(RequestPriority), request.Priority))
                request.Priority = RequestPriority.Medium;

            await _repository.AddAsync(request);
            TempData["StatusMessage"] = $"Request '{request.Title}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();
            if (!CanViewRequest(request)) return Forbid();

            await PopulateCategoriesAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRequest request)
        {
            if (id != request.Id) return NotFound();

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!CanViewRequest(existing)) return Forbid();

            // A student may edit request details, but never its status or owner.
            ModelState.Remove(nameof(request.UserId));
            ModelState.Remove(nameof(request.User));
            ModelState.Remove(nameof(request.Status));
            ModelState.Remove(nameof(request.DateCreated));

            if (!ModelState.IsValid)
            {
                request.Status = existing.Status;
                await PopulateCategoriesAsync();
                return View(request);
            }

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.Location = request.Location;
            existing.Priority = request.Priority;
            existing.CategoryId = request.CategoryId;

            await _repository.UpdateAsync(existing);
            TempData["StatusMessage"] = "The maintenance request was updated.";
            return RedirectToAction(nameof(Index));
        }

        // Only Maintenance staff or Administrators can change request status.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Maintenance,Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
        {
            if (!Enum.IsDefined(typeof(RequestStatus), status))
                return BadRequest("Invalid request status.");

            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await _repository.UpdateAsync(request);
            TempData["StatusMessage"] = $"Request #{request.Id} status changed to {request.Status}.";
            return RedirectToAction(nameof(Details), new { id = request.Id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();
            if (!CanViewRequest(request)) return Forbid();
            return View(request);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _repository.GetByIdAsync(id);
            if (request == null) return NotFound();
            if (!CanViewRequest(request)) return Forbid();

            // Students may delete only their own requests while still Submitted.
            if (!User.IsInRole("Maintenance") && !User.IsInRole("Administrator") &&
                request.Status != RequestStatus.Submitted)
                return Forbid();

            await _repository.DeleteAsync(id);
            TempData["StatusMessage"] = "The maintenance request was deleted.";
            return RedirectToAction(nameof(Index));
        }

        private bool CanViewRequest(MaintenanceRequest request)
        {
            if (User.IsInRole("Maintenance") || User.IsInRole("Administrator"))
                return true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(userId) && request.UserId == userId;
        }

        private async Task PopulateCategoriesAsync()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name");
        }
    }
}
