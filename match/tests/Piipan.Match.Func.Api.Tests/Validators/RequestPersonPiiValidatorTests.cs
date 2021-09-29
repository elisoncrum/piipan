using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using Xunit;

namespace Piipan.Match.Func.Api.Validators.Tests
{
    public class RequestPersonPiiValidatorTests
    {
        public RequestPersonPiiValidator Validator()
        {
            return new RequestPersonPiiValidator();
        }

        [Fact]
        public void ReturnsErrorWhenFirstEmpty()
        {
            var model = new RequestPersonWithPii()
            {
                First = "",
                Last = "last",
                Dob = DateTime.Now,
                Ssn = "000-00-0000"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.First);
        }

        [Fact]
        public void ReturnsErrorWhenLastEmpty()
        {
            var model = new RequestPersonWithPii()
            {
                First = "first",
                Last = "",
                Dob = DateTime.Now,
                Ssn = "000-00-0000"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.Last);
        }

        [Fact]
        public void ReturnsErrorWhenSsnMalformed()
        {
            var model = new RequestPersonWithPii()
            {
                First = "first",
                Last = "last",
                Dob = DateTime.Now,
                Ssn = "000-00-000"
            };
            var result = Validator().TestValidate(model);
            result.ShouldHaveValidationErrorFor(person => person.Ssn);
        }

        [Fact]
        public void ReturnsNoErrorsWhenWellFormed()
        {
            var model = new RequestPersonWithPii()
            {
                First = "first",
                Last = "last",
                Dob = DateTime.Now,
                Ssn = "000-00-0000"
            };
            var result = Validator().TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}