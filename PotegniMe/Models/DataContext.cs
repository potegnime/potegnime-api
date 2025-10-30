// TODO make sure roles are in the database on initialization - admin, user...
namespace PotegniMe.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opions) : base(opions) { }

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
            modelBuilder.Entity<User>()
                .Property(u => u.AuthToken)
                .HasDefaultValue("0");

            modelBuilder.Entity<User>()
                .Property(u => u.UploadedTorrentsCount)
                .HasDefaultValue(0);

            // Role

            // UserNotification

            // Recommendation
            // Composite key - Date and Type
            modelBuilder.Entity<Recommendation>()
                .Property(r => r.Date)
                .HasColumnType("date");
            modelBuilder.Entity<Recommendation>()
                .HasKey(e => new { e.Date, e.Type });
        }
    }
}