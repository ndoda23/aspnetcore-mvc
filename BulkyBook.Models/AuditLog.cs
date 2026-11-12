using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }          

        public string? UserEmail { get; set; }     

        [Required]
        [MaxLength(50)]
        public string Action { get; set; }          

        [Required]
        [MaxLength(50)]
        public string EntityName { get; set; }      // "Order", "User", "Product"...

        public string? EntityId { get; set; }      

        public string? OldValue { get; set; }       
        public string? NewValue { get; set; }       

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}