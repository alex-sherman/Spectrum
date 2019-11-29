using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.Scripting
{
    public delegate object ScriptFunction(params object[] args);
    public interface IScript
    {
        void AddObject(string name, object value);
        ScriptFunction GetFunction(string name);

    }
}
