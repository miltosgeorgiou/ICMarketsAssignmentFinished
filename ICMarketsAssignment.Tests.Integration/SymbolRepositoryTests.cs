using ICMarketsAssignment.Entities;

namespace ICMarketsAssignment.Tests.Integration
{
    public class SymbolRepositoryTests
    {
        // Test the descending order
        [Fact]
        public async Task GetHistoryAsync_ReturnsDescendingByCreatedAt()
        {
            var (conn, db, repo) = TestDatabaseConfig.Create();
            await using var _ = conn;
            await using var __ = db;

            var symbol = "BTC.main";

            // Add 3 records with different CreatedAt values
            db.Symbols.AddRange(
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0) },
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 2, 10, 0, 0) },
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 3, 10, 0, 0) }
            );

            await db.SaveChangesAsync();

            var rows = await repo.GetHistoryAsync(symbol, 10, CancellationToken.None);

            Assert.Equal(3, rows.Count);
            Assert.Equal(new DateTime(2026, 1, 3, 10, 0, 0), rows[0].CreatedAt);
            Assert.Equal(new DateTime(2026, 1, 2, 10, 0, 0), rows[1].CreatedAt);
            Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), rows[2].CreatedAt);
        }

        //Test the limit parameter
        [Fact]
        public async Task GetHistoryAsync_RespectsLimit()
        {
            var (conn, db, repo) = TestDatabaseConfig.Create();
            await using var _ = conn;
            await using var __ = db;

            var symbol = "BTC.main";

            db.Symbols.AddRange(
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0) },
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 2, 10, 0, 0) },
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 3, 10, 0, 0) },
                new BlockChainSymbol { SymbolName = symbol, ResponseJson = "{}", CreatedAt = new DateTime(2026, 1, 4, 10, 0, 0) }
            );

            await db.SaveChangesAsync();

            // Act
            var rows = await repo.GetHistoryAsync(symbol, 2, CancellationToken.None);


            Assert.Equal(2, rows.Count);
            Assert.Equal(new DateTime(2026, 1, 4, 10, 0, 0), rows[0].CreatedAt);
            Assert.Equal(new DateTime(2026, 1, 3, 10, 0, 0), rows[1].CreatedAt);

        }

    }
}
