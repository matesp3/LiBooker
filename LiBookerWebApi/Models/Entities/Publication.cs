namespace LiBookerWebApi.Models.Entities
{
    public class Publication
    {
        public int Id { get; set; } // column 'id_vydania'
        public int BookId { get; set; } // column 'id_knihy'
        public int? PublisherId { get; set; } // column 'id_vydavatelstva'
        public int? LanguageId { get; set; } // column 'id_jazyka'
        public int? CoverImageId { get; set; } // column 'id_obrazku'
        public string? ISBN { get; set; } // column 'ISBN'
        public int? Year { get; set; } // column 'rok_vydania'
        public int? EditionNumber { get; set; } // column 'cislo_vydania'
        public int? PageCount { get; set; } // column 'pocet_stran'
        public string? XmlProperties { get; set; } // column 'vlastnosti_xml' (XMLTYPE)

        public Book? Book { get; set; }
        public Publisher? Publisher { get; set; }
        public Language? Language { get; set; }
        public CoverImage? CoverImage { get; set; }
        public ICollection<Copy>? Copies { get; set; }
        public ICollection<Reservation>? Reservations { get; set; }
    }
}