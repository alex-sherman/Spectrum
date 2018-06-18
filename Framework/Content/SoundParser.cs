using SharpDX.IO;
using SharpDX.Multimedia;
using Spectrum.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    class SoundParser : CachedContentParser<SoundStream, SoundEffect>
    {
        public SoundParser()
        {
            Prefix = "Sounds";
        }

        protected override SoundStream LoadData(string path, string name)
        {
            name = TryThrowExtensions(path, ".wav", ".m4a");
            var nativefilestream = new NativeFileStream(
                name,
                NativeFileMode.Open,
                NativeFileAccess.Read,
                NativeFileShare.Read);

            return new SoundStream(nativefilestream);
        }

        protected override SoundEffect SafeCopy(SoundStream data)
        {
            data.Position = 0;
            return new SoundEffect(data);
        }
    }
}
