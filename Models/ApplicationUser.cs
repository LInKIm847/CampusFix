
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models
{
    // Extends ASP.NET Core Identity's user account.
    // Identity manages password hashing, login and account security.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        // Navigation property: one user can submit many requests.
        public ICollection<MaintenanceRequest> MaintenanceRequests
        { get; set; } = new List<MaintenanceRequest>();
    }
}
