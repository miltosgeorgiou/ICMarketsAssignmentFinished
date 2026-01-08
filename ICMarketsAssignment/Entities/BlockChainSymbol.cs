using System.ComponentModel.DataAnnotations;

namespace ICMarketsAssignment.Entities
{
    public class BlockChainSymbol
    {
        [Key]
        public long RecordId { get; set; }

        //  ETH, Dash, BTC, LTC symbols
        public string SymbolName { get; set; } = string.Empty;

        // Main data should be stored as provided in theAPI’s JSON responses.
        public string ResponseJson { get; set; } = string.Empty;

        // column adding the time requested from the API endpoint.
        public DateTime CreatedAt { get; set; }
    }
}
