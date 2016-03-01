using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Spectrum.Framework.Entities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class ReplicateAttribute : System.Attribute { }
}
