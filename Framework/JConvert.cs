using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectrum.Framework.Content;
using Spectrum.Framework.Entities;
using Spectrum.Framework.Graphics;
using Spectrum.Framework.JSON;
using Spectrum.Framework.Physics.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public class JConvert
    {
        public static List<JsonConverter> Converters = new List<JsonConverter>();
        static JsonSerializerSettings Settings;
        static JConvert()
        {
            Converters.Add(new JVectorConverter());
            Converters.Add(new JPointConverter());
            Converters.Add(new JMatrixConverter());
            Converters.Add(new JShapeConverter());
            Converters.Add(new SimpleConverter<SpecModel>(
                model => model.Name,
                token => ContentHelper.Load<SpecModel>((string)token)));
            Converters.Add(new JInitDataConverter());
            Converters.Add(new SimpleConverter<TypeData>(
                typeData => typeData.Type.Name,
                token => TypeHelper.Types.GetData((string)token)));
            Settings = new JsonSerializerSettings();
            foreach (var converter in Converters)
            {
                Settings.Converters.Add(converter);
            }
        }
        public static object Deserialize(JToken token, Type targetType)
        {
            return token?.ToObject(targetType, JsonSerializer.Create(Settings));
        }
        public static T Deserialize<T>(JToken token)
        {
            return (T)(Deserialize(token, typeof(T)) ?? default(T));
        }
        public static T Deserialize<T>(string json)
        {
            return (T)JsonConvert.DeserializeObject(json, typeof(T), Settings);
        }
        public static T DeserializeFile<T>(string path)
        {
            using (var reader = new StreamReader(File.OpenRead(path)))
                return Deserialize<T>(reader.ReadToEnd());
        }
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
        }
        public static void SerializeFile(object obj, string path)
        {
            using (var f = File.Open(path, FileMode.OpenOrCreate))
            {
                f.SetLength(0);
                using (var sw = new StreamWriter(f))
                    sw.Write(Serialize(obj));
            }
        }
    }
}
