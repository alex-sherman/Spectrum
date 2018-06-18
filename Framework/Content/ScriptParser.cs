using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class ScriptParser : CachedContentParser<ScriptAsset, ScriptAsset>
    {
        public ScriptParser()
        {
            Prefix = "Scripts";
        }
        protected override ScriptAsset LoadData(string path, string name)
        {
            path = TryThrowExtensions(path, ".py");
            StreamReader reader = new StreamReader(path);
            return new ScriptAsset(path, reader.ReadToEnd());
        }

        protected override ScriptAsset SafeCopy(ScriptAsset data)
        {
            return data;
        }
    }
}
