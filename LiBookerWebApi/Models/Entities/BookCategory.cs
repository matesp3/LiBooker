namespace LiBookerWebApi.Models.Entities
{
    // join table 'kategorie_knih' (id_kategorie, id_knihy)
    public class BookCategory
    {
        public int CategoryId { get; set; } // column 'id_kategorie'
        public int BookId { get; set; } // column 'id_knihy'

        public Category? Category { get; set; }
        public Book? Book { get; set; }
    }
}