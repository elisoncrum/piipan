using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Piipan.Match.State {
    public class DateTimeConverter : IsoDateTimeConverter
    {
        public DateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-dd";
        }
    }
    
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
