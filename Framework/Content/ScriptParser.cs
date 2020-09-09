using Spectrum.Framework.Content.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class ScriptParser : CachedContentParser<ScriptAsset>
    {
        public ScriptParser() : base("cs")
        {
            Prefix = "Scripts";
        }
        protected override ScriptAsset LoadData(string path, string name)
        {
            return new ScriptAsset(path, new CSScript(path));
        }
    }
}
