using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    /// JSON.NET converter for serializing/deserializing a DateTime
    /// object using our desired YYYY-MM format.
    /// </summary>
    /// <remarks>
    /// Applied to model properties as `[JsonConverter(typeof(DateMonthConverter))]`
    /// </remarks>
    public class DateMonthConverter : IsoDateTimeConverter
    {
        public DateMonthConverter()
        {
            base.DateTimeFormat = "yyyy-MM";
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

        public override object ReadJson(JsonReader reader, Type objectType,
                                    object existingValue, JsonSerializer serializer)
        {
            return String.IsNullOrWhiteSpace((string)reader.Value) ? null : (string)reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                   JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
