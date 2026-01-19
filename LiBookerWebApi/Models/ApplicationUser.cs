using Microsoft.AspNetCore.Identity;

namespace LiBookerWebApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Foreign key to the existing Person table ('osoba').
        /// This will link the Identity user to the library person record.
        /// </summary>
        public int? PersonId { get; set; }
    }
}
