namespace ICMarketsAssignment.Services
{
    public interface ISymbolsService
    {
        Task<object> FetchAllSymbolsAsync(bool showResults, CancellationToken ct);

        Task<object> FetchSingleSymbolAsync(string symbolName, CancellationToken ct);


        Task<object> GetHistoryAsync(string SymbolName, int? limit, CancellationToken ct);


    }
}
