using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LiBookerWebApi.Models.Entities
{
    //[Table("V_VYPOZICANIE_EF")]
    public class LoanEf
    {
        //[Key]
        //[Column("ID_VYPOZICANIA")]
        public int Id { get; set; } // column 'id_vypozicania'
        //[Column("OSOBA_ID_OSOBY")]
        public int PersonId { get; set; } // column 'osoba_id_osoby'
        //[Column("VYTLACOK_ID_VYTLACKA")]
        public int CopyId { get; set; } // column 'vytlacok_id_vytlacka'
        //[Column("DAT_VYPOZICANIA")]
        public DateTime LoanedAt { get; set; } // column 'dat_vypozicania'
        //[Column("DAT_KONCA_VYPOZ")]
        public DateTime DueAt { get; set; } // column 'dat_konca_vypoz'
        //[Column("DAT_VRATENIA")]
        public DateTime? ReturnedAt { get; set; } // column 'dat_vratenia'
        //[Column("POKUTA_ID")] // mapped to calculated column in DB view
        public int? FineId { get; set; } // column 'o_pokuta'

    }
}
