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
        public static Matrix projection;
        public static Matrix reflectionProjection;
        public static Matrix lightProjection;
        public static bool enableWater = false;
        public static int waterQuality = 0;
        public const String WaterBumpMapBase = "waterbump";
        public const String WaterBumpMap = "waterbump1";
        public static Vector2 ScreenSize;

        public static void Init(GraphicsDevice device)
        {
            ResetProjection(SpectrumGame.Game, EventArgs.Empty);
            SpectrumGame.Game.OnScreenResize += ResetProjection;
        }
        public static void ResetProjection(object sender, EventArgs args)
        {
            GraphicsDevice device = (sender as SpectrumGame).GraphicsDevice;
            projection = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4.0f,
                (float)device.Viewport.Width /
                (float)device.Viewport.Height,
                1f, 10000);
            //The reflection view has a slightly larger field of view
            //so that water doesn't get messed up at the edges when
            //waves cause the texture coordinates to go off the edge
            reflectionProjection = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 3.5f,
                (float)device.Viewport.Width /
                (float)device.Viewport.Height,
                1f, 10000);
            lightProjection = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 20f,
                1,
                100, 1100f);
            //lightProjection = new Matrix();
            //float e = (float)(1 / Math.Tan((float)Math.PI / 4.0f));
            //lightProjection.M11 = e;
            //lightProjection.M22 = e;
            //lightProjection.M33 = 0.001f - 1;
            //lightProjection.M34 = 0.001f - 2;
            //lightProjection.M43 = -1;
            //ScreenSize.X = device.Viewport.Width;
            //ScreenSize.Y = device.Viewport.Height;
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
