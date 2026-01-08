using ICMarketsAssignment.Entities;
using ICMarketsAssignment.HttpClients;
using ICMarketsAssignment.Repositories;
using ICMarketsAssignment.Services.impl;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text.Json.Nodes;

namespace ICMarketsAssignment.Tests.Unit
{
    public partial class SymbolsServiceTests
    {
        [Fact]
        public async Task FetchSingleSymbolAsync_ValidSymbol_CallsClientAndStoresToRepository()
        {

            var clientMock = new Mock<IBlockCypherClient>(MockBehavior.Strict);
            var repoMock = new Mock<ISymbolRepository>(MockBehavior.Strict);

            var logger = NullLogger<SymbolsService>.Instance;

            // I will use the BTC.main for the test
            var symbolName = "BTC.main";

            var apiJson = """{ "name": "BTC.main" }""";

            clientMock
                .Setup(c => c.GetResponseJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(apiJson);

            BlockChainSymbol? symbol = null;

            repoMock
                .Setup(r => r.AddAsync(It.IsAny<BlockChainSymbol>(), It.IsAny<CancellationToken>()))
                .Callback<BlockChainSymbol, CancellationToken>((entity, _) => symbol = entity)
                .Returns(Task.CompletedTask);

            var service = new SymbolsService(clientMock.Object, repoMock.Object, logger);

            var resultObj = await service.FetchSingleSymbolAsync(symbolName, CancellationToken.None);

            var json = Assert.IsType<JsonObject>(resultObj);

            // results contains the parsed payload under the symbol key
            var results = Assert.IsType<JsonObject>(json["results"]);
            Assert.True(results.ContainsKey(symbolName));

            // Verify calls
            clientMock.Verify(c => c.GetResponseJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            repoMock.Verify(r => r.AddAsync(It.IsAny<BlockChainSymbol>(), It.IsAny<CancellationToken>()), Times.Once);
            repoMock.VerifyNoOtherCalls();
            clientMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task FetchAllSymbolsAsync_OneFetchFails_StoresOnlySuccesses_AndReturnsFailures()
        {
            var clientMock = new Mock<IBlockCypherClient>(MockBehavior.Loose);
            var repoMock = new Mock<ISymbolRepository>(MockBehavior.Strict);
            var logger = NullLogger<SymbolsService>.Instance;


            var callCount = 0;

            clientMock
                .Setup(c => c.GetResponseJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((_, __) =>
                {
                    callCount++;
                    if (callCount == 5)
                        throw new HttpRequestException("Something went wrong");

                    return Task.FromResult($"{{\"name\":\"{callCount}\",\"");


                });

            IReadOnlyList<BlockChainSymbol>? successResults = null;

            repoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<BlockChainSymbol>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<BlockChainSymbol>, CancellationToken>((success, _) => successResults = success.ToList())
                .Returns(Task.CompletedTask);

            var service = new SymbolsService(clientMock.Object, repoMock.Object, logger);

            var resultObj = await service.FetchAllSymbolsAsync(showResults: false, ct: CancellationToken.None);

            //make sure that repository has the success responses.
            Assert.NotNull(successResults);
            Assert.Equal(4, successResults!.Count);
            Assert.All(successResults, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.SymbolName));
                Assert.False(string.IsNullOrWhiteSpace(s.ResponseJson));
            });

            var json = Assert.IsType<JsonObject>(resultObj);

            Assert.Equal(5, json["Total"]!.GetValue<int>());

            var successful = Assert.IsType<JsonArray>(json["Successful Stored Symbols"]);
            Assert.Equal(4, successful.Count);

            var failed = Assert.IsType<JsonArray>(json["Failed Symbols"]);
            Assert.Single(failed);

            repoMock.Verify(r => r.AddRangeAsync(It.IsAny<List<BlockChainSymbol>>(), It.IsAny<CancellationToken>()), Times.Once);
            repoMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task FetchSingleSymbolAsync_UnknownSymbol()
        {
            var clientMock = new Mock<IBlockCypherClient>(MockBehavior.Strict);
            var repoMock = new Mock<ISymbolRepository>(MockBehavior.Strict);
            var logger = NullLogger<SymbolsService>.Instance;
            var service = new SymbolsService(clientMock.Object, repoMock.Object, logger);

            var invalidSymbol = "INVALID.symbol";

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.FetchSingleSymbolAsync(invalidSymbol, CancellationToken.None));

            Assert.Contains("Unknown symbol", ex.Message);

            // Testing that it did not go through will calling the endpoint
            clientMock.Verify(
                c => c.GetResponseJsonAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);

            //Test that it did not go through with calling the repository and trying to add the symbol to the database.
            repoMock.Verify(
                r => r.AddAsync(It.IsAny<BlockChainSymbol>(), It.IsAny<CancellationToken>()),
                Times.Never);

        }
    }
}
