namespace LiBooker.Shared.DTOs.Admin
{
    public class UserRolesUpdate
    {
        public required string Email { get; set; }
        public required List<string> NewRoles { get; set; }
    }
}
