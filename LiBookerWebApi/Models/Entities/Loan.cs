namespace LiBookerWebApi.Models.Entities
{
    public class Loan
    {
        public int Id { get; set; } // column 'id_vypozicania'
        public int PersonId { get; set; } // column 'osoba_id_osoby'
        public int CopyId { get; set; } // column 'vytlacok_id_vytlacka'
        public DateTime? LoanedAt { get; set; } // column 'dat_vypozicania'
        public DateTime? DueAt { get; set; } // column 'dat_konca_vypoz'
        public DateTime? ReturnedAt { get; set; } // column 'dat_vratenia'
        public int? FineId { get; set; } // column 'o_pokuta'

        public Person? Person { get; set; }
        public Copy? Copy { get; set; }
        public Fine? Fine { get; set; }
    }
}