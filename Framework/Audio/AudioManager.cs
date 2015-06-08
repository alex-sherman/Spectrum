using CSCore;
using CSCore.XAudio2;
using CSCore.XAudio2.X3DAudio;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Audio = CSCore.Utils;

namespace Spectrum.Framework.Audio
{
    public class AudioManager
    {
        internal static CSCore.Utils.Vector3 V3ToV3(Vector3 vector)
        {
            return new CSCore.Utils.Vector3(vector.X, vector.Y, vector.Z);
        }
        internal static Vector3 V3ToV3(CSCore.Utils.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        private static Listener _listener;
        internal static readonly XAudio2 _xaudio2 = XAudio2.CreateXAudio2();
        private static X3DAudioCore _x3daudio;
        private static XAudio2MasteringVoice _masteringVoice;
        private static int _destinationChannels;

        public Vector3 ListenerPosition
        {
            get
            {
                return V3ToV3(_listener.Position);
            }
            set
            {
                _listener.Position = V3ToV3(value);
            }
        }
        public static void Init()
        {
            _masteringVoice = _xaudio2.CreateMasteringVoice(XAudio2.DefaultChannels, XAudio2.DefaultSampleRate);

            object defaultDevice = _xaudio2.DefaultDevice;
            ChannelMask channelMask;
            if (_xaudio2.Version == XAudio2Version.XAudio2_7)
            {
                var xaudio27 = (XAudio2_7)_xaudio2;
                var deviceDetails = xaudio27.GetDeviceDetails((int)defaultDevice);
                channelMask = deviceDetails.OutputFormat.ChannelMask;
                _destinationChannels = deviceDetails.OutputFormat.Channels;
            }
            else
            {
                channelMask = _masteringVoice.ChannelMask;
                _destinationChannels = _masteringVoice.VoiceDetails.InputChannels;
            }

            _x3daudio = new X3DAudioCore(channelMask);

            _listener = new Listener()
            {
                Position = new CSCore.Utils.Vector3(0, 0, 0),
                OrientFront = new CSCore.Utils.Vector3(0, 0, 1),
                OrientTop = new CSCore.Utils.Vector3(0, 1, 0),
                Velocity = new CSCore.Utils.Vector3(0, 0, 0)
            };
        }
    }
}
