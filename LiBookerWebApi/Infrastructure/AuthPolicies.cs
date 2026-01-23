namespace LiBookerWebApi.Infrastructure;

/// <summary>
/// Global constants for Authorization Policy names.
/// </summary>
public static class AuthPolicies
{
    public const string RequireLoggedUser = "RequireLoggedUser";
    public const string RequireLibrarian = "RequireLibrarian";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireAdminOrLibrarian = "RequireAdminOrLibrarian";
}