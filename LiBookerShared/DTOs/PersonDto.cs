namespace LiBooker.Shared.DTOs
{
    public sealed record PersonDto
    {
        public int Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public DateTime BirthDate { get; init; }
        public DateTime RegisteredAt { get; init; }
        public required string Email { get; init; }
        public char Gender { get; init; }
        public string? Phone { get; init; }
        public int ReservationCount { get; init; }
        public int LoanCount { get; init; }
    }
}