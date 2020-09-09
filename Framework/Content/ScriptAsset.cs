using Spectrum.Framework.Content.Scripting;
using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class ScriptAsset
    {
        public readonly CSScript Script;
        public string Path { get; private set; }
        public ScriptAsset(string path, CSScript script)
        {
            Path = path;
            Script = script;
        }
        public static implicit operator Component(ScriptAsset script)
        {
            return script.Script.GetComponent();
        }
    }
}
