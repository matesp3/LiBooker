namespace LiBooker.Shared.DTOs
{
    // DTO representing the main info for a publication
    public class PublicationMainInfo
    {
        // Title of the book (table 'kniha', column 'nazov')
        public required string Title { get; set; }

        // Comma separated authors, e.g. "First Last, First2 Last2"
        public required string Author { get; set; }

        // Publisher name (table 'vydavatelstvo', column 'nazov')
        public required string Publication { get; set; }

        // Year of publication (table 'vydanie', column 'rok_vydania')
        public int Year { get; set; }

        // Cover image ID (table 'titulny_obrazok', column 'id_obrazku')
        public int ImageId { get; set; }

        // Cover image bytes (table 'titulny_obrazok', column 'obrazok')
        public byte[]? Image { get; set; }
    }
}