namespace LiBookerWebApi.Infrastructure;

/// <summary>
/// Global constants for Authorization Policy names.
/// </summary>
public static class AuthPolicies
{
    public const string RequireLoggedUser = "RequireLoggedUser";
    public const string RequireBlogger = "RequireBlogger";
    public const string RequireAdmin = "RequireAdmin";
}