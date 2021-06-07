using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Piipan.Shared.Helpers;

namespace Piipan.Match.State
{

    /// <summary>
    /// JSON.NET converter for serializing/deserializing a DateTime
    /// object using our desired YYYY-MM-DD format.
    /// </summary>
    /// <remarks>
    /// Applied to model properties as `[JsonConverter(typeof(DateTimeConverter))]`
    /// </remarks>
    public class DateTimeConverter : IsoDateTimeConverter
    {
        public DateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }

    /// <summary>
    /// JSON.NET converter for deserializing month-only string to a DateTime
    /// to last day of the month
    /// and serializing a DateTime into a month-only string
    /// using our desired ISO 8601 YYYY-MM format.
    /// </summary>
    /// <remarks>
    /// Applied to model properties as `[JsonConverter(typeof(DateMonthConverter))]`
    /// </remarks>
    public class DateMonthConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;

		public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

		public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        ){
            if (String.IsNullOrEmpty((string)reader.Value)) return null;
            return MonthEndDateTime.Parse((string)reader.Value);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        ){
            writer.WriteValue(((DateTime)value).ToString("yyyy-MM"));
        }
    }

    /// <summary>
    /// JSON.NET converter used for converting null, missing, or empty
    /// properties to a `null` value when deserializing JSON.
    /// </summary>
    /// <remarks>
    /// Applied to model properties as `[JsonConverter(typeof(NullConverter))]`.
    ///
    /// Intended for use when deserializing JSON in incoming request bodies.
    /// Null values are needed for optional fields to properly perform exact
    /// matches when querying the state-level database.
    ///
    /// Matches the behavoir of `Piipan.Etl.BulkUpload` which writes missing
    /// or empty optional fields to the databse as `DbNull.Value`.
    /// </remarks>
    public class NullConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        ){
            return String.IsNullOrWhiteSpace((string)reader.Value) ? null : (string)reader.Value;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        ){
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// JSON.NET converter used for converting an array of DateTimes
    /// into an array of strings, each formatted as an ISO 8601 year and month
    /// </summary>
    public class DateMonthArrayConverter: JsonConverter
    {
		public override bool CanRead => false;
        public override bool CanWrite => true;

		public override bool CanConvert(Type objectType) => objectType == typeof(List<DateTime>);

		public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        ){
            throw new NotImplementedException();
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        ){
            var results = new List<string>();
            var dateList = (List<DateTime>)value;
            dateList.Sort((x, y) => y.CompareTo(x));
            writer.WriteStartArray();
            foreach (var date in dateList)
            {
                writer.WriteValue(date.ToString("yyyy-MM"));
            }
            writer.WriteEndArray();
        }
    }
}
