namespace LiBookerWebApi.Models.Entities
{
    public class Author
    {
        public int Id { get; set; } // column 'id_autora"
        public string? FirstName { get; set; } // column 'meno'
        public string? LastName { get; set; } // column 'priezvisko'
        public string? Nationality { get; set; } // column 'narodnost'

        public ICollection<BookAuthor>? BookAuthors { get; set; }
    }
}