using SharpDX.IO;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using Spectrum.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Content
{
    class SoundParser : CachedContentParser<SoundEffect>
    {
        public SoundParser() : base("wav", "m4a")
        {
            Prefix = "Sounds";
        }

        protected override SoundEffect LoadData(string path, string name)
        {
            var nativefilestream = new NativeFileStream(
                path,
                NativeFileMode.Open,
                NativeFileAccess.Read,
                NativeFileShare.Read);
            return new SoundEffect(nativefilestream);
        }
    }
}
