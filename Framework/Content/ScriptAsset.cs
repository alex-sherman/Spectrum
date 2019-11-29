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
    //public class FunctionWrapper
    //{
    //    private readonly ScriptEngine _engine;
    //    private readonly ObjectHandle _classHandle;
    //    public PythonFunction Function { get; private set; }
    //    public string Name { get; private set; }
    //    public FunctionWrapper(ScriptEngine engine, string name, PythonFunction function, ObjectHandle classHandle)
    //    {
    //        Function = function;
    //        Name = name;
    //        _engine = engine;
    //        _classHandle = classHandle;
    //    }
    //    public object Invoke(params object[] args)
    //    {
    //        return _engine.Operations.Invoke(_classHandle, args);
    //    }
    //}
    //public ScriptAsset(string path, string source_text)
    //{
    //    Path = path;
    //    scope = engine.CreateScope();
    //    ObjectOperations op = engine.Operations;
    //    source = engine.CreateScriptSourceFromString(source_text);
    //    CompiledCode code = source.Compile();
    //    code.Execute(scope);
    //    Types = scope.GetItems()
    //        .Where(kvp => kvp.Value is PythonType && !kvp.Key.StartsWith("__"))
    //        .Select(kvp => new IronPythonTypeWrapper(engine, kvp.Key, kvp.Value, scope.GetVariableHandle(kvp.Key)))
    //        .ToList();
    //}
    public class ScriptAsset
    {
        //public List<IronPythonTypeWrapper> Types;
        public readonly IScript Script;
        public Dictionary<string, object> variables = new Dictionary<string, object>();
        public string Path { get; private set; }
        public ScriptAsset(string path, IScript script)
        {
            Path = path;
            Script = script;
        }
        public static implicit operator Component(ScriptAsset script)
        {
            return new ScriptComponent(script.Script);
        }
        public object TryCall(string function, params object[] args)
        {
            try
            {
                ScriptFunction func = Script.GetFunction(function);
                if (func != null)
                    using (DebugTiming.Scripts.Time(Path))
                        return func(args);
                else
                    DebugPrinter.Print($"No function {function} in script {Path}");
            }
            catch (Exception e)
            {
                DebugPrinter.Print(e.Message);
            }
            return null;
        }
    }
}
