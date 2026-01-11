namespace LiBookerWebApi.Models.Entities
{
    public class Reservation
    {
        public int Id { get; set; } // column 'id_rezervacie'
        public int PublicationId { get; set; } // column 'id_vydania'
        public int PersonId { get; set; } // column 'id_osoby'
        public DateTime? ReservedAt { get; set; } // column 'dat_rezervacie'

        public Publication? Publication { get; set; }
        public Person? Person { get; set; }
    }
}