using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Input
{
    public interface IAxis
    {
        float Value(InputState input, PlayerInformation player);
    }
    public struct KeyboardAxis : IAxis
    {
        public Keys Positive;
        public Keys Negative;
        public float Scalar;
        public KeyboardAxis(Keys positive, Keys negative, float scalar = 1)
        {
            Positive = positive;
            Negative = negative;
            Scalar = scalar;
        }

        public float Value(InputState input, PlayerInformation player)
        {
            if (!player.UsesKeyboard) return 0;
            float output = 0;
            if (input.IsKeyDown(Positive))
                output += 1;
            if (input.IsKeyDown(Negative))
                output -= 1;
            return output * Scalar;
        }
    }
    public struct GamepadAxis : IAxis
    {
        public GamepadAxisType Axis;
        public float Scalar;
        public GamepadAxis(GamepadAxisType axis, float scalar = 1)
        {
            Axis = axis;
            Scalar = scalar;
        }
        public float Value(InputState input, PlayerInformation player)
        {
            float output = 0;
            foreach (int gamepadIndex in player.UsedGamepads)
            {
                output += input.Gamepads[gamepadIndex].Axis(Axis);
            }
            return output * Scalar;
        }
    }
    public struct MouseAxis : IAxis
    {
        public bool horizontal;
        public float scalar;
        public MouseAxis(bool horizontal, float scalar = 0.003f)
        {
            this.horizontal = horizontal;
            this.scalar = scalar;
        }

        public float Value(InputState input, PlayerInformation player)
        {
            if (horizontal)
                return (input.MouseState.X - SpectrumGame.Game.GraphicsDevice.Viewport.Width / 2) * scalar;
            else
                return (input.MouseState.Y - SpectrumGame.Game.GraphicsDevice.Viewport.Height / 2) * scalar;
        }
    }
    public struct Axis1
    {
        public List<IAxis> Axes;
        public Axis1(IAxis axis)
        {
            Axes = new List<IAxis>();
            Axes.Add(axis);
        }
        public float Value(InputState input, PlayerInformation player)
        {
            float output = 0;
            foreach (IAxis axis in Axes)
            {
                output += axis.Value(input, player);
            }
            return Math.Min(Math.Max(output, -1), 1);
        }
    }
}
