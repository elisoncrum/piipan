using System.Collections.Generic;
using FluentValidation.TestHelper;
using Piipan.Match.Orchestrator;
using Xunit;

public class OrchMatchRequestValidatorTests
{
    public OrchMatchRequestValidator Validator()
    {
        return new OrchMatchRequestValidator();
    }

    [Fact]
    public void ReturnsErrorWhenDataEmpty()
    {
        // Setup
        var model = new OrchMatchRequest()
        {
            Data = new List<RequestPerson>()
        };
        // Act
        var result = Validator().TestValidate(model);
        // Assert
        result.ShouldHaveValidationErrorFor(result => result.Data);
    }

    [Fact]
    public void ReturnsErrorWhenDataOverMax()
    {
        // Setup
        var list = new List<RequestPerson>();
        for (int i = 0; i < 51; i++)
        {
            list.Add(new RequestPerson
            {
                LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
            });
        }
        var model = new OrchMatchRequest { Data = list };
        // Act
        var result = Validator().TestValidate(model);
        // Assert
        result.ShouldHaveValidationErrorFor(result => result.Data);
    }
}

public class PersonValidatorTests
{
    public PersonValidator Validator()
    {
        return new PersonValidator();
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
