namespace LiBooker.Shared.Roles
{
    public enum UserRoles
    {
        Admin,
        Blocked,
        Blogger,
        Librarian,
        User
    }

    public static class UserRolesExtensions
    {
        private const string AdminRoleName = "Admin";
        private const string BlockedRoleName = "Blocked";
        private const string BloggerRoleName = "Blogger";
        private const string LibrarianRoleName = "Librarian";
        private const string UserRoleName = "User";
        public static string GetRoleName(this UserRoles role)
        {
            return role switch
            {
                UserRoles.Admin => AdminRoleName,
                UserRoles.Blocked => BlockedRoleName,
                UserRoles.Blogger => BloggerRoleName,
                UserRoles.Librarian => LibrarianRoleName,
                UserRoles.User => UserRoleName,
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }

        public static IEnumerable<string> GetAllRoleNames()
        {
            return Enum.GetValues<UserRoles>().Select(r => r.GetRoleName());
        }

        public static UserRoles FromString(string roleName)
        {
            return roleName switch
            {
                AdminRoleName => UserRoles.Admin,
                BlockedRoleName => UserRoles.Blocked,
                BloggerRoleName => UserRoles.Blogger,
                LibrarianRoleName => UserRoles.Librarian,
                UserRoleName => UserRoles.User,
                _ => throw new ArgumentException($"Unknown role name: {roleName}")
            };
        }
    }
}
