﻿using System;
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
                        return new BoxShape(JConvert.Parse<Vector3>(obj["size"]),
                            JConvert.Parse<Vector3?>(obj["position"]));
                    case "list":
                        return new ListMultishape(((JArray)obj["shapes"])
                            .Select(shape => JConvert.Parse<Shape>(shape)).ToList());
                    default:
                        break;
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is BoxShape box)
                new JObject() {
                    { "type", "box" },
                    { "size", JToken.FromObject(box.Size, serializer) }
                }.WriteTo(writer);
            else if (value is ListMultishape listShape)
            {
                new JObject() {
                    { "type", "list" },
                    { "shapes", new JArray(listShape.Shapes.Select(shape => JToken.FromObject(shape, serializer))) }
                }.WriteTo(writer);
            }
            else
                throw new NotImplementedException();
        }
    }
}
