using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class InitDataParser : CachedContentParser<InitData, InitData>
    {
        public InitDataParser()
        {
            Prefix = "InitData";
        }
        protected override InitData LoadData(string path, string name)
        {
            try
            {
                Dictionary<string, JToken> jobj;
                using (JsonTextReader reader = new JsonTextReader(new StreamReader(Path.Combine(path))))
                    jobj = ((IEnumerable<KeyValuePair<string, JToken>>)JObject.Load(reader)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                InitData output = new InitData((string)jobj["@TypeName"]);
                output.Name = (string)jobj["@Name"];
                ParseCalls(jobj, true, output);
                ParseCalls(jobj, false, output);
                jobj.Remove("@Name"); jobj.Remove("@TypeName");
                jobj.Remove("@Call"); jobj.Remove("@CallOnce");

                foreach (var kvp in jobj)
                {
                    if (output.TypeData.members.TryGetValue(kvp.Key, out MemberInfo memberInfo))
                        output.Set(kvp.Key, ParseValue(kvp.Value, memberInfo.MemberType));
                    else
                        DebugPrinter.PrintOnce("Skipping field {0} in {1}", kvp.Key, name);
                }
                return output;
            }
            catch (Exception e)
            {
                DebugPrinter.PrintOnce("Failed to parse init data: {0}\n{1}", path, e);
            }
            return null;
        }

        static void ParseCalls(Dictionary<string, JToken> jobj, bool callOnce, InitData output)
        {
            string key = callOnce ? "@CallOnce" : "@Call";
            if (jobj.TryGetValue(key, out JToken calls))
            {
                if (calls.Type != JTokenType.Object)
                    throw new InvalidDataException(string.Format("Expected field {0} to be an object", key));
                JObject callsObj = (JObject)calls;
                foreach (var call in callsObj)
                {
                    if (call.Value.Type != JTokenType.Array)
                       throw new InvalidDataException(string.Format("Expected field {0}.{1} to be an array", key, call.Key));
                    var callArgs = ((JArray)call.Value).Select(callArg => ParseValue(callArg, null)).ToArray();
                    if (callOnce)
                        output.CallOnce(call.Key, args: callArgs);
                    else
                        output.Call(call.Key, args: callArgs);
                }
            }
        }

        public static object ParseValue(JToken token, Type targetType)
        {
            if (token.Type == JTokenType.String)
                return (string)token;
            switch (token.Type)
            {
                case JTokenType.Undefined:
                case JTokenType.Null:
                case JTokenType.None:
                    return null;
                case JTokenType.Object:
                    return ParseObject((JObject)token, targetType);
                case JTokenType.Array:
                    break;
                case JTokenType.Integer:
                    return (int)token;
                case JTokenType.Float:
                    return (float)token;
                case JTokenType.Uri:
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Boolean:
                    return (bool)token;
                case JTokenType.Guid:
                    return (Guid)token;

                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Bytes:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.TimeSpan:
                default:
                    throw new InvalidDataException(string.Format("Unexpected field type in prefab JSON {0}", token.Type.ToString()));
            }
            return null;
        }

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

        protected override InitData SafeCopy(InitData data)
        {
            return data;
        }
    }
}
