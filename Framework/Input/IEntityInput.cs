using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public interface IEntityInput
    {
        void HandleInput(InputState input);
    }
}
