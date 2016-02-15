using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spectrum.Framework;
using System.Reflection;
using System.IO;
using Spectrum.Framework.Network;
using Spectrum.Framework.Content;

namespace Spectrum.Framework
{
    public class TypeHelper
    {
        public static TypeHelper Types = new TypeHelper();
        private static RealDict<string, Type> types = new RealDict<string, Type>();
        private static RealDict<Type, Plugin> plugins = new RealDict<Type, Plugin>();
        public List<String> GetTypes() { return types.Keys.ToList(); }

        public static void RegisterType(Type type, Plugin plugin)
        {
            types[type.Name] = type;
            plugins[type] = plugin;
        }
        public static T Instantiate<T>(string type, params object[] args) where T : class
        {
            return Instantiate(types[type], args) as T;
        }
        public List<string> GetNames(Type t)
        {
            List<string> output = new List<string>();
            foreach (string type in types.Keys)
            {
                if (types[type].IsSubclassOf(t))
                {
                    output.Add(type);
                }
            }
            return output;
        }
        public Type this[string name]
        {
            get
            {
                return types[name];
            }
        }
        public static object Instantiate(Type t, params object[] args)
        {
            if (t == null) { return null; }
            if (args == null) args = new object[0];
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }
            ConstructorInfo cinfo = t.GetConstructor(types);
            if (cinfo == null)
            {
                throw new InvalidOperationException("Unable to construct an entity with the given parameters");
            }
            try
            {
                object output = cinfo.Invoke(args);

                foreach (FieldInfo field in output.GetType().GetFields())
                {
                    foreach (object attribute in field.GetCustomAttributes(true).ToList())
                    {
                        PreloadedContentAttribute preload = attribute as PreloadedContentAttribute;
                        if (preload != null)
                        {
                            field.SetValue(output, ContentHelper.LoadType(field.FieldType, preload.Path));
                        }
                    }
                }
                return output;
            }
            catch
            {
                throw new Exception("An error occured constructing the entity");
            }
        }
        public static Plugin GetPlugin(Type type)
        {
            return plugins[type];
        }
    }
}
