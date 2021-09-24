using FluentValidation;

namespace Piipan.Match.Func.Api
{
    /// <summary>
    /// Validates the whole API match request from a client
    /// </summary>
    public class OrchMatchRequestValidator : AbstractValidator<OrchMatchRequest>
    {
        private readonly int MaxPersonsInRequest = 50;
        public OrchMatchRequestValidator()
        {
            RuleFor(r => r.Data)
                .NotNull()
                .NotEmpty()
                .Must(data => data.Count <= MaxPersonsInRequest)
                .WithMessage($"Data count cannot exceed {MaxPersonsInRequest}");
        }
    }

    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class PersonValidator : AbstractValidator<RequestPerson>
    {
        public PersonValidator()
        {
            const string HashRegex = "^[a-z0-9]{128}$";

            RuleFor(q => q.LdsHash).Matches(HashRegex);
        }
    }

    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class PersonWithPiiValidator : AbstractValidator<RequestPersonWithPii>
    {
        public PersonWithPiiValidator()
        {
            RuleFor(q => q.First).NotEmpty();
            RuleFor(q => q.Last).NotEmpty();
            RuleFor(q => q.Ssn).Matches(@"^[0-9]{3}-[0-9]{2}-[0-9]{4}$");
        }
    }
}
