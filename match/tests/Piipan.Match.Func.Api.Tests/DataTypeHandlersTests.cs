using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Piipan.Match.Func.Api.DataTypeHandlers.Tests
{
    public class DataTypeHandlersTests
    {
        [Fact]
        public void ParseThrowsForBadInput()
        {
            // Arrange 
            var handler = new DateTimeListHandler();

            // Act / Assert
            Assert.Throws<InvalidCastException>(() => handler.Parse("not a DateTime[]"));
        }

        [Fact]
        public void ParseReturnsList()
        {
            // Arrange
            var handler = new DateTimeListHandler();

            // Act
            var response = handler.Parse(new DateTime[] {
                DateTime.Now,
                DateTime.Now
            });

            Assert.NotNull(response);
            Assert.IsType<List<DateTime>>(response);
            Assert.Equal(2, response.Count());
        }

        [Fact]
        public void SetValueSetsParameterValue()
        {
            // Arrange
            var handler = new DateTimeListHandler();
            var parameter = new Mock<IDbDataParameter>();

            // Act
            handler.SetValue(parameter.Object, new List<DateTime> { DateTime.Now });

            // Assert
            parameter.VerifySet(m => m.Value = It.IsAny<List<DateTime>>(), Times.Once);
        }
    }
}