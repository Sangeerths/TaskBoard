using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class UserProfile
    {
            [Key]
            public Guid ProfileId { get; set; }

            [Required]
            public Guid UserId { get; set; }

            [Required]
            [MaxLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [MaxLength(100)]
            public string? LastName { get; set; }

            [MaxLength(20)]
            public string? PhoneNumber { get; set; }

            public DateTime? DateOfBirth { get; set; }

            [MaxLength(20)]
            public string? Gender { get; set; }

            public string? Address { get; set; }

            [MaxLength(100)]
            public string? City { get; set; }

            [MaxLength(100)]
            public string? State { get; set; }

            [MaxLength(100)]
            public string? Country { get; set; }

            [MaxLength(20)]
            public string? PostalCode { get; set; }

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

            // Navigation property
            public User User { get; set; } = null!;


    }
}
