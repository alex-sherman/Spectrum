using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Network
{
    public interface NetworkSerializable
    {
        void WriteTo(NetMessage output);
        NetworkSerializable Copy();
    }
}
