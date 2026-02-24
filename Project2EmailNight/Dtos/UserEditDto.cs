using Microsoft.AspNetCore.Http;

namespace Project2EmailNight.Dtos
{
    public class UserEditDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }

    
        public string? Password { get; set; }

      
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }

        public string? Phone { get; set; }
        public string? About { get; set; }

       
        public DateTime? BirthDate { get; set; }
        public string? City { get; set; }
   
    }
}
