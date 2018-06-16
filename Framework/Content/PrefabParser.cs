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
    public class PrefabParser : CachedContentParser<Prefab, Prefab>
    {
        public PrefabParser()
        {
            Prefix = "Prefabs";
        }
        protected override Prefab LoadData(string path, string name)
        {
            try
            {
                using (JsonTextReader reader = new JsonTextReader(new StreamReader(Path.Combine(path))))
                {
                    var jobj = JObject.Load(reader);
                }
            }
            catch (Exception e)
            {
                DebugPrinter.PrintOnce("Failed to parse prefab: {0}\n{1}", path, e);
            }
            return null;
        }

        protected override Prefab SafeCopy(Prefab data)
        {
            return data;
        }
    }
}
