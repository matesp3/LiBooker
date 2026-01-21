namespace LiBooker.Shared.DTOs
{
    public class PersonUpdate
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime BirthDate { get; set; }
        public char Gender { get; set; }
    }
}