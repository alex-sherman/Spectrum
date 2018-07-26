using ProtoBuf;
using Spectrum.Framework.Content;
using Spectrum.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    [ProtoContract]
    public class ModelSurrogate
    {
        [ProtoMember(1)]
        string ModelName;
        public static implicit operator SpecModel(ModelSurrogate surrogate)
        {
            if (surrogate.ModelName == null) return null;
            return ContentHelper.Load<SpecModel>(surrogate.ModelName);
        }
        public static implicit operator ModelSurrogate(SpecModel model)
        {
            return new ModelSurrogate() { ModelName = model?.Name };
        }
    }
}
