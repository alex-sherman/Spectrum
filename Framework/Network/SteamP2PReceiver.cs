using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Spectrum.Framework.Network
{
    class SteamP2PReceiver
    {
        private MultiplayerService mpService;
        private volatile bool Running;

        public SteamP2PReceiver(MultiplayerService mpService)
        {
            this.mpService = mpService;
            Thread receiverThread = new Thread(new ThreadStart(ListenForData));
            Running = true;
            receiverThread.IsBackground = true;
            receiverThread.Start();
        }

        private void ListenForData()
        {
            uint messageSize;
            while (Running)
            {
                try
                {
                    while (Steamworks.SteamNetworking.IsP2PPacketAvailable(out messageSize))
                    {
                        byte[] buffer = new byte[messageSize];
                        CSteamID steamID;
                        SteamNetworking.ReadP2PPacket(buffer, messageSize, out messageSize, out steamID);
                        MemoryStream data = new MemoryStream(buffer);

                        byte comType = (byte)data.ReadByte();
                        NetMessage header = new NetMessage(data);
                        header.Read<NetID>();

                        NetMessage messageOut = new NetMessage(data);
                        byte testBytes = (byte)data.ReadByte();
                        if (testBytes != 255)
                        {
                        }

                        mpService.ReceiveMessage(comType, new NetID(steamID.m_SteamID), messageOut);
                    }
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {
                    DebugPrinter.print(e.Message);
                }
            }
        }
        public void Terminate()
        {
            Running = false;
        }
    }
}
