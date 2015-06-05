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
        private MultiplayerService mpService;
        public bool Running { get; private set; }
        public MultiplayerReceiver(MultiplayerService mpService, Connection conn)
        {
            this.mpService = mpService;
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
                while (Running && client.Connected)
                {
                    byte comType = (byte)netStream.ReadByte();
                    NetMessage message = ReadFromControlStream(netStream);
                    switch (comType)
                    {
                        case FrameworkMessages.Handshake:
                            conn.HandleHandshake(message);
                            break;
                        case FrameworkMessages.Termination:
                            throw new Exception("Peer has terminated the connection");
                        //TODO: Handle the case where one peer drops a connection to a single other peer
                        //Everyone should probably just panic and drop all connections
                        case FrameworkMessages.KeepAlive:
                            List<NetID> peerGuids = new List<NetID>();
                            int count = message.ReadInt();
                            for(int i = 0; i < count; i ++)
                            {
                                peerGuids.Add(message.ReadNetID());
                            }
                            List<NetID> missingPeers = peerGuids.ToList();
                            missingPeers.Remove(mpService.ID);
                            foreach (NetID knownPeer in mpService.connectedPeers.Keys)
                            {
                                missingPeers.Remove(knownPeer);
                            }
                            if (missingPeers.Count() != 0)
                            {
                                DebugPrinter.print("Clients mismatched");
                                //TODO: Start a timer or something
                            }
                            break;
                        case 255:
                            throw new InvalidDataException("Bad data from host");
                        default:
                            mpService.ReceiveMessage(comType, conn.ClientID, message);
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
