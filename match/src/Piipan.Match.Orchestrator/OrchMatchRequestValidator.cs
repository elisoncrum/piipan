using FluentValidation;

namespace Piipan.Match.Orchestrator
{
    /// <summary>
    /// Validates the whole API match request from a client
    /// </summary>
    public class OrchMatchRequestValidator : AbstractValidator<OrchMatchRequest>
    {
        public OrchMatchRequestValidator()
        {
            RuleForEach(m => m.Persons).SetValidator(new PersonValidator());
        }
    }

    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class PersonValidator : AbstractValidator<RequestPerson>
    {
        public PersonValidator()
        {
            RuleFor(q => q.First).NotEmpty();
            RuleFor(q => q.Last).NotEmpty();
            RuleFor(q => q.Ssn).Matches(@"^[0-9]{3}-[0-9]{2}-[0-9]{4}$");
        }
    }
}
