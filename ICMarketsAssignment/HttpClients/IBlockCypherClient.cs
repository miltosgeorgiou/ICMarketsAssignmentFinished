namespace ICMarketsAssignment.HttpClients
{
    public interface IBlockCypherClient
    {
        //this is my asynchronous operation that will return the json response from the apis
        Task<string> GetResponseJsonAsync(string relativePath, CancellationToken ct);

    }
}
