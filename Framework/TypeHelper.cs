using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Spectrum.Framework.Network;
using Spectrum.Framework.Content;
using Replicate.MetaData;
using Replicate;
using Spectrum.Framework.Entities;

namespace Spectrum.Framework
{
    public class TypeHelper
    {
        public static ReplicationModel Model = new ReplicationModel(false);
        private static DefaultDict<string, TypeData> types = new DefaultDict<string, TypeData>();
        private static DefaultDict<Type, Plugin> plugins = new DefaultDict<Type, Plugin>();

        public static TypeData RegisterType(Type type, Plugin plugin)
        {
            // Laziness to avoid marking up every type with ReplicateType
            var typeData = Model.Add(type, type.GetCustomAttribute<ReplicateTypeAttribute>() ?? new ReplicateTypeAttribute());
            // Reinitialize if it was added by another call and has no ReplicateType
            if (typeData.TypeAttribute == null)
            {
                typeData.TypeAttribute = new ReplicateTypeAttribute();
                typeData.InitializeMembers();
                Model.ClearTypeAccessorCache();
            }
            plugins[type] = plugin;
            return typeData;
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
                return types[name]?.Type;
            }
        }
        public static Plugin GetPlugin(Type type)
        {
            return plugins[type];
        }
        public static Type FixGeneric(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GetGenericArguments()[0];
                else
                    type = type.GetGenericTypeDefinition();
            }
            return type;
        }
    }
}
