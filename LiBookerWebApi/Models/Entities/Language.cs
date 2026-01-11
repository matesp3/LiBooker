namespace LiBookerWebApi.Models.Entities
{
    public class Language
    {
        public int Id { get; set; } // column 'id_jazyka'
        public string? Name { get; set; } // column 'nazov'
        public string? Code { get; set; } // column 'skratka'

        public ICollection<Publication>? Publications { get; set; }
    }
}