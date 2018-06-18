using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    class ImageAssetParser : CachedContentParser<ImageAsset, ImageAsset>
    {
        public ImageAssetParser()
        {
            Prefix = "Textures";
        }
        protected override ImageAsset LoadData(string path, string name)
        {

            string full_path = TryExtensions(path, ".svg");
            if(full_path != null)
            {
                return new ImageAsset(SvgDocument.Open(full_path));
            }
            full_path = TryThrowExtensions(path, ".png", ".jpg");
            return new ImageAsset(Texture2DParser.Load(full_path));
        }

        protected override ImageAsset SafeCopy(ImageAsset data)
        {
            return data.Clone();
        }
    }
}
