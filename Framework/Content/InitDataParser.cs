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
                    {
                        try
                        {
                            output.Set(kvp.Key, ParseValue(kvp.Value, memberInfo.MemberType));
                        }
                        catch (Exception)
                        {
                            DebugPrinter.PrintOnce($"Failed to parse init data value for {name}.{kvp.Key}: {kvp.Value.ToString()} -> {memberInfo.MemberType}".Replace("\n", "").Replace("\r", ""));
                        }
                    }
                    else
                        DebugPrinter.PrintOnce($"Skipping field {kvp.Key} in {name}");
                }
                return output;
            }
            catch (Exception e)
            {
                DebugPrinter.PrintOnce($"Failed to parse init data: {path}\n{e}");
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
                case JTokenType.Array:
                    return JConvert.Read(token, targetType);
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

        protected override InitData SafeCopy(InitData data)
        {
            return data;
        }
    }
}
