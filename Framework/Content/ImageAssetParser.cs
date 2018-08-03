using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    class ImageAssetParser : CachedContentParser<ImageAsset>
    {
        public ImageAssetParser() : base("svg", "png", "jpg")
        {
            Prefix = "Textures";
        }
        protected override ImageAsset LoadData(string path, string name)
        {
            var extension = Path.GetExtension(path).Substring(1);
            if (extension == "svg")
                return new ImageAsset(SvgDocument.Open(path));
            else
                return new ImageAsset(Texture2DParser.LoadFromPath(path));
        }
    }
}
