namespace LiBookerWebApi.Models.Entities
{
    public class Fine
    {
        public int Id { get; set; } // column 'id_pokuty'
        public DateTime? PaidAt { get; set; } // column 'dat_zaplatenia'
        public decimal? Amount { get; set; } // column 'cena' (NUMBER)

        public ICollection<Loan>? Loans { get; set; }
    }
}