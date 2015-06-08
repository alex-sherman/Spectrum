using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class SoundEffect
    {
        private readonly SoundStream _stream;
        private readonly AudioBuffer _buffer;
        internal readonly SourceVoice _voice;
        public bool Loop { get; set; }

        public SoundEffect(SoundStream stream)
        {
            _stream = stream;
            _buffer = new AudioBuffer
                          {
                              Stream = _stream,
                              AudioBytes = (int)_stream.Length,
                              Flags = BufferFlags.EndOfStream

                          };

            _voice = new SourceVoice(AudioManager._xaudio2, stream.Format, true);
            _voice.BufferEnd += _voice_BufferEnd;
        }

        void _voice_BufferEnd(IntPtr obj)
        {
            if (Loop)
                Play();
        }

        public void Play()
        {
            _voice.FlushSourceBuffers();
            _voice.SubmitSourceBuffer(_buffer, _stream.DecodedPacketsInfo);
            _voice.Start();
        }

        public float Volume
        {
            get { return _voice.Volume; }
            set { _voice.SetVolume(value); }
        }

        public void Dispose()
        {
            _voice.Stop();
            _voice.Dispose();
        }
    }
}
