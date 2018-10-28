using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum.Framework.Audio
{
    public enum AudioState
    {
        Stopped,
        Playing,
        Paused,
    }
    public class AudioPlayer
    {
        const int bufferAhead = 3;
        protected SourceVoice voice;
        public event Action OnTrackEnd;
        private SoundEffect _sound;
        public SoundEffect SoundEffect
        {
            get => _sound;
            set
            {
                _sound = value;
                DisposeVoice();
                if (value != null)
                {
                    voice = new SourceVoice(AudioManager._xaudio2, value.WaveFormat);
                    voice.BufferEnd += _voice_BufferEnd;
                    voice.SetVolume(_volume);
                    if (State == AudioState.Playing)
                    {
                        index = 0;
                        State = AudioState.Playing;
                    }
                    else
                        State = AudioState.Stopped;
                }
            }
        }
        public bool Loop { get; set; }
        ResourcePool<AudioBuffer> buffers = new ResourcePool<AudioBuffer>();
        List<AudioBuffer> queuedBuffers = new List<AudioBuffer>();
        int index = 0;

        public void ClearEvents()
        {
            if (OnTrackEnd != null)
                foreach (var handler in OnTrackEnd.GetInvocationList())
                    OnTrackEnd -= (Action)handler;
        }

        private AudioState _state = AudioState.Stopped;
        public AudioState State
        {
            get => _state;
            set
            {
                if (voice == null)
                {
                    _state = AudioState.Stopped;
                    return;
                }
                switch (value)
                {
                    case AudioState.Stopped:
                        index = 0;
                        voice.Stop();
                        break;
                    case AudioState.Playing:
                        queueBuffers();
                        voice.Start();
                        break;
                    case AudioState.Paused:
                        voice.Stop();
                        break;
                    default:
                        break;
                }
                _state = value;
            }
        }

        void _voice_BufferEnd(IntPtr obj)
        {
            lock (this)
            {
                queuedBuffers.RemoveAt(0);
                if (SoundEffect == null || voice == null)
                    return;
                queueBuffers();
                if (index >= SoundEffect.Samples.Count && voice.State.BuffersQueued == 0)
                {
                    voice.Stop();
                    State = AudioState.Stopped;
                    SpecTime.Wait(0).ContinueWith(dt =>
                    {
                        OnTrackEnd?.Invoke();
                        if (Loop)
                            State = AudioState.Playing;
                    });
                }
            }
        }
        void queueBuffers()
        {
            lock (this)
            {
                while (SoundEffect != null && voice.State.BuffersQueued < bufferAhead && index < SoundEffect.Samples.Count)
                {
                    var buffer = new AudioBuffer();
                    buffer.AudioDataPointer = SoundEffect.Samples[index].Pointer;
                    buffer.AudioBytes = SoundEffect.Samples[index].Size;
                    index++;
                    queuedBuffers.Add(buffer);
                    voice.SubmitSourceBuffer(buffer, null);
                }
            }
        }

        private float _volume = 1f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                voice?.SetVolume(value);
            }
        }

        public void DisposeVoice()
        {
            lock (this)
            {
                if (!(voice?.IsDisposed ?? true))
                {
                    voice.FlushSourceBuffers();
                    voice.Stop();
                    //voice.Dispose();
                    voice = null;
                }
            }
        }
    }
}
