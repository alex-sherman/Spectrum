using ProtoBuf;
using Replicate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Network.Surrogates
{
    [ProtoContract]
    [ReplicateType]
    public class StreamSurrogate
    {
        [ProtoMember(1)]
        public byte[] buffer;
        public static implicit operator StreamSurrogate(MemoryStream stream)
        {
            if (stream == null) return null;
            return new StreamSurrogate() { buffer = stream.ToArray() };
        }
        public static implicit operator MemoryStream(StreamSurrogate stream)
        {
            if (stream == null) return null;
            return new MemoryStream(stream.buffer);
        }
    }
}
