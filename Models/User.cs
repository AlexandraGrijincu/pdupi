using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Gym.Models
{
    public enum UserRole { Member, Staff }

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public string? AccessToken { get; set; }

        // 🌟 String-ul lung Base64 extras din galeria Expo a telefonului
        public string? ProfileImage { get; set; }

        // 🌟 Câmpurile noi pentru managementul dinamic al abonamentelor direct pe User
        public string? SubscriptionType { get; set; }
        public int? SessionsLeftThisWeek { get; set; }

        [JsonIgnore]
        public virtual ICollection<Subscription>? Subscriptions { get; set; } = new List<Subscription>();

      
    }
}