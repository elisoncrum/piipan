using FluentValidation;

namespace Piipan.Match.Orchestrator
{
    public class MatchQueryRequestValidator : AbstractValidator<MatchQueryRequest>
    {
        public MatchQueryRequestValidator()
        {
            RuleForEach(m => m.Query).SetValidator(new MatchQueryValidator());
        }
    }

    public class MatchQueryValidator : AbstractValidator<MatchQuery>
    {
        public MatchQueryValidator()
        {
            RuleFor(q => q.First).NotEmpty();
            RuleFor(q => q.Last).NotEmpty();
            RuleFor(q => q.Ssn).Matches(@"^[0-9]{3}-[0-9]{2}-[0-9]{4}$");
        }
    }
}
