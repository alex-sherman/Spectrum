using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public class JConvert
    {
        private static Dictionary<string, Type> typeLookup = new Dictionary<string, Type>()
        {
            { "matrix", typeof(Matrix) },
            { "vector", typeof(Vector3) },
            { "vector2", typeof(Vector2) },
            { "vector3", typeof(Vector3) },
            { "vector4", typeof(Vector4) },
        };
        private static HashSet<Type> validTypes = new HashSet<Type>(typeLookup.Values);
        public static object ParseObject(JObject obj, Type targetType)
        {
            string typeName = (string)obj["type"];
            Type type = null;
            if (typeName != null && typeLookup.ContainsKey(typeName))
                type = typeLookup[typeName];
            else if (targetType != null && validTypes.Contains(targetType))
                type = targetType;
            else
                throw new InvalidDataException(string.Format("Unable to parse an object field"));

            if (type == typeof(Matrix))
            {
                Vector3 rotation = Vector3.Zero;
                Vector3 scale = Vector3.One;
                Vector3 translation = Vector3.Zero;
                if (obj["rotation"] != null)
                    rotation = JConvert.To<Vector3>(obj["rotation"]) * (float)(Math.PI / 180);
                if (obj["scale"] != null)
                {
                    if (obj["scale"].Type == JTokenType.Array)
                        scale = JConvert.To<Vector3>(obj["scale"]);
                    if (obj["scale"].Type == JTokenType.Float)
                        scale = new Vector3((float)obj["scale"]);
                }
                if (obj["translation"] != null)
                    translation = JConvert.To<Vector3>(obj["translation"]);
                return Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(translation);
            }
            return null;
        }
        public static bool IsNumeric(JToken token)
        {
            return token.Type == JTokenType.Float || token.Type == JTokenType.Integer;
        }
        public static object ParseArray(JArray array, Type targetType)
        {
            if (targetType == typeof(Vector3) && array.Count == 3 && array.All(IsNumeric))
                return new Vector3((float)array[0], (float)array[1], (float)array[2]);
            if (targetType == typeof(Vector2) && array.Count == 2 && array.All(IsNumeric))
                return new Vector2((float)array[0], (float)array[1]);
            if (targetType == typeof(Vector4) && array.Count == 4 && array.All(IsNumeric))
                return new Vector4((float)array[0], (float)array[1], (float)array[2], (float)array[3]);
            throw new JsonSerializationException("No conversion possible");
        }
        private static Vector3 ToVector3(JToken token)
        {
            return (new Vector3((float)((JArray)token)[0], (float)((JArray)token)[1], (float)((JArray)token)[2]));
        }
        public static object To(JToken token, Type targetType)
        {
            if (token.Type == JTokenType.Array)
                return ParseArray((JArray)token, targetType);
            if (token.Type == JTokenType.Object)
                return ParseObject((JObject)token, targetType);
            throw new JsonSerializationException("No conversion possible");
        }
        public static T To<T>(JToken token)
        {
            return (T)To(token, typeof(T));
        }
    }
}
