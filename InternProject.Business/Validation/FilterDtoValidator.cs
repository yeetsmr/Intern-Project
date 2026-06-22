using FluentValidation;
using InternProject.Core;

namespace InternProject.Business.Validation
{
    public class FilterDtoValidator : AbstractValidator<FilterDto>
    {
        public FilterDtoValidator()
        {

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page number must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("At least 1, at most {To} records can be listed per page.");

            RuleFor(x => x.MaxTime)
                .GreaterThan(0)
                .When(x => x.MaxTime.HasValue)
                .WithMessage("Maximum time must be greater than 0.");

            RuleFor(x => x.pri)
                .InclusiveBetween(0, 2)
                .When(x => x.pri.HasValue)
                .WithMessage("Invalid priority value. Only 0(low), 1(mid) or 2(high) is allowed.");
        }
    }
}