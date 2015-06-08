using CSCore;
using CSCore.Codecs;
using Spectrum.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    class SoundParser : CachedContentParser<IWaveSource, SoundEffect>
    {
        public SoundParser()
        {
            Prefix = "Sounds\\";
        }

        protected override IWaveSource LoadData(string path)
        {
            if (System.IO.File.Exists(path + ".wav")) path += ".wav";
            else if (System.IO.File.Exists(path + ".m4a")) path += ".m4a";
            return CodecFactory.Instance.GetCodec(path);
        }

        protected override SoundEffect SafeCopy(IWaveSource data)
        {
            return new SoundEffect(data);
        }
    }
}
