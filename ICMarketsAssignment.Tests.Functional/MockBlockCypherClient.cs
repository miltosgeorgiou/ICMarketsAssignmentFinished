using ICMarketsAssignment.HttpClients;

namespace ICMarketsAssignment.Tests.Functional
{
    public class MockBlockCypherClient : IBlockCypherClient
    {
        public Task<string> GetResponseJsonAsync(string path, CancellationToken ct)
        {

            return Task.FromResult($"{{\"path\":\"{path}\",\"ok\":true}}");
        }
    }
}
