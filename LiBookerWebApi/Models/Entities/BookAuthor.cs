namespace LiBookerWebApi.Models.Entities
{
    // join table 'knihy' (id_autora, id_knihy)
    public class BookAuthor
    {
        public int AuthorId { get; set; } // column 'id_autora'
        public int BookId { get; set; } // column 'id_knihy'

        public Author? Author { get; set; }
        public Book? Book { get; set; }
    }
}