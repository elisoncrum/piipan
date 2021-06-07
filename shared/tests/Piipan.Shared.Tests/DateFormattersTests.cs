using System;
using System.Collections.Generic;
using Xunit;
using Piipan.Shared.Helpers;

namespace Piipan.Shared.Tests
{
	public class DateFormattersTests
    {
		[Fact]
        public void FormatDatesAsPgArrayReturnsString()
        {
            var empty = new List<DateTime>();
            Assert.Equal("{}", DateFormatters.FormatDatesAsPgArray(empty));
            var singleDate = new List<DateTime>(){ new DateTime(2021, 5, 1) };
            Assert.Equal("{2021-05-01}", DateFormatters.FormatDatesAsPgArray(singleDate));
            var multiDates = new List<DateTime>(){
                new DateTime(2021, 4, 1),
                new DateTime(2021, 5, 1)
            };
            Assert.Equal("{2021-05-01,2021-04-01}", DateFormatters.FormatDatesAsPgArray(multiDates));
        }
	}
}
