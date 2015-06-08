using CSCore;
using CSCore.Codecs;
using CSCore.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class SoundEffect
    {
        private StreamingSourceVoice _streamingSourceVoice;
        private IWaveSource _waveSource;
        public bool Playing { get; private set; }

        internal SoundEffect(IWaveSource wavesource)
        {
            _waveSource = wavesource;
            _streamingSourceVoice = StreamingSourceVoice.Create(AudioManager._xaudio2, _waveSource, 300);
            _streamingSourceVoice.Stopped += _streamingSourceVoice_Stopped;
        }

        public bool Loop
        {
            get { return _streamingSourceVoice.Loop; }
            set { _streamingSourceVoice.Loop = value; }
        }

        public void Play()
        {
            StreamingSourceVoiceListener.Default.Add(_streamingSourceVoice);
            _streamingSourceVoice.Start();
            Playing = true;
        }

        public void Stop()
        {
            Playing = false;
        }

        public float Volume
        {
            get { return _streamingSourceVoice.Volume; }
            set { _streamingSourceVoice.Volume = value; }
        }

        void _streamingSourceVoice_Stopped(object sender, EventArgs e)
        {
            Playing = false;
        }
    }
}
