using ICMarketsAssignment.Services.impl;

namespace ICMarketsAssignment.HttpClients.impl
{
    public class BlockCypherClientService : IBlockCypherClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<SymbolsService> _logger;


        public BlockCypherClientService(HttpClient http, ILogger<SymbolsService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<string> GetResponseJsonAsync(string relativePath, CancellationToken ct)
        {
            using var resp = await _http.GetAsync(relativePath, ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(ct);
            throw new NotImplementedException();
        }



    }
}
