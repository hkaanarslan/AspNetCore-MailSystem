using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Context
{
    public class EmailContext : IdentityDbContext<AppUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
   @"Server=localhost\SQLEXPRESS;Initial Catalog=Project2EmailNightDb;Integrated Security=True;TrustServerCertificate=True");

        }


        public DbSet<Message> Messages { get; set; }
       
    }

}