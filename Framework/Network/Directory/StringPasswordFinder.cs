using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Network.Directory
{
    class StringPasswordFinder : IPasswordFinder
    {
        string pass;
        public StringPasswordFinder(string pass)
        {
            this.pass = pass;
        }
        public char[] GetPassword()
        {
            return pass.ToArray();
        }
    }
}
