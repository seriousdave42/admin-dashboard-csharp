using Microsoft.EntityFrameworkCore;

namespace UserAdmin.Models
{
    public class AdminContext : DbContext
    {
        public AdminContext (DbContextOptions options) : base(options) {}
        public DbSet<User> Users {get; set;}
        public DbSet<Message> Messages {get; set;}
        public DbSet<Comment> Comments {get; set;}
    }
}