using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework;
using System.Reflection;
using System.IO;
using Spectrum.Framework.Network;
using Spectrum.Framework.Content;
using IronPython.Runtime.Types;

namespace Spectrum.Framework
{
    public class TypeHelper
    {
        public static TypeHelper Types = new TypeHelper();
        private static DefaultDict<string, TypeData> types = new DefaultDict<string, TypeData>();
        private static DefaultDict<Type, Plugin> plugins = new DefaultDict<Type, Plugin>();
        public List<String> GetTypes() { return types.Keys.ToList(); }

        public static void RegisterType(string name, IronPythonTypeWrapper type, Plugin plugin)
        {
            types[name] = new TypeData(type);
            plugins[type.Type] = plugin;
        }
        public static void RegisterType(Type type, Plugin plugin)
        {
            types[type.Name] = new TypeData(type);
            plugins[type] = plugin;
        }
        public static T Instantiate<T>(string type, params object[] args) where T : class
        {
            return types[type].Instantiate(args) as T;
        }
        public List<string> GetNames(Type t)
        {
            List<string> output = new List<string>();
            foreach (string type in types.Keys)
            {
                if (types[type].Type.IsSubclassOf(t))
                {
                    output.Add(type);
                }
            }
            return output;
        }
        public TypeData GetData(string name)
        {
            return types[name];
        }
        public Type this[string name]
        {
            get
            {
                return types[name].Type;
            }
        }
        public static Plugin GetPlugin(Type type)
        {
            return plugins[type];
        }
    }
}
