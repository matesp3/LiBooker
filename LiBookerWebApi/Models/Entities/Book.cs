namespace LiBookerWebApi.Models.Entities
{
    public class Book
    {
        public int Id { get; set; } // column 'id_knihy'
        public string? Title { get; set; } // column 'nazov'
        public string? Description { get; set; } // column 'popis' (CLOB)

        public ICollection<BookAuthor>? BookAuthors { get; set; }
        public ICollection<BookGenre>? BookGenres { get; set; }
        public ICollection<BookCategory>? BookCategories { get; set; }
        public ICollection<Publication>? Publications { get; set; }
    }
}