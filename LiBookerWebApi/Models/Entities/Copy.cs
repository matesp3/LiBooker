namespace LiBookerWebApi.Models.Entities
{
    public class Copy
    {
        public int Id { get; set; } // column 'id_vytlacka'
        public int PublicationId { get; set; } // column 'id_vydania'
        public string? Status { get; set; } // column 'stav'

        public Publication? Publication { get; set; }
        public ICollection<Loan>? Loans { get; set; }
    }
}