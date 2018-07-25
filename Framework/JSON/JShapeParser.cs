using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Physics.Collision.Shapes;

namespace Spectrum.Framework.JSON
{
    class JShapeParser : SpectrumJsonConverter
    {
        public override object Read(JToken token, Type targetType)
        {
            if (token is JObject obj)
            {
                var shapeType = (string)obj["type"];
                switch (shapeType)
                {
                    case "box":
                        return new BoxShape(JConvert.Read<Vector3>(obj["size"]),
                            JConvert.Read<Vector3?>(obj["position"]));
                    case "list":
                        return new ListMultishape(((JArray)obj["shapes"])
                            .Select(shape => JConvert.Read<Shape>(shape)).ToList());
                    default:
                        break;
                }
            }
            return null;
        }

        public override JToken Write(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
