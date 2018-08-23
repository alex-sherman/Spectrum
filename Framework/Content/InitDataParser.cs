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
        public InitDataParser() : base("json")
        {
            Prefix = "InitData";
        }
        protected override InitData LoadData(string path, string name)
        {
            using (var reader = new StreamReader(File.OpenRead(path)))
                return JConvert.Deserialize<InitData>(reader.ReadToEnd()).ToImmutable();
        }

        protected override InitData SafeCopy(InitData data)
        {
            return data;
        }
    }
}
