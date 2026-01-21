namespace LiBookerWebApi.Models.Entities
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace VasProjekt.Models
    {
        [Keyless] //  no primary key for this entity (specifically for a view)
        public class PublicationDetails
        {
            [Column("PUBLICATIONID")]
            public int PublicationId { get; set; }

            [Column("BOOKID")]
            public int BookId { get; set; }

            [Column("BOOKTITLE")]
            public string BookTitle { get; set; } = string.Empty;

            [Column("PUBLISHERID")]
            public int PublisherId { get; set; }

            [Column("PUBLISHERNAME")]
            public string PublisherName { get; set; } = string.Empty;

            [Column("LANGUAGEID")]
            public int LanguageId { get; set; }

            /// <summary>
            /// LISTAGG result in format "Author1Id,Author2Id,Author3Id"
            /// </summary>
            [Column("AUTHORIDS")]
            public string AuthorIds { get; set; } = string.Empty;

            /// <summary>
            /// LISTAGG result in format "Author1[Author1Id],Author2[Author2Id],Author3[Author3Id]"
            /// </summary>
            [Column("AUTHORFULLNAMES")]
            public string AuthorFullNames { get; set; } = string.Empty;

            /// <summary>
            /// LISTAGG result in format "Genre1Id,Genre2Id,Genre3Id"
            /// </summary>
            [Column("GENREIDS")]
            public string GenreIds { get; set; } = string.Empty;

            /// <summary>
            /// LISTAGG result in format "Genre1[Genre1Id],Genre2[Genre2Id],Genre3[Genre3Id]"
            /// </summary>
            [Column("GENRENAMES")]
            public string GenreNames { get; set; } = string.Empty;

            [Column("LANGUAGENAME")]
            public string LanguageName { get; set; } = string.Empty;

            [Column("PICTUREID")]
            public int? PictureId { get; set; } // may be null

            [Column("ISBN")]
            public string Isbn { get; set; } = string.Empty;

            [Column("PUBLICATIONYEAR")]
            public int PublicationYear { get; set; }

            [Column("PAGECOUNT")]
            public int PageCount { get; set; }

            // xml property
            [Column("BINDING")]
            public string Binding { get; set; } = string.Empty;

            // xml property
            [Column("PAPER")]
            public string Paper { get; set; } = string.Empty;

            // xml property
            [Column("COVER")]
            public string Cover { get; set; } = string.Empty;

            // xml properties
            [Column("DIMENSIONS")]
            public string Dimensions { get; set; } = string.Empty;

            // xml property, contains unit ' [g]'
            [Column("WEIGHT")]
            public string Weight { get; set; } = string.Empty;

            /// <summary>
            /// Returns a comma-separated list of author names, excluding any associated identifiers.
            /// </summary>
            /// <returns>A string containing the names of all authors without their IDs. If no author names are available,
            /// returns an empty string.</returns>
            public string GetAuthorNamesWithoutIds() => GetItemsWithoutIds(AuthorFullNames);

            /// <summary>
            /// Returns a comma-separated list of genre names, excluding any associated identifiers.
            /// </summary>
            /// <returns></returns>
            public string GetGenreNamesWithoutIds() => GetItemsWithoutIds(GenreNames);

            private static string GetItemsWithoutIds(string fullStr)
            {
                string[] items = fullStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (items.Length == 0)
                    return string.Empty;
                int i = 0;
                string result = items[i][..items[i].IndexOf('[')];
                for (++i; i < items.Length; i++)
                {
                    result += $", {items[i][..items[i].IndexOf('[')]}";
                }
                return result;
            }
        }
    }
}
