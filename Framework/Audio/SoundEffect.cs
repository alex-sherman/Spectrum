using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class SoundEffect
    {
        public List<DataPointer> Samples;
        public WaveFormat WaveFormat;

        public SoundEffect(Stream stream)
        {
            AudioDecoder audioDecoder = new AudioDecoder(stream);
            WaveFormat = audioDecoder.WaveFormat;
            Samples = audioDecoder.GetSamples().Select(sample =>
            {
                var output = new DataPointer(SharpDX.Utilities.AllocateMemory(sample.Size), sample.Size);
                SharpDX.Utilities.CopyMemory(output.Pointer, sample.Pointer, sample.Size);
                return output;
            }).ToList();
        }
        ~SoundEffect()
        {
            if (Samples != null)
                foreach (var sample in Samples)
                    SharpDX.Utilities.FreeMemory(sample.Pointer);

        }
    }
}
