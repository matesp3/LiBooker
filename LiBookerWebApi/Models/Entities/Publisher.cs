namespace LiBookerWebApi.Models.Entities
{
    public class Publisher
    {
        public int Id { get; set; } // column 'id_vydavatelstva'
        public string? Name { get; set; } // column 'nazov'
        public string? Description { get; set; } // column 'popis' (CLOB)

        public ICollection<Publication>? Publications { get; set; }
    }
}