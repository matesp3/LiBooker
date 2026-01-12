namespace LiBooker.Shared.DTOs
{
    public sealed record ReservationDto
    {
        public int Id { get; init; }
        public int PersonId { get; init; }
        public int ItemId { get; init; }
        public DateTime ReservedAt { get; init; }
        public DateTime? ExpiresAt { get; init; }
    }
}