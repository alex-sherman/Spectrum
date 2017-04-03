using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Screens
{
    public interface LayoutManager
    {
        void OnMeasure(Element element, int width, int height);
        void OnLayout(Element element, Rectangle bounds);
    }
}
