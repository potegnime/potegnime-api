namespace PotegniMe.Models
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {

        // Tables
        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<UserNotification> UserNotification { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Recommendation> Recommendation { get; set; }
        public DbSet<RoleRequest> RoleRequest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);
            
            // Recommendation
            // Composite key - Date and Type
            modelBuilder.Entity<Recommendation>()
                .Property(r => r.Date)
                .HasColumnType("date");
            modelBuilder.Entity<Recommendation>()
                .HasKey(r => new { r.Date, r.Type });
        }
    }
}