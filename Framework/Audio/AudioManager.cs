using Microsoft.Xna.Framework;
using SharpDX.Mathematics.Interop;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Audio
{
    public class AudioManager
    {
        internal static RawVector3 V3ToV3(Vector3 vector)
        {
            return new RawVector3(vector.X, vector.Y, vector.Z);
        }
        internal static Vector3 V3ToV3(RawVector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        internal static Listener Listener;
        internal static readonly XAudio2 _xaudio2 = new XAudio2();
        internal static readonly X3DAudio X3DAudio = new X3DAudio(SharpDX.Multimedia.Speakers.Stereo);
        internal static MasteringVoice MasteringVoice = new MasteringVoice(_xaudio2);
        public static int DestinationChannels { get { return 2; } }

        public static Vector3 ListenerPosition
        {
            get => V3ToV3(Listener.Position);
            set => Listener.Position = V3ToV3(value);
        }
        public static Vector3 ListenerForward
        {
            get => V3ToV3(Listener.OrientFront);
            set => Listener.OrientFront = V3ToV3(value);
        }
        public static Vector3 ListenerUp
        {
            get => V3ToV3(Listener.OrientTop);
            set => Listener.OrientTop = V3ToV3(value);
        }

        public static void Init()
        {
            Listener = new Listener()
            {
                Position = new RawVector3(0, 0, 0),
                OrientFront = new RawVector3(0, 0, 1),
                OrientTop = new RawVector3(0, 1, 0),
                Velocity = new RawVector3(0, 0, 0)
            };
        }

        public static void Shutdown()
        {
            _xaudio2.Dispose();
        }
    }
}
