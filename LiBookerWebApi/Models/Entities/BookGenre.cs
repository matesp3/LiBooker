namespace LiBookerWebApi.Models.Entities
{
    // join table 'zanre_knih' (id_zanra, id_knihy)
    public class BookGenre
    {
        public int GenreId { get; set; } // column 'id_zanra'
        public int BookId { get; set; } // column 'id_knihy'

        public Genre? Genre { get; set; }
        public Book? Book { get; set; }
    }
}