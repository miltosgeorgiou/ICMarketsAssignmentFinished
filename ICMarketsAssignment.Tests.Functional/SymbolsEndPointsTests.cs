using System.Net;
using System.Text.Json;

namespace ICMarketsAssignment.Tests.Functional
{
    public class SymbolsEndPointsTests : IClassFixture<WebAppConfig>
    {
        private readonly HttpClient _client;

        public SymbolsEndPointsTests(WebAppConfig client)
        {
            _client = client.CreateClient();
        }

        [Fact]
        public async Task Post_FetchSingleSymbol_ReturnsOkAndContainsSymbol()
        {
            var resp = await _client.PostAsync("/api/symbols/fetch/BTC.main", content: null);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var body = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Status: {resp.StatusCode}\nBody:\n{body}");
            }
            using var doc = JsonDocument.Parse(body);

            Assert.Equal(1, doc.RootElement.GetProperty("count").GetInt32());

            var symbols = doc.RootElement.GetProperty("symbols");
            Assert.Equal("BTC.main", symbols[0].GetString());
        }
    }
}
