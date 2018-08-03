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
        public SoundParser() : base("wav", "m4a")
        {
            Prefix = "Sounds";
        }

        protected override SoundStream LoadData(string path, string name)
        {
            var nativefilestream = new NativeFileStream(
                path,
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
