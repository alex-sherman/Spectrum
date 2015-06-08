using Microsoft.Xna.Framework;
using SharpDX.X3DAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class Emitter
    {
        private SharpDX.X3DAudio.Emitter _emitter;
        private List<SoundEffect> _sounds = new List<SoundEffect>();

        public Vector3 Position
        {
            get { return AudioManager.V3ToV3(_emitter.Position); }
            set { _emitter.Position = AudioManager.V3ToV3(value); }
        }

        public Vector3 Forward
        {
            get { return AudioManager.V3ToV3(_emitter.OrientFront); }
            set { _emitter.OrientFront = AudioManager.V3ToV3(value); }
        }

        public Vector3 Up
        {
            get { return AudioManager.V3ToV3(_emitter.OrientTop); }
            set { _emitter.OrientTop = AudioManager.V3ToV3(value); }
        }

        public Emitter()
        {
            _emitter = new SharpDX.X3DAudio.Emitter()
            {
                ChannelCount = 1,
                CurveDistanceScaler = 20,
                OrientFront = new SharpDX.Vector3(0, 0, 1),
                OrientTop = new SharpDX.Vector3(0, 1, 0),
                Position = new SharpDX.Vector3(0, 0, 0),
                Velocity = new SharpDX.Vector3(0, 0, 0)
            };
        }

        public void RegisterSoundEffect(SoundEffect sound)
        {
            _sounds.Add(sound);
        }

        void streamingSourceVoice_Stopped(object sender, EventArgs e)
        {
            SoundEffect sound = sender as SoundEffect;
            UnregisterSoundEffect(sender as SoundEffect);
        }
        public void UnregisterSoundEffect(SoundEffect sound)
        {
            _sounds.Remove(sound);
        }

        public void Update()
        {
            foreach (var sound in _sounds)
            {
                DspSettings dspSettings = new DspSettings(1, AudioManager.DestinationChannels);
                AudioManager.X3DAudio.Calculate(AudioManager.Listener, _emitter, CalculateFlags.Matrix, dspSettings);
                sound._voice.SetOutputMatrix(1, AudioManager.DestinationChannels, dspSettings.MatrixCoefficients);
            }
        }
    }
}
