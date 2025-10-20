using Microsoft.EntityFrameworkCore;
using VirtualCard.Request;
using VirtualCard.Dtos;


namespace VirtualCard.Data
{
    public class VirtualCardDbContext : DbContext
    {
        public VirtualCardDbContext(DbContextOptions<VirtualCardDbContext> options) : base(options)
        {
        }
        public DbSet<CreatedCard> CreatedCards { get; set; }

        public DbSet<BlockedCard> BlockedCards { get; set; }
        public DbSet<UnblockedCard> UnBlockedCards { get; set; }
        public DbSet<ChangePinResponse> ChangePinRequests { get; set; }
        public DbSet<ResetPinResponse> ResetPinResponses { get; set; }
        public DbSet<TransectionDispute> VirtualCardTransactionDisputes { get; set; }
        
        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CreatedCard>().ToTable("CreatedCard"); // adjust if named differently
        }*/

        
    }
}
