namespace LiBookerWebApi.Models.Entities
{
    public class Category
    {
        public int Id { get; set; } // column 'id_kategorie'
        public string? Name { get; set; } // column 'nazov'

        public ICollection<BookCategory>? BookCategories { get; set; }
    }
}