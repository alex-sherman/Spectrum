using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Content
{
    class SVGParser : CachedContentParser<SvgDocument>
    {
        protected override SvgDocument LoadData(string path, string name)
        {
            return SvgDocument.Open(path);
        }
    }
}
