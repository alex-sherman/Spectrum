using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
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
    public class IronPythonTypeWrapper
    {
        private readonly ScriptEngine _engine;
        private readonly ObjectHandle _classHandle;
        private ObjectHandle _instanceHandle;
        private object _instance;

        public IronPythonTypeWrapper(ScriptEngine engine, string name, PythonType pythonType, ObjectHandle classHandle)
        {
            Name = name;
            PythonType = pythonType;
            _engine = engine;
            _classHandle = classHandle;
        }

        public string Name { get; private set; }
        public PythonType PythonType { get; private set; }
        public Type Type { get { return PythonType; } }
        public object Activator()
        {
            _instanceHandle = _engine.Operations.Invoke(_classHandle, new object[] { });
            _instance = _engine.Operations.Unwrap<object>(_instanceHandle);
            return _instance;
        }

        public void InvokeMethodWithArgument(string methodName, object argument)
        {
            _engine.Operations.InvokeMember(_instance, methodName, argument);
        }
    }
    public class FunctionWrapper
    {
        private readonly ScriptEngine _engine;
        private readonly ObjectHandle _classHandle;
        public PythonFunction Function { get; private set; }
        public string Name { get; private set; }
        public FunctionWrapper(ScriptEngine engine, string name, PythonFunction function, ObjectHandle classHandle)
        {
            Function = function;
            Name = name;
            _engine = engine;
            _classHandle = classHandle;
        }
        public object Invoke(params object[] args)
        {
            return _engine.Operations.Invoke(_classHandle, args);
        }
    }
    public class ScriptAsset
    {
        private ScriptEngine engine = Python.CreateEngine();
        private ScriptScope scope;
        private ScriptSource source;
        public List<IronPythonTypeWrapper> Types;
        public Dictionary<string, FunctionWrapper> Functions;
        public Dictionary<string, object> variables = new Dictionary<string, object>();
        public Plugin Plugin { get; private set; }
        public string Path { get; private set; }
        public ScriptAsset(string path, string source_text)
        {
            Path = path;
            scope = engine.CreateScope();
            ObjectOperations op = engine.Operations;
            source = engine.CreateScriptSourceFromString(source_text);
            CompiledCode code = source.Compile();
            code.Execute(scope);
            Types = scope.GetItems()
                .Where(kvp => kvp.Value is PythonType && !kvp.Key.StartsWith("__"))
                .Select(kvp => new IronPythonTypeWrapper(engine, kvp.Key, kvp.Value, scope.GetVariableHandle(kvp.Key)))
                .ToList();
            Functions = scope.GetItems()
                .Where(kvp => kvp.Value is PythonFunction && !kvp.Key.StartsWith("__"))
                .ToDictionary(kvp => kvp.Key, kvp => new FunctionWrapper(engine, kvp.Key, kvp.Value, scope.GetVariableHandle(kvp.Key)));
        }
        public void SetVariable(string name, object value)
        {
            scope.SetVariable(name, value);
        }
        public bool HasFunction(string function)
        {
            try
            {
                scope.GetVariable<Func<object[], object>>(function);
                return true;
            }
            catch { }
            return false;
        }
        public object TryCall(string function, params object[] args)
        {
            try
            {
                using (DebugTiming.Scripts.Time(Path))
                {
                    Func<object[], object> f = scope.GetVariable<Func<object[], object>>(function);
                    object o = f(args);
                    return o;
                }
            }
            catch (Exception e)
            {
                DebugPrinter.print(e.Message);
                return null;
            }

        }
    }
}
