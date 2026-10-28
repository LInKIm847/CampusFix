
using System.ComponentModel.DataAnnotations;

namespace CampusFix.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(60)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // One category can classify many maintenance requests.
        public ICollection<MaintenanceRequest> MaintenanceRequests
        { get; set; } = new List<MaintenanceRequest>();
    }
}
