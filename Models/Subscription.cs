using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym.Models
{
    public class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        // Navigation Property pentru OData Expand
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public int RemainingSessions { get; set; }

        [NotMapped]
        public bool IsActive => ExpiryDate > DateTime.UtcNow && RemainingSessions > 0;

      
    }
}