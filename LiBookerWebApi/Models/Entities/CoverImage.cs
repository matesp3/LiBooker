namespace LiBookerWebApi.Models.Entities
{
    public class CoverImage
    {
        public int Id { get; set; } // column 'id_obrazku'
        public byte[]? Image { get; set; } // column 'obrazok' (BLOB)

        public ICollection<Publication>? Publications { get; set; }
    }
}