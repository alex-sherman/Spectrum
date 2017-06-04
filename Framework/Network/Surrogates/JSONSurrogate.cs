using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    [ProtoContract]
    class JSONSurrogate
    {
        [ProtoMember(1)]
        string json;
        public static implicit operator JToken(JSONSurrogate surrogate)
        {
            return JToken.Parse(surrogate.json);
        }
        public static implicit operator JSONSurrogate(JToken token)
        {
            if (token == null) return null;
            return new JSONSurrogate() { json = token.ToString(Formatting.None) };
        }
    }
}
