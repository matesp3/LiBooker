namespace LiBooker.Shared.DTOs
{
    // DTO representing the main info for a publication
    public class PublicationMainInfo
    {
        // Publication ID (table 'vydanie', column 'id_vydania')
        public int Id { get; set; }

        // Cover image ID (table 'titulny_obrazok', column 'id_obrazka')
        public int CoverImageId { get; set; }

        // Title of the book (table 'kniha', column 'nazov')
        public string? Title { get; set; }

        // Comma separated authors, e.g. "First Last, First2 Last2"
        public string? Author { get; set; }

        // Publisher name (table 'vydavatelstvo', column 'nazov')
        public string? Publication { get; set; }

        // Year of publication (table 'vydanie', column 'rok_vydania')
        public int? Year { get; set; }

        // Cover image bytes (table 'titulny_obrazok', column 'obrazok')
        public byte[]? Image { get; set; }
    }
}