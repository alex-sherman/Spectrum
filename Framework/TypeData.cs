using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public class TypeData
    {
        private Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
        private Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
        private List<string> replicated = new List<string>();
        public Type Type { get; private set; }
        public TypeData(Type type)
        {
            Type = type;
            foreach (var field in type.GetFields())
            {
                fields[field.Name] = field;
                if (field.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    replicated.Add(field.Name);
            }
            foreach (var property in type.GetProperties())
            {
                properties[property.Name] = property;
                if(property.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    replicated.Add(property.Name);
            }
        }
        public void Set(object obj, string name, object value)
        {
            if (fields.ContainsKey(name))
                fields[name].SetValue(obj, value);
            else if (properties.ContainsKey(name))
                properties[name].SetValue(obj, value);
        }
        public object Get(object obj, string name)
        {
            if (fields.ContainsKey(name))
                return fields[name].GetValue(obj);
            else if (properties.ContainsKey(name))
                return properties[name].GetValue(obj);
            return null;
        }
    }
}
