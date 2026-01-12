namespace LiBooker.Shared.DTOs
{
    public sealed record PersonDto
    {
        public int Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public DateTime? BirthDate { get; init; }
        public DateTime? RegisteredAt { get; init; }
        public string? Email { get; init; }
        public string? Gender { get; init; }
        public string? Phone { get; init; }
        public int ReservationCount { get; init; }
        public int LoanCount { get; init; }
    }
}