using Microsoft.Xna.Framework.Graphics;
using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    public class ImageAssetParser : CachedContentParser<ImageAsset>
    {
        private static Dictionary<string, ImageAsset> additionalAssets = new Dictionary<string, ImageAsset>();
        public static void Add(string path, ImageAsset asset) => additionalAssets[path] = asset;
        public ImageAssetParser() : base("svg", "png", "jpg")
        {
            Prefix = "Textures";
        }
        protected override string ResolvePath(string path, string name)
        {
            if (additionalAssets.ContainsKey(name)) return path;
            return base.ResolvePath(path, name);
        }
        protected override ImageAsset LoadData(string path, string name)
        {
            if (additionalAssets.TryGetValue(name, out var result)) return result;
            var extension = Path.GetExtension(path).Substring(1);
            if (extension == "svg")
                return new ImageAsset(SvgDocument.Open(path));
            else
                return new ImageAsset(Texture2DParser.LoadFromPath(path));
        }
    }
}
