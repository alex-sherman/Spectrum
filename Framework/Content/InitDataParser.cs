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
            {
                var output = JConvert.Deserialize<InitData>(reader.ReadToEnd()).ToImmutable();
                if (output.Name == null) output.Name = name;
                output.Path = name;
                output.FullPath = path;
                return output;
            }
        }

        protected override InitData SafeCopy(InitData data)
        {
            return data;
        }
    }
}
