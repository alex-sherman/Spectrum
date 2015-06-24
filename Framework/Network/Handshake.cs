using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.Framework.Network
{
    public enum HandshakeStage
    {
        Wait = 0,
        Begin = 1,
        AckBegin = 2,
        PartialRequest = 3,
        PartialResponse = 4,
        Completed = 5
    }
    public class Handshake
    {
        private static Dictionary<HandshakeStage, List<HandshakeHandler>> handshakeHandlers = new Dictionary<HandshakeStage, List<HandshakeHandler>>();
        public static void RegisterHandshakeHandler(HandshakeStage stage, HandshakeHandler handler)
        {
            if (!handshakeHandlers.ContainsKey(stage)) { handshakeHandlers[stage] = new List<HandshakeHandler>(); }
            handshakeHandlers[stage].Add(handler);
        }
        public static void WriteHandshake(HandshakeStage stage, NetID peer, NetMessage message)
        {
            List<HandshakeHandler> handlers;
            if (!handshakeHandlers.TryGetValue(stage, out handlers)) { return; }
            foreach (HandshakeHandler handler in handlers)
            {
                handler.Write(peer, message);
            }
        }
        public static void ReadHandshake(HandshakeStage stage, NetID peer, NetMessage message)
        {
            List<HandshakeHandler> handlers;
            if (!handshakeHandlers.TryGetValue(stage, out handlers)) { return; }
            foreach (HandshakeHandler handler in handlers)
            {
                handler.Receive(peer, message);
            }
        }
    }
}
