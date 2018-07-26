using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Spectrum.Framework.JSON
{
    class JMatrixConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Matrix);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            if (token is JArray array)
            {
                return MatrixHelper.FromArray(array.Select(_token => (float)_token).ToArray());
            }
            if (token is JObject obj)
            {
                Vector3 rotation = Vector3.Zero;
                Vector3 scale = Vector3.One;
                Vector3 translation = Vector3.Zero;
                if (obj["rotation"] != null)
                    rotation = JConvert.Deserialize<Vector3>(obj["rotation"]) * (float)(Math.PI / 180);
                if (obj["scale"] != null)
                {
                    if (obj["scale"].Type == JTokenType.Array)
                        scale = JConvert.Deserialize<Vector3>(obj["scale"]);
                    if (obj["scale"].Type == JTokenType.Float)
                        scale = new Vector3((float)obj["scale"]);
                }
                if (obj["translation"] != null)
                    translation = JConvert.Deserialize<Vector3>(obj["translation"]);
                return Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.Formatting = Formatting.None;
            serializer.Formatting = Formatting.None;
            serializer.Serialize(writer, ((Matrix)value).ToArray());
            serializer.Formatting = Formatting.Indented;
            writer.Formatting = Formatting.Indented;
        }
    }
}
