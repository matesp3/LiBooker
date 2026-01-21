namespace LiBooker.Shared.DTOs
{
    namespace VasProjekt.Shared.Dtos
    {
        public class PublicationDetails
        {
            public int PublicationId { get; set; }
            public int BookId { get; set; }

            public string BookTitle { get; set; } = string.Empty;

            public int PublisherId { get; set; }
            public string PublisherName { get; set; } = string.Empty;

            public int LanguageId { get; set; }
            public string LanguageName { get; set; } = string.Empty;

            // result in format "Author1Id,Author2Id,Author3Id"
            public string AuthorIds { get; set; } = string.Empty;
            // result in format "AuthorFullName1,AuthorFullName2,AuthorFullName3"
            public string AuthorFullNames { get; set; } = string.Empty;
            // result in format "Genre1Id,Genre2Id,Genre3Id"
            public string GenreIds { get; set; } = string.Empty;
            // result in format "GenreName1,GenreName2,GenreName3"
            public string GenreNames { get; set; } = string.Empty;

            public int? PictureId { get; set; }

            public string Isbn { get; set; } = string.Empty;

            public int PublicationYear { get; set; }
            public int PageCount { get; set; }

            public string Binding { get; set; } = string.Empty;
            public string Paper { get; set; } = string.Empty;
            public string Cover { get; set; } = string.Empty;

            // contains unit (e.g., "cm", "inches") as part of the string
            public string Dimensions { get; set; } = string.Empty;
            public string Weight { get; set; } = string.Empty;
        }
    }
}
