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
    public class Settings
    {
        public static Matrix reflectionProjection;
        public static Matrix lightProjection;
        public static bool enableWater = false;
        public static int waterQuality = 0;
        public const String WaterBumpMapBase = "waterbump";
        public const String WaterBumpMap = "waterbump1";
        public static Vector2 ScreenSize;
        public Settings()
        {
            lightProjection = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 20f,
                1,
                100, 1100f);
        }
        public static float WaterPerturbation
        {
            get
            {
                return WaterEffect.WaterPerturbation;
            }
            set
            {
                WaterEffect.WaterPerturbation = value;
            }
        }

    }
}
