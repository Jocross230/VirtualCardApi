using Microsoft.EntityFrameworkCore;
using VirtualCard.Request;

namespace VirtualCard.Data
{
    public class UserProfileDbContext : DbContext
    {
        public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfileTable> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfileTable>(entity =>
            {
                entity.ToTable("user_profile", schema: "dbo");
                entity.HasNoKey();
                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.cus_num).HasColumnName("cus_num");
            });
        }
    }
}
