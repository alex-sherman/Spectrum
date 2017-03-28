using Spectrum.Framework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    class MemberInfo
    {
        private FieldInfo field;
        private PropertyInfo property;
        public MemberInfo(FieldInfo field)
        {
            this.field = field;
        }
        public MemberInfo(PropertyInfo property)
        {
            this.property = property;
        }
        public Type MemberType { get { return property?.PropertyType ?? field?.FieldType; } }
        public void SetValue(object obj, object value) { property?.SetValue(obj, value); field?.SetValue(obj, value); }
        public object GetValue(object obj) { return property?.GetValue(obj) ?? field?.GetValue(obj); }
    }
    public class TypeData
    {
        private Dictionary<string, MemberInfo> members = new Dictionary<string, MemberInfo>();
        public List<string> ReplicatedMemebers = new List<string>();
        public Type Type { get; private set; }
        public TypeData(Type type)
        {
            Type = type;
            foreach (var field in type.GetFields())
            {
                members[field.Name] = new MemberInfo(field);
                if (field.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(field.Name);
            }
            foreach (var property in type.GetProperties())
            {
                members[property.Name] = new MemberInfo(property);
                if(property.GetCustomAttributes().Where(attr => attr is ReplicateAttribute).Any())
                    ReplicatedMemebers.Add(property.Name);
            }
        }
        public void Set(object obj, string name, object value)
        {
            if (members.ContainsKey(name))
            {
                MemberInfo info = members[name];
                if (info.MemberType.IsAssignableFrom(value.GetType()))
                    members[name].SetValue(obj, value);
                else if(value is string)
                {
                    object new_value = TypeHelper.Instantiate<object>(value as string);
                    if (info.MemberType.IsAssignableFrom(new_value.GetType()))
                        members[name].SetValue(obj, new_value);
                }
            }
        }
        public object Get(object obj, string name)
        {
            if (members.ContainsKey(name))
            {
                return members[name].GetValue(obj);
            }
            return null;
        }
    }
}
