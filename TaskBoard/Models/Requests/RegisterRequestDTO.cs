using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class RegisterRequestDTO
    {
        [Required]
        [EmailAddress] 
        public string Email { get; set; } = string.Empty; 

        [Required]
        [MinLength(6)] 
        public string Password { get; set; } = string.Empty; 
        
        [Required]
        [MaxLength(100)] 
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; }

        [Phone]
        [MaxLength(20)] 
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; }
        public string Address { get; set; }
        
        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string State { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(20)] 
        public string? PostalCode { get; set; }
    }
}
