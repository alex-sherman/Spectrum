using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content.Scripting
{
    public class PyScript : IScript
    {
        private readonly ScriptEngine engine;
        public Dictionary<string, ScriptFunction> Functions = new Dictionary<string, ScriptFunction>();
        private readonly ScriptScope scope;

        public PyScript(string script)
        {
            engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(typeof(Vector2).Assembly);
            engine.Runtime.LoadAssembly(typeof(PyScript).Assembly);
            scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromString(script);
            engine.Execute(script, scope);
            Functions = scope.GetItems()
                .Where(kvp => kvp.Value is PythonFunction && !kvp.Key.StartsWith("__"))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ScriptFunction(args => InvokeMethod((object)kvp.Value, args)));
        }

        public string Name { get; private set; }

        public void AddObject(string name, object value)
        {
            scope.SetVariable(name, value);
        }

        public ScriptFunction GetFunction(string name)
        {
            if (Functions.TryGetValue(name, out var func)) return func;
            return null;
        }

        public object InvokeMethod(object method, params object[] args)
        {
            try
            {
                return (object)engine.Operations.Invoke(method, args);
            }
            catch(Exception e)
            {
                DebugPrinter.PrintOnce(e.Message);
            }
            return null;
        }
    }
}
