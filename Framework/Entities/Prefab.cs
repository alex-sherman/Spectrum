using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Entities
{
    public class Prefab
    {
        private static Dictionary<string, InitData> prefabs = new Dictionary<string, InitData>();
        public static IReadOnlyDictionary<string, InitData> Prefabs
        {
            get { return prefabs; }
        }
        public static void Register(string name, InitData data)
        {
            prefabs[name] = data.ToImmutable();
            prefabs[name].TypeName = name;
        }
    }
}
