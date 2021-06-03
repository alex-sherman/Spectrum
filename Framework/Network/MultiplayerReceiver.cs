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
        private NetworkStream NetStream;
        private TcpClient Client;
        private Connection Connection;
        public bool Running { get; private set; }
        public MultiplayerReceiver(Connection conn)
        {
            Connection = conn;
            Client = conn.Client;
            NetStream = Client.GetStream();
            //new AsyncListenData(ListenForControlData).BeginInvoke(null, this);
            Thread receiverThread = new Thread(new ThreadStart(ListenForControlData))
            {
                IsBackground = true
            };
            Running = true;
            receiverThread.Start();
        }
        public void ListenForControlData()
        {
            try
            {
                DateTime lastHeard = DateTime.Now;
                NetStream.ReadTimeout = 10000;
                byte[] guidBuffer = new byte[16];
                while (Running && Client.Connected)
                {
                    byte comType = (byte)NetStream.ReadByte();
                    NetMessage header = new NetMessage(NetStream);
                    Connection.PeerID = header.Read<NetID>();
                    NetMessage message = ReadFromControlStream(NetStream);
                    switch (comType)
                    {
                        case 255:
                            throw new InvalidDataException("Bad data from host");
                        default:
                            Connection.MPService.ReceiveMessage(comType, Connection.PeerID, message);
                            break;
                    }
                    lastHeard = DateTime.Now;
                }
            }
            catch (InvalidDataException) { }
            catch (System.IO.IOException) { }
            catch (NullReferenceException) { }
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
                this.Connection.Terminate();
            }
            return messageOut;
        }
    }
}
