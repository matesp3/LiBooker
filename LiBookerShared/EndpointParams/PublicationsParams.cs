namespace LiBooker.Shared.EndpointParams
{
    public class PublicationParams
    {
        public enum PublicationAvailability
        {
            All,
            AvailableOnly
        }

        public enum PublicationsSorting
        {
            None,
            ByTitleAsc,
            ByTitleDesc,
            ByPublicationYearAsc,
            ByPublicationYearDesc,
            ByGreatestPopularity,
        }

        public static string GetAvailabilityText(PublicationAvailability availability)
        {
            return availability switch
            {
                PublicationAvailability.All => "all",
                PublicationAvailability.AvailableOnly => "available_only",
                _ => "all",
            };
        }

        public static PublicationAvailability ParseAvailabilityParam(string? availability)
        {
            return availability?.ToLower() switch
            {
                "available_only" => PublicationAvailability.AvailableOnly,
                "all" => PublicationAvailability.All,
                _ => PublicationAvailability.All,
            };
        }

        public static string GetSortingText(PublicationsSorting sort)
        {
            return sort switch
            {
                PublicationsSorting.None => "none",
                PublicationsSorting.ByTitleAsc => "title_asc",
                PublicationsSorting.ByTitleDesc => "title_desc",
                PublicationsSorting.ByPublicationYearAsc => "year_asc",
                PublicationsSorting.ByPublicationYearDesc => "year_desc",
                PublicationsSorting.ByGreatestPopularity => "popular",
                _ => "none",
            };
        }

        public static PublicationsSorting ParseSortParam(string? sort)
        {
            return sort?.ToLower() switch
            {
                "title_asc" => PublicationsSorting.ByTitleAsc,
                "title_desc" => PublicationsSorting.ByTitleDesc,
                "year_asc" => PublicationsSorting.ByPublicationYearAsc,
                "year_desc" => PublicationsSorting.ByPublicationYearDesc,
                "popular" => PublicationsSorting.ByGreatestPopularity,
                "none" => PublicationsSorting.None,
                _ => PublicationsSorting.None,
            };
        }
    }
}
