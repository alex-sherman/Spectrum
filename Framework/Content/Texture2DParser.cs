using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    public class Texture2DData
    {
        public bool HasAlpha = false;
    }
    public class Texture2DParser : CachedContentParser<Texture2D, Texture2D>
    {
        public Texture2DParser()
        {

            Prefix = @"Textures\";
        }
        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }
        protected override Texture2D LoadData(string path, string name)
        {
            name = TryExtensions(path, ".jpg", ".png");
            Texture2D loaded = Texture2D.FromStream(SpectrumGame.Game.GraphicsDevice, new FileStream(name, FileMode.Open, FileAccess.Read));
            //Of course you have to generate your own mip maps when you import from a file
            //why not. Thanks Monogame.
            bool mipMap = IsPowerOfTwo(loaded.Width) && IsPowerOfTwo(loaded.Height);
            Texture2D output = new Texture2D(SpectrumGame.Game.GraphicsDevice, loaded.Width, loaded.Height, mipMap, loaded.Format);
            Texture2DData outputTag = new Texture2DData();
            output.Tag = outputTag;
            Color[] data = new Color[loaded.Height * loaded.Width];
            Color[] lastLevelData = data;
            loaded.GetData<Color>(data);
            for (int level = 0; level < output.LevelCount; level++)
            {
                int stride = 1 << level;
                int levelHeight = loaded.Height / stride;
                int levelWidth = loaded.Width / stride;
                Color[] levelData = level == 0 ? data : new Color[levelHeight * levelWidth];
                if (level > 0)
                {
                    for (int x = 0; x < levelWidth; x++)
                    {
                        for (int y = 0; y < levelHeight; y++)
                        {
                            Vector4 sum = new Vector4();
                            for (int ix = 0; ix < 2; ix++)
                            {
                                for (int iy = 0; iy < 2; iy++)
                                {
                                    Color c = lastLevelData[(x * 2 + ix) + (y * 2 + iy) * levelWidth * 2];
                                    if (c.A < 255)
                                    {
                                        outputTag.HasAlpha = true;
                                        continue;
                                    }
                                    sum.X += c.R;
                                    sum.Y += c.G;
                                    sum.Z += c.B;
                                    sum.W += c.A;
                                }
                            }
                            if (sum.W == 0) continue;
                            levelData[x + y * levelWidth].R = (byte)(sum.X / (sum.W / 255));
                            levelData[x + y * levelWidth].G = (byte)(sum.Y / (sum.W / 255));
                            levelData[x + y * levelWidth].B = (byte)(sum.Z / (sum.W / 255));
                            levelData[x + y * levelWidth].A = (byte)(sum.W / (sum.W / 255));
                        }
                    }
                }
                output.SetData<Color>(level, null, levelData, 0, levelHeight * levelWidth);
                lastLevelData = levelData;
            }
            return output;
        }

        protected override Texture2D SafeCopy(Texture2D cache)
        {
            return cache;
        }
    }
}
