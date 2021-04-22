using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Piipan.QueryTool.Binders;
using Xunit;

namespace Piipan.QueryTool.Tests
{
    public class TrimBinderTests
    {
        [Theory]
        [InlineData(" A ")]
        [InlineData("\u00A0A")]
        public async Task TrimBinderTrimsStrings(string incoming)
        {
            // Arrange
            var binder = new TrimModelBinder();
            var formCollection = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    { "someString", new StringValues(incoming) }
                });
            var vp = new FormValueProvider(BindingSource.Form, formCollection, CultureInfo.CurrentCulture);
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(Type.GetType("System.String")),
                ModelName = "someString",
                ModelState = new ModelStateDictionary(),
                ValueProvider = vp,
            };

            // Act
            await binder.BindModelAsync(bindingContext);
            var resultString = bindingContext.Result.Model as string;

            // Assert
            Assert.Equal(incoming.Trim(), resultString);
        }
    }
}
