using ICMarketsAssignment.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarketsAssignment.AppDatabaseContext

{
    public class DatabaseContext : DbContext
    {
        public DbSet<BlockChainSymbol> Symbols => Set<BlockChainSymbol>();

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<BlockChainSymbol>();

            e.Property(x => x.SymbolName).IsRequired();
            e.Property(x => x.ResponseJson).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();

            e.HasIndex(x => new { x.SymbolName, x.CreatedAt });
        }

    }
}
