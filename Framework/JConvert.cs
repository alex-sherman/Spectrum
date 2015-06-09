using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public class JConvert
    {
        private static Vector3 ToVector3(JToken token)
        {
            return (new Vector3((float)((JArray)token)[0], (float)((JArray)token)[1], (float)((JArray)token)[2]));
        }

        public static T To<T>(JToken token)
        {
            if (typeof(T) == typeof(Vector3))
                return (T)(object)ToVector3(token);
            throw new JsonSerializationException("No conversion possible");
        }
    }
}
