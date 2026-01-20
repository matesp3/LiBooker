
namespace LiBooker.Shared.DTOs
{
    // wrapper class for response from /manage/user-info
    public class UserInfoResponse
    {
        public int? PersonId { get; set; }
        public string Email { get; set; } = string.Empty;

        public string LoginName => Email;
        public List<string> Roles { get; set; } = [];
    }
}
