
using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public RequestStatus Status { get; set; }
            = RequestStatus.Submitted;

        public RequestPriority Priority { get; set; }
            = RequestPriority.Medium;

        // Foreign key linking the request to its student/user.
        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        // Foreign key linking the request to its category.
        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
