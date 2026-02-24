using Microsoft.AspNetCore.Identity;

namespace Project2EmailNight.Entities
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string? ImageUrl { get; set; }
        public string? About { get; set; }
        public string? ConfirmCode { get; set; }

        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? City { get; set; }
    }
}
