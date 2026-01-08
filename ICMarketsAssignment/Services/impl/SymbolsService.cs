using ICMarketsAssignment.Common;
using ICMarketsAssignment.Entities;
using ICMarketsAssignment.HttpClients;
using ICMarketsAssignment.Repositories;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ICMarketsAssignment.Services.impl
{
    public class SymbolsService : ISymbolsService
    {
        private readonly ILogger<SymbolsService> _logger;

        private readonly IBlockCypherClient _client;
        private readonly ISymbolRepository _symbolRepository;

        public SymbolsService(IBlockCypherClient client, ISymbolRepository symbolRepository, ILogger<SymbolsService> logger)
        {
            _client = client;
            _symbolRepository = symbolRepository;
            _logger = logger;
        }

        //Method that fetches a single symbol from endpoint and saves it to database and returns the response.
        public async Task<object> FetchSingleSymbolAsync(string symbolName, CancellationToken ct)
        {
            _logger.LogInformation("In FetchSingleSymbolAsync");
            var symbol = CommonSymbols.SymbolsAndPaths.SingleOrDefault(s => s.SymbolName == symbolName);
            // in case i have mistyped the symbol 
            if (symbol == default)
            {
                throw new ArgumentException($"Unknown symbol '{symbolName}'");
            }

            var responseJson = await _client.GetResponseJsonAsync(symbol.Path, ct);

            var now = DateTime.Now;

            var result = new BlockChainSymbol
            {
                SymbolName = symbol.SymbolName,
                ResponseJson = responseJson,
                CreatedAt = now
            };
            //Save to db
            await _symbolRepository.AddAsync(result, ct);
            var response = new JsonObject
            {
                ["storedAt"] = now,
                ["count"] = 1,
                ["symbols"] = new JsonArray((JsonNode?)result.SymbolName)
            };

            var payloads = new JsonObject();
            payloads[result.SymbolName] = JsonNode.Parse(result.ResponseJson);
            response["results"] = payloads;
            _logger.LogInformation("In FetchSingleSymbolAsync - END");
            return response;
        }

        //Method that fetches all symbols from endpoints and saves them to database and returns the responses.
        public async Task<object> FetchAllSymbolsAsync(bool showResults, CancellationToken ct)
        {
            _logger.LogInformation("In FetchAllSymbolsAsync");

            //var fetchTasks = Symbols.Select(async c => new
            //{
            //    c.SymbolName,
            //    ResponseJson = await _client.GetResponseJsonAsync(c.Path, ct)
            //});

            //I went with this approach below instead of the one above that i had earlier because if one fetch symbol was failing, it killed the whole fetching.
            //I instead do a safe fetch where if one fetch fails, i catch it,log it and add it to the failed ones and move on to the next.
            var fetchTasks = CommonSymbols.SymbolsAndPaths.Select(c => SafeFetchAsync(c.SymbolName, c.Path, ct));

            var results = await Task.WhenAll(fetchTasks);

            var now = DateTime.Now;

            var successSymbols = results
                .Where(r => r.ResponseJson is not null)
                .Select(r => new BlockChainSymbol
                {
                    SymbolName = r.SymbolName,
                    ResponseJson = r.ResponseJson!,
                    CreatedAt = now
                })
                .ToList();

            var failedSymbols = results.Where(r => r.ResponseJson is null).ToList();

            //Log the results failed and sucess.
            _logger.LogInformation(
            "FetchAllSymbolsAsync finished. stored={StoredCount}, failed={FailedCount}, tookMs={Ms}",
            successSymbols.Count,
            failedSymbols.Count,
            (DateTime.Now - now).TotalMilliseconds
            );

            //Save to db only the success fetched symbols
            await _symbolRepository.AddRangeAsync(successSymbols, ct);

            var response = new JsonObject
            {
                ["storedAt"] = JsonValue.Create(now),
                ["Total"] = results.Length,
                ["Successful Stored Symbols"] = new JsonArray(
                    successSymbols
                        .Select(s => (JsonNode?)s.SymbolName)
                        .ToArray()
                ),
                ["Failed Symbols"] = new JsonArray(
                    failedSymbols
                        .Select(f => new JsonObject
                        {
                            ["symbolName"] = f.SymbolName,
                            ["error"] = f.Error
                        })
                        .ToArray()
                )
            };

            if (showResults)
            {
                var payloads = new JsonObject();
                foreach (var r in successSymbols)
                    payloads[r.SymbolName] = JsonNode.Parse(r.ResponseJson);

                response["results"] = payloads;
            }
            _logger.LogInformation("In FetchAllSymbolsAsync - END");

            return response;
        }

        public async Task<object> GetHistoryAsync(string SymbolName, int? limit, CancellationToken ct)
        {
            _logger.LogInformation("In GetHistoryAsync");

            var NumberOfRecordsToShow = (limit is null || limit <= 0) ? 9999999 : limit.Value;

            var rows = await _symbolRepository.GetHistoryAsync(SymbolName, NumberOfRecordsToShow, ct);

            // parse JSON AFTER fetching from DB (in-memory)
            var result = rows.Select(x => new
            {
                x.RecordId,
                x.SymbolName,
                CreatedAt = x.CreatedAt,
                Data = JsonDocument.Parse(x.ResponseJson).RootElement
            });

            _logger.LogInformation("In GetHistoryAsync - END");

            return result;

        }

        //Helper method
        public async Task<(string SymbolName, string? ResponseJson, string? Error)> SafeFetchAsync(string symbolName, string path, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching {SymbolName} from {path}", symbolName, path);
                var ResponseJson = await _client.GetResponseJsonAsync(path, ct);
                _logger.LogInformation("Fetched {SymbolName} successfully", symbolName);
                return (symbolName, ResponseJson, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetch failed for {SymbolName}", symbolName);
                return (symbolName, null, ex.Message);
            }
        }
    }
}
