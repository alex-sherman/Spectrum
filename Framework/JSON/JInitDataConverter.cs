using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Replicate;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Network.Surrogates;

namespace Spectrum.Framework.JSON
{
    class JInitDataConverter : JsonConverter
    {

        static void ParseCalls(Dictionary<string, JToken> jobj, bool callOnce, InitData output)
        {
            string key = callOnce ? "@CallOnce" : "@Call";
            if (jobj.TryGetValue(key, out JToken calls))
            {
                if (calls.Type != JTokenType.Array)
                    throw new InvalidDataException($"Expected field {key} to be an object");
                JArray callsArray = (JArray)calls;
                foreach (var callEntry in callsArray)
                {
                    if (!(callEntry is JObject callJObj) || callJObj.Count > 1)
                        throw new InvalidDataException($"Expected field {callEntry}.{key} to be an object with one key/value");
                    var call = ((IEnumerable<KeyValuePair<string, JToken>>)callJObj).First();
                    var callArgs = ((JArray)call.Value).Select(callArg => ParseValue(callArg, null)).ToArray();
                    if (callOnce)
                        output.CallOnce(call.Key, args: callArgs);
                    else
                        output.Call(call.Key, args: callArgs);
                }
            }
        }

        static void ParseData(Dictionary<string, JToken> jobj, InitData output)
        {
            if (jobj.TryGetValue("@Data", out var datas))
            {
                if (datas.Type != JTokenType.Object)
                    throw new InvalidDataException(string.Format("Expected field @Dict to be an object"));
                output.Data = JConvert.Deserialize<Dictionary<string, Primitive>>(datas);
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
                case JTokenType.Array:
                    return JConvert.Deserialize(token, targetType);
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
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var init = (InitData)value;
            var jobj = new Dictionary<string, object>()
            {
                { "@Name", init.Name },
                { "@TypeName", init.TypeName },
                { "@Data", init.Data },
            };
            foreach (var kvp in init.Fields)
            {
                jobj[kvp.Key] = kvp.Value.Object;
            }
            serializer.Serialize(writer, jobj);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.Null)
                return null;
            Dictionary<string, JToken> jobj =
                ((IEnumerable<KeyValuePair<string, JToken>>)token).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            InitData output;
            if (objectType == typeof(InitData))
                output = new InitData((string)jobj["@TypeName"]);
            else
                output = (InitData)objectType.GetConstructor(new Type[] { typeof(object[]) })
                    .Invoke(new object[] { new object[] { } });
            if (jobj.ContainsKey("@Name")) output.Name = (string)jobj["@Name"];
            ParseCalls(jobj, true, output);
            ParseCalls(jobj, false, output);
            ParseData(jobj, output);
            jobj.Remove("@Name"); jobj.Remove("@TypeName");
            jobj.Remove("@Call"); jobj.Remove("@CallOnce");
            jobj.Remove("@Dict");

            foreach (var kvp in jobj)
            {
                if (output.TypeData.Members.TryGetValue(kvp.Key, out var member))
                {
                    try
                    {
                        output.Set(kvp.Key, ParseValue(kvp.Value, member.Type));
                    }
                    catch (Exception)
                    {
                        DebugPrinter.PrintOnce($"Failed to parse init data value for {output.Name}.{kvp.Key}: {kvp.Value.ToString()} -> {member.Type}".Replace("\n", "").Replace("\r", ""));
                    }
                }
                else
                    DebugPrinter.PrintOnce($"Skipping field {kvp.Key} in {output.Name}");
            }
            return output;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InitData) || objectType.IsSameGeneric(typeof(InitData<>));
        }
    }
}
