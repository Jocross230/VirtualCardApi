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
                entity.HasKey(e => e.id);
                entity.Property(e => e.id)
                      .HasColumnName("id")
                      .HasColumnType("varchar(50)")
                      .IsRequired();
            });
        }
    }
}
