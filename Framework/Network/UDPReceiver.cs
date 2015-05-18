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
    class UDPReceiver
    {
        private MultiplayerService mpService;
        private UdpClient client;
        private IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);

        public UDPReceiver(MultiplayerService mpService, UdpClient client)
        {
            this.mpService = mpService;
            this.client = client;
            Thread receiverThread = new Thread(new ThreadStart(ListenForData));
            receiverThread.IsBackground = true;
            receiverThread.Start();
        }

        private void ListenForData()
        {
            while (true)
            {
                try
                {
                    NetMessage data = new NetMessage(client.Receive(ref endpoint));
                    byte comType = data.ReadByte();
                    Guid peerGuid = data.ReadGuid();
                    NetMessage userMessage = data.ReadMessage();
                    switch (comType)
                    {
                        case FrameworkMessages.KeepAlive:
                            break;
                        default:
                            mpService.ReceiveMessage(comType, peerGuid, userMessage);
                            break;
                    }

                }
                catch (Exception e)
                {
                    DebugPrinter.print(e.Message);
                }
            }
        }
    }
}
