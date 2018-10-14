using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Utility
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// Scales the source rectangle (and optionally centers it) to the destination rectangle.
        /// The crop paremeter determines whether to scale up (overflowing and requiring a crop) or down (requiring no crop).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="crop">If true will scale the source up, else down</param>
        /// <param name="center"></param>
        /// <returns></returns>
        public static Rectangle FitTo(this Rectangle source, Rectangle destination, bool crop = true, bool center = true)
        {
            bool scaleX = (!crop) ^ (destination.Width * 1.0 / source.Width * source.Height > destination.Height);
            double scale = scaleX ? destination.Width * 1.0 / source.Width : destination.Height * 1.0 / source.Height;
            var fittedX = source.X;
            var fittedY = source.Y;
            var fittedWidth = (int)(source.Width * scale);
            var fittedHeight = (int)(source.Height * scale);
            if (center)
            {
                fittedX = (destination.Width - fittedWidth) / 2;
                fittedY = (destination.Height - fittedHeight) / 2;
            }
            return new Rectangle(fittedX, fittedY, fittedWidth, fittedHeight);
        }
    }
}
