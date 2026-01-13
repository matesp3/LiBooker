namespace LiBookerWebApi.Models.Entities
{
    public class Book
    {
        public int Id { get; set; } // column 'id_knihy'
        public required string Title { get; set; } // column 'nazov'
        public string? Description { get; set; } // column 'popis' (CLOB)

        public required ICollection<BookAuthor> BookAuthors { get; set; }
        public required ICollection<BookGenre> BookGenres { get; set; }
        public required ICollection<BookCategory> BookCategories { get; set; }
        public required ICollection<Publication> Publications { get; set; }
    }
}