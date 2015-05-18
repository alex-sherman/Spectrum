using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Network
{
    public interface ISerializable
    {
        void WriteTo(NetMessage output);
        ISerializable Copy();
    }
}
