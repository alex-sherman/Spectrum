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
        // TODO: Is using this model a good idea?
        public static ReplicationModel Model = TypeUtil.Model;
        private static DefaultDict<Type, Plugin> plugins = new DefaultDict<Type, Plugin>();
        static ReplicateTypeAttribute MakeReplicateAttribute() => new ReplicateTypeAttribute() { AutoMembers = AutoAdd.None };
        public static TypeData RegisterType(Type type, Plugin plugin)
        {
            // Laziness to avoid marking up every type with ReplicateType
            var typeData = Model.Add(type, type.GetCustomAttribute<ReplicateTypeAttribute>(false) ?? MakeReplicateAttribute());
            // Reinitialize if it was added by another call and has no ReplicateType
            if (typeData.TypeAttribute == null)
            {
                typeData.TypeAttribute = MakeReplicateAttribute();
                typeData.InitializeMembers();
                Model.ClearTypeAccessorCache();
            }
            plugins[type] = plugin;
            return typeData;
        }
        public static IEnumerable<TypeAccessor> GetTypes(Type type)
        {
            return Model.Types.Values
                .Where(t => t.GenericTypeParameters == null && type.IsAssignableFrom(t.Type))
                .Select(t => Model.GetTypeAccessor(t.Type));
        }
        public static Plugin GetPlugin(Type type)
        {
            return plugins[type];
        }
    }
}
