using System.Text.Json.Serialization;

namespace LiBooker.Shared.DTOs
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")] // V JSONe pribudne pole "type"
    [JsonDerivedType(typeof(BookMatch), typeDiscriminator: "book")]
    [JsonDerivedType(typeof(AuthorMatch), typeDiscriminator: "author")]
    [JsonDerivedType(typeof(GenreMatch), typeDiscriminator: "genre")]
    public abstract class FoundMatch
    {
        public required int Id { get; set; }

    }

    public class BookMatch : FoundMatch
    {
        public required string Title { get; set; }
        public string? Authors { get; set; }
    }

    public class AuthorMatch : FoundMatch
    {
        public required string FullName { get; set; }
    }

    public class GenreMatch : FoundMatch
    {
        public required string Name { get; set; }
    }
}
