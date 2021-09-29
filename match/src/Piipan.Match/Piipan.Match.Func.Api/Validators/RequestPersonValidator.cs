using FluentValidation;

namespace Piipan.Match.Func.Api.Validators
{
    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class RequestPersonValidator : AbstractValidator<RequestPerson>
    {
        public RequestPersonValidator()
        {
            const string HashRegex = "^[a-z0-9]{128}$";

            RuleFor(q => q.LdsHash).Matches(HashRegex);
        }
    }
}