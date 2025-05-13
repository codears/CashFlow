using CashFlow.Domain.Models;
using FluentValidation;

namespace CashFlow.CashierApi.Validations
{
    public class CashPostingRequestValidator : AbstractValidator<CashPostingRequest>
    {
        public CashPostingRequestValidator()
        {
            RuleFor(p => p.Amount)
                .NotEmpty().WithMessage("Amount is required")
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(p => p.PostingType)
                .NotEmpty().WithMessage("Posting type is required")
                .Must(ValidatePostingType).WithMessage("Posting type must be either 'C' (credit) or 'D' (debit)");

            RuleFor(p => p.Description)
                .MaximumLength(255).WithMessage("Description cannot exceed 255 characters");
        }

        private bool ValidatePostingType(string postingType)
        {
            return postingType == "C" || postingType == "D";
        }
    }
}