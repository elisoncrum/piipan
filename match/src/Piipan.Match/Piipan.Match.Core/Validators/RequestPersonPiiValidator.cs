using Piipan.Match.Core.Models;
using FluentValidation;

namespace Piipan.Match.Core.Validators
{
    /// <summary>
    /// Validates each person in an API request
    /// </summary>
    public class RequestPersonPiiValidator : AbstractValidator<RequestPersonWithPii>
    {
        public RequestPersonPiiValidator()
        {
            RuleFor(q => q.First).NotEmpty();
            RuleFor(q => q.Last).NotEmpty();
            RuleFor(q => q.Ssn).Matches(@"^[0-9]{3}-[0-9]{2}-[0-9]{4}$");
        }
    }
}