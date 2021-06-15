using System;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Piipan.Shared.Helpers;

namespace Piipan.Etl
{
    /// <summary>
    /// Converts month-only date to last day of month when as a DateTime
    /// and to ISO 8601 year-months when as a string
    /// </summary>
	public class ToMonthEndConverter : DefaultTypeConverter
	{
		public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
		{
			if (String.IsNullOrEmpty(text)) return null;
            return MonthEndDateTime.Parse(text);
		}
		public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
      	{
			return ((DateTime?)value).HasValue
				? ((DateTime)value).ToString("yyyy-MM")
				: string.Empty;
      	}
	}
    /// <summary>
    /// Converts list of month-only dates to last day of month when as DateTimes
    /// and to ISO 8601 year-months when as a string
    /// </summary>
  	public class ToMonthEndArrayConverter : DefaultTypeConverter
  	{
      	public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
      	{
			if (text == "") return new List<DateTime>();
			string[] allElements = text.Split(' ');
			DateTime[] elementsAsDateTimes = allElements.Select(s => MonthEndDateTime.Parse(s)).ToArray();
			return new List<DateTime>(elementsAsDateTimes);
      	}

      	public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
      	{
          	return string.Join(" ", ((List<DateTime>)value).ToArray());
      	}
  	}
}
