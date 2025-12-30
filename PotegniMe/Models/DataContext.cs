namespace PotegniMe.Models
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {

        // Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<RoleRequest> RoleRequests { get; set; }

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