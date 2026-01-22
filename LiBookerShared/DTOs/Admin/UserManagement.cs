namespace LiBooker.Shared.DTOs.Admin
{
    public class UserManagement
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        
        public List<string> Roles { get; set; } = [];

        // Comma separated roles or list, used for display
        public string RolesDisplay => (Roles != null && Roles.Count > 0) 
            ? string.Join(", ", Roles) 
            : "-";
    }
}