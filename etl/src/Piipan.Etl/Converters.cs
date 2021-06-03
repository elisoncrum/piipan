using System;
using System.Linq;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Piipan.Etl
{
  public class ToDateTimeArrayConverter : DefaultTypeConverter
  {
      public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
      {
          if (text == "") return new List<DateTime>();
          string[] allElements = text.Split(' ');
          DateTime[] elementsAsDateTimes = allElements.Select(s => DateTime.Parse(s)).ToArray();
          return new List<DateTime>(elementsAsDateTimes);
      }

      public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
      {
          return string.Join(" ", ((List<DateTime>)value).ToArray());
      }
  }
}
