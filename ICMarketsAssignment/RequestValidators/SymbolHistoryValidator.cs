using FluentValidation;
using ICMarketsAssignment.Common;
using ICMarketsAssignment.DTOs;

namespace ICMarketsAssignment.RequestValidators
{
    public class SymbolHistoryValidator : AbstractValidator<SymbolHistoryDto>
    {
        public SymbolHistoryValidator()
        {
            RuleFor(x => x.SymbolName)
           .NotEmpty()
           .Must(x => CommonSymbols.AllowedSymbols.Contains(x))
           .WithMessage(_ => $"Symbol Name must be one of: {string.Join(", ", CommonSymbols.AllowedSymbols)}");

            When(x => x.Limit.HasValue, () =>
            {
                RuleFor(x => x.Limit!.Value)
                    .GreaterThan(0)
                    .WithMessage(_ => $"Limit nust be greater than 0 else if you want the full data history, leave it empty.");
            });

        }
    }
}