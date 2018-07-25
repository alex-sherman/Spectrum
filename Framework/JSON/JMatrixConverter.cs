using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace Spectrum.Framework.JSON
{
    class JMatrixConverter : SpectrumJsonConverter
    {
        public override object Read(JToken token, Type targetType)
        {
            if (token is JObject obj)
            {
                Vector3 rotation = Vector3.Zero;
                Vector3 scale = Vector3.One;
                Vector3 translation = Vector3.Zero;
                if (obj["rotation"] != null)
                    rotation = JConvert.Read<Vector3>(obj["rotation"]) * (float)(Math.PI / 180);
                if (obj["scale"] != null)
                {
                    if (obj["scale"].Type == JTokenType.Array)
                        scale = JConvert.Read<Vector3>(obj["scale"]);
                    if (obj["scale"].Type == JTokenType.Float)
                        scale = new Vector3((float)obj["scale"]);
                }
                if (obj["translation"] != null)
                    translation = JConvert.Read<Vector3>(obj["translation"]);
                return Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);
            }
            return null;
        }

        public override JToken Write(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
