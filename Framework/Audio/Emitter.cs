using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class Emitter
    {
        private CSCore.XAudio2.X3DAudio.Emitter _emitter;

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
            _emitter = new CSCore.XAudio2.X3DAudio.Emitter()
            {
                ChannelCount = 1,
                CurveDistanceScaler = float.MinValue,
                OrientFront = new CSCore.Utils.Vector3(0, 0, 1),
                OrientTop = new CSCore.Utils.Vector3(0, 1, 0),
                Position = new CSCore.Utils.Vector3(0, 0, 0),
                Velocity = new CSCore.Utils.Vector3(0, 0, 0)
            };
        }
    }
}
