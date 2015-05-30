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
        public static TypeHelper Helper = new TypeHelper();
        private RealDict<string, Type> Types = new RealDict<string, Type>();
        public List<String> GetTypes() { return Types.Keys.ToList(); }

        public T Instantiate<T>(string type, params object[] args) where T: class
        {
            return Instantiate(this[type], args) as T;
        }
        public List<string> GetNames(Type t)
        {
            List<string> output = new List<string>();
            foreach (string type in Types.Keys)
            {
                if (Types[type].IsSubclassOf(t))
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
                return Types[name];
            }
            set
            {
                if (Types[name] != null) { throw new ArgumentException("Key was already in the dictionary"); }
                Types[name] = value;
            }
        }
        public static object Instantiate(Type t, params object[] args)
        {
            if (t == null) { return null; }
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }
            System.Reflection.ConstructorInfo cinfo = t.GetConstructor(types);
            if (cinfo == null) { throw new InvalidOperationException("Unable to construct an entity with the given parameters"); }
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
                            field.SetValue(output, ContentHelper.LoadType(field.FieldType, preload.Path, preload.Plugin));
                        }
                    }
                }
                return output;
            }
            catch
            {
                return null;
            }
        }
    }
}
