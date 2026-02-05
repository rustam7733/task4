using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace task4.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class ApplicationUser
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserStatus Status { get; set; } = UserStatus.Unverified;

        public DateTime RegisterTime { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginTime { get; set; }

        public string? EmailConfirmToken { get; set; }
    }

}
