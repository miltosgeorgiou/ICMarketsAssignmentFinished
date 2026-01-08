using FluentValidation;
using ICMarketsAssignment.Common;
using ICMarketsAssignment.DTOs;

namespace ICMarketsAssignment.RequestValidators
{
    public class SymbolValidator : AbstractValidator<SymbolDto>
    {
        public SymbolValidator()
        {
            RuleFor(x => x.SymbolName)
                .NotEmpty()
                .Must(x => CommonSymbols.AllowedSymbols.Contains(x))
                .WithMessage(_ => $"Symbol Name must be one of: {string.Join(", ", CommonSymbols.AllowedSymbols)}");
        }
    }
}
