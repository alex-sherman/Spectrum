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
        public static ReplicationModel Model = TypeUtil.Model;
        private static DefaultDict<Type, Plugin> plugins = new DefaultDict<Type, Plugin>();

        public static TypeData RegisterType(Type type, Plugin plugin)
        {
            // Laziness to avoid marking up every type with ReplicateType
            var typeData = Model.Add(type, type.GetCustomAttribute<ReplicateTypeAttribute>(false) ?? new ReplicateTypeAttribute());
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
        public static Plugin GetPlugin(Type type)
        {
            return plugins[type];
        }
    }
}
