//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using V8.Net;

//namespace Spectrum.Framework.Content.Scripting
//{
//    public class JSScript : IScript
//    {
//        V8Engine engine = new V8Engine();
//        public JSScript(string script)
//        {
//            engine.Execute(script);
//            var derp = engine.GlobalObject.GetPropertyNames();
//            var objs = engine.GetObjects();
//        }
//        private Dictionary<string, ScriptFunction> functions = new Dictionary<string, ScriptFunction>();
//        public void AddObject(string name, object value)
//        {
//            engine.GlobalObject.SetProperty(name, value, memberSecurity: ScriptMemberSecurity.ReadWrite);
//        }
//        private object Call(InternalHandle prop, object[] args)
//        {
//            var result = prop.Call(null);
//            if (result.IsError)
//            {
//                DebugPrinter.PrintOnce(result.Value as string);
//            }
//            return null;
//        }
//        public ScriptFunction GetFunction(string name)
//        {
//            if (!functions.ContainsKey(name))
//            {
//                var prop = engine.GlobalObject.GetProperty(name);
//                if (!prop.IsFunction)
//                    functions[name] = null;
//                else
//                {
//                    functions[name] = args => Call(prop, args);
//                }
//            }
//            return functions[name];
//        }
//    }
//}
