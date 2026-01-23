namespace LiBooker.Shared.DTOs
{
    public class LoanInfo
    {
        public required int LoanId { get; set; }

        public required int PublicationId { get; set; }

        public required string BookTitle { get; set; } = string.Empty;

        public required DateTime DateFrom { get; set; }

        public required DateTime DateTo { get; set; }

        public DateTime? ReturnDate { get; set; } = null;

        /// <summary>
        /// overdue/on time/not returned
        /// </summary>
        public string ReturnedDescription => ReturnDate switch
        {
            null => "not returned",
            var date when date.Value.Date <= DateTo.Date => "on time",
            _ => "overdue"
        };
    }
}
