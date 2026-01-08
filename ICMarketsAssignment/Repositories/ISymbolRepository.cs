using ICMarketsAssignment.Entities;

namespace ICMarketsAssignment.Repositories
{
    public interface ISymbolRepository
    {
        Task AddRangeAsync(IEnumerable<BlockChainSymbol> symbols, CancellationToken ct);
        Task AddAsync(BlockChainSymbol symbol, CancellationToken ct);

        Task<List<BlockChainSymbol>> GetHistoryAsync(string SymbolName, int? NumberOfRecordsLimit, CancellationToken ct);
    }
}
