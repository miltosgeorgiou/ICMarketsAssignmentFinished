using ICMarketsAssignment.AppDatabaseContext;
using ICMarketsAssignment.Entities;
using Microsoft.EntityFrameworkCore;

namespace ICMarketsAssignment.Repositories.impl
{
    public class SymbolRepository : ISymbolRepository
    {
        private readonly DatabaseContext _db;

        public SymbolRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task AddRangeAsync(IEnumerable<BlockChainSymbol> Symbols, CancellationToken ct)
        {
            await _db.Symbols.AddRangeAsync(Symbols, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task AddAsync(BlockChainSymbol Symbol, CancellationToken ct)
        {
            await _db.Symbols.AddAsync(Symbol, ct);
            await _db.SaveChangesAsync(ct);
        }

        public Task<List<BlockChainSymbol>> GetHistoryAsync(string SymbolName, int? limit, CancellationToken ct)
        {
            var NumberOfRecordsToShow = (limit is null || limit <= 0) ? 9999999 : limit.Value;

            return _db.Symbols
                .Where(x => x.SymbolName == SymbolName)
                .OrderByDescending(x => x.CreatedAt)
                .Take(NumberOfRecordsToShow)
                .ToListAsync(ct);
        }
    }
}
