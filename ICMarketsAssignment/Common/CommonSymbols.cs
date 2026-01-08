namespace ICMarketsAssignment.Common
{

    public class CommonSymbols
    {
        // Assignment list of symbols to fetch ( I have used he String comparer to make it case insensitive)
        public static readonly IReadOnlySet<string> AllowedSymbols =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ETH.main",
            "DASH.main",
            "BTC.main",
            "BTC.test3",
            "LTC.main"
        };


        public static readonly (string SymbolName, string Path)[] SymbolsAndPaths =
       [
           ("ETH.main", "/v1/eth/main"),
           ("DASH.main", "/v1/dash/main"),
           ("BTC.main", "/v1/btc/main"),
           ("BTC.test3", "/v1/btc/test3"),
           ("LTC.main", "/v1/ltc/main"),
       ];
    }
}
