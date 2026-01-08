using FluentValidation;
using ICMarketsAssignment.DTOs;
using ICMarketsAssignment.Services;
using Microsoft.AspNetCore.Mvc;

namespace ICMarketsAssignment.Controllers
{
    [ApiController]
    [Route("api/symbols")]
    public class SymbolsController : ControllerBase
    {
        private readonly ISymbolsService symbolsService;

        public SymbolsController(ISymbolsService symbolsService)
        {
            this.symbolsService = symbolsService;
        }

        //Endpoint to fetch and store in database all symbols from the specified endpoints.
        [HttpPost("fetch/all")]
        public async Task<IActionResult> FetchAll([FromQuery] bool showResults = true, CancellationToken ct = default)
        {
            var result = await symbolsService.FetchAllSymbolsAsync(showResults, ct);
            return Ok(result);
        }

        //Endpoint to fetch and store in database a single symbol from the specified endpoints.
        [HttpPost("fetch/{symbolName}")]
        public async Task<IActionResult> FetchSingleSymbol(
            [FromRoute] string symbolName,
             [FromServices] IValidator<SymbolDto> validator,
            CancellationToken ct = default)
        {

            var symbolDto = new SymbolDto
            {
                SymbolName = symbolName
            };

            // i will validate the name of the symbol
            var validation = await validator.ValidateAsync(symbolDto, ct);

            if (!validation.IsValid)
            {
                return BadRequest(validation.Errors);
            }

            var result = await symbolsService.FetchSingleSymbolAsync(symbolDto.SymbolName, ct);
            return Ok(result);
        }

        //Endpoint to fetch from database the history of a specified symbol, with DESC order.You can also specify the limit of the fetched results you want.
        [HttpGet("history/{symbolName}")]
        public async Task<IActionResult> History(
            [FromRoute] string symbolName,
            [FromQuery] int? limit,
            [FromServices] IValidator<SymbolHistoryDto> validator,
            CancellationToken ct = default)
        {
            var symbolHistoryDto = new SymbolHistoryDto
            {
                SymbolName = symbolName,
                Limit = limit
            };
            // i will validate the name of the symbol and if limit has a value , that is bigger than 0
            var validation = await validator.ValidateAsync(symbolHistoryDto, ct);
            if (!validation.IsValid)
            {
                return BadRequest(validation.Errors);
            }

            var result = await symbolsService.GetHistoryAsync(symbolHistoryDto.SymbolName, symbolHistoryDto.Limit, ct);
            return Ok(result);
        }
    }
}
