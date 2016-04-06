using Microsoft.Xna.Framework;
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
    public class MultiplayerReceiver
    {
        private NetworkStream netStream;
        private TcpClient client;
        private Connection conn;
        public bool Running { get; private set; }
        public MultiplayerReceiver(Connection conn)
        {
            this.conn = conn;
            this.client = conn.client;
            this.netStream = client.GetStream();
            //new AsyncListenData(ListenForControlData).BeginInvoke(null, this);
            Thread receiverThread = new Thread(new ThreadStart(ListenForControlData));
            receiverThread.IsBackground = true;
            Running = true;
            receiverThread.Start();
        }
        public void ListenForControlData()
        {
            try
            {
                DateTime lastHeard = DateTime.Now;
                netStream.ReadTimeout = 10000;
                byte[] guidBuffer = new byte[16];
                while (Running && client.Connected)
                {
                    byte comType = (byte)netStream.ReadByte();
                    NetMessage header = new NetMessage(netStream);
                    conn.PeerID = header.Read<NetID>();
                    NetMessage message = ReadFromControlStream(netStream);
                    switch (comType)
                    {
                        case 255:
                            throw new InvalidDataException("Bad data from host");
                        default:
                            conn.MPService.ReceiveMessage(comType, conn.PeerID, message);
                            break;
                    }
                    lastHeard = DateTime.Now;
                }
            }
            catch (InvalidDataException) { }
            catch (System.IO.IOException) { }
            finally
            {
                Running = false;
            }
        }

        public NetMessage ReadFromControlStream(Stream netStream)
        {
            NetMessage messageOut = new NetMessage(netStream);
            byte testBytes = (byte)netStream.ReadByte();
            if (testBytes != 255)
            {
                this.conn.Terminate();
            }
            return messageOut;
        }
    }
}
