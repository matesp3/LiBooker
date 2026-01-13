namespace LiBookerWebApi.Models.Entities
{
    public class Genre
    {
        public int Id { get; set; } // column 'id_zanra'
        public required string Name { get; set; } // column 'nazov'

        public ICollection<BookGenre>? BookGenres { get; set; }
    }
}