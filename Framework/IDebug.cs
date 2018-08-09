using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework
{
    public interface IDebug
    {
        string Debug();
        void DebugDraw(float gameTime);
    }
}
