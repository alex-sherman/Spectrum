using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework
{
    public struct Color
    {
        public byte R { get => (byte)(storage & 0x000000ff); set { storage = (storage & 0xffffff00) | value; } }
        public byte G { get => (byte)((storage & 0x0000ff00) >> 8); set { storage = (storage & 0xffff00ff) | (uint)value << 8; } }
        public byte B { get => (byte)((storage & 0x00ff0000) >> 16); set { storage = (storage & 0xff00ffff) | (uint)value << 16; } }
        public byte A { get => (byte)((storage & 0xff000000) >> 24); set { storage = (storage & 0x00ffffff) | (uint)value << 24; } }
        // ABGR storage
        // 0xAA BB GG RR
        private uint storage;
        public Color(byte r, byte g, byte b, byte a) { storage = 0; R = r; G = g; B = b; A = a; }
        public Color(Color c, byte a) { storage = 0; R = c.R; G = c.G; B = c.B; A = a; }
        public Color(Color c, float a) { storage = 0; R = c.R; G = c.G; B = c.B; A = (byte)(a * 255); }
        public Color(Vector4 v) : this((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.W * 255)) { }
        public Color(Vector3 v) : this((byte)(v.X * 255), (byte)(v.Y * 255), (byte)(v.Z * 255), 255) { }
        public Color(float r, float g, float b) : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), 255) { }
        public Vector4 ToVector4() => new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
        public Vector3 ToVector3() => new Vector3(R / 255f, G / 255f, B / 255f);
        public static Color FromString(string value)
        {
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(value);
                return new Color((byte)(color.R * color.A / 255), (byte)(color.G * color.A / 255), (byte)(color.B * color.A / 255), color.A);
            }
            catch (Exception)
            {
                return Black;
            }
        }
        public static implicit operator Microsoft.Xna.Framework.Color(Color color) => new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        public static implicit operator Color(Microsoft.Xna.Framework.Color color) => new Color(color.R, color.G, color.B, color.A);
        public static implicit operator Color(string color) => FromString(color);
        public static Color Transparent => new Color(0, 0, 0, 0);
        public static Color Black => new Color(0, 0, 0, 255);
        public static Color Red => new Color(255, 0, 0, 255);
        public static Color Green => new Color(0, 255, 0, 255);
        public static Color Blue => new Color(0, 0, 255, 255);
        public static Color White => new Color(255, 255, 255, 255);
    }
}
