namespace API.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> opions) : base(opions) { }

        // Tables
        public virtual DbSet<User> User { get; set; }
        public required virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Recommendation> Recommendation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Recommendation>()
                .Property(r => r.Date)
                .HasColumnType("date");

            modelBuilder.Entity<Recommendation>()
                .HasKey(e => new { e.Date, e.Type });
        }
    }
}