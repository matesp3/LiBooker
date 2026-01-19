using System.ComponentModel.DataAnnotations;

namespace LiBooker.Shared.DTOs
{
    public class PersonRegistration
    {
        // Identity User info
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Person info (for table 'OSOBA')
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime BirthDate { get; set; }
        public string? Phone { get; set; }
        public char Gender { get; set; } = 'N'; //  M/F
    }
}