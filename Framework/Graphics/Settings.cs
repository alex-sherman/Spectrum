using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Spectrum.Framework.Content;

namespace Spectrum.Framework.Graphics
{
    public static class Settings
    {
        public static Matrix reflectionProjection;
        public static Matrix lightProjection;
        public static bool enableWater = false;
        public static int waterQuality = 0;
        public const string WaterBumpMapBase = "waterbump";
        public const string WaterBumpMap = "waterbump1";
        public static Vector2 ScreenSize;
        static Settings()
        {
            lightProjection = Matrix.CreatePerspectiveFOV(
                (float)Math.PI / 20f,
                1,
                100, 1100f);
        }
    }
}
