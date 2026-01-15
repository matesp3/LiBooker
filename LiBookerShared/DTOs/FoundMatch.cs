namespace LiBookerShared.DTOs
{
    public class FoundMatch
    {
        public enum MatchType
        {
            Title,
            Author,
            Genre
        }
        public required string Match { get; set; } = string.Empty;

        public required MatchType Type { get; set; }
    }
}
