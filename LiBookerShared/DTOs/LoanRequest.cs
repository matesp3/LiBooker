namespace LiBooker.Shared.DTOs
{
    public class LoanRequest
    {
        public required int PersonId { get; set; }

        public required int PublicationId { get; set; }
    }
}
