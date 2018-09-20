using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Physics.Collision.Shapes;

namespace Spectrum.Framework.JSON
{
    class JShapeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Shape) || objectType == typeof(BoxShape) || objectType == typeof(ListMultishape);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            if (token is JObject obj)
            {
                var shapeType = (string)obj["type"];
                switch (shapeType)
                {
                    case "box":
                        return new BoxShape(JConvert.Deserialize<Vector3>(obj["size"]),
                            JConvert.Deserialize<Vector3?>(obj["position"]));
                    case "list":
                        return new ListMultishape(((JArray)obj["shapes"])
                            .Select(shape => JConvert.Deserialize<Shape>(shape)).ToList());
                    default:
                        break;
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is BoxShape box)
                serializer.Serialize(writer, new
                {
                    type = "box",
                    size = box.Size,
                    position = box.Position,
                });
            else if (value is ListMultishape listShape)
            {
                serializer.Serialize(writer, new
                {
                    type = "list",
                    shapes = listShape.Shapes
                });
            }
            else
                throw new NotImplementedException();
        }
    }
}
