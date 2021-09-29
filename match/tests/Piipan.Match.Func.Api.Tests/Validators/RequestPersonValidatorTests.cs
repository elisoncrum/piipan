using FluentValidation.TestHelper;
using Xunit;

namespace Piipan.Match.Func.Api.Validators.Tests
{
    public class RequestPersonValidatorTests
    {
        public RequestPersonValidator Validator()
        {
            return new RequestPersonValidator();
        }

        [Fact]
        public void ReturnsErrorWhenHashEmpty()
        {
            var model = new RequestPerson()
            {
                LdsHash = ""
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.LdsHash);
        }

        [Fact]
        public void ReturnsErrorWhenHashMalformed()
        {
            var model = new RequestPerson()
            {
                LdsHash = "Foo"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.LdsHash);
        }
    }
}