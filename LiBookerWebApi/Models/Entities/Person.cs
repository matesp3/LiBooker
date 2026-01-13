namespace LiBookerWebApi.Models.Entities
{
    public class Person
    {
        public int Id { get; set; } // column 'id_osoby'
        public required string FirstName { get; set; } // column 'meno'
        public required string LastName { get; set; } // column 'priezvisko'
        public DateTime BirthDate { get; set; } // column 'dat_narodenia'
        public DateTime RegisteredAt { get; set; } // column 'dat_registracie'
        public required string Email { get; set; } // column 'email'
        public char Gender { get; set; } // column 'pohlavie' (CHAR(1))
        public string? Phone { get; set; } // column 'tel_cislo'

        public ICollection<Reservation>? Reservations { get; set; }
        public ICollection<Loan>? Loans { get; set; }
    }
}