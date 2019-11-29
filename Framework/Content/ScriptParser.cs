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
        public ScriptParser() : base("py")
        {
            Prefix = "Scripts";
        }
        protected override ScriptAsset LoadData(string path, string name)
        {
            StreamReader reader = new StreamReader(path);
            var scriptString = reader.ReadToEnd();
            IScript script = null;
            //if (Path.GetExtension(path) == ".js")
            //    script = new JSScript(scriptString);
            if (Path.GetExtension(path) == ".py")
                script = new PyScript(scriptString);
            return new ScriptAsset(path, script);
        }
    }
}
