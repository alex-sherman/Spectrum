using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spectrum.Framework.JSON
{
    class SimpleConverter<T> : JsonConverter
    {
        readonly Func<T, JToken> Writer;
        readonly Func<JToken, T> Reader;
        public SimpleConverter(Func<T, JToken> writer, Func<JToken, T> reader)
        {
            Writer = writer;
            Reader = reader;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Reader(JToken.ReadFrom(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Writer((T)value).WriteTo(writer);
        }
    }
}
