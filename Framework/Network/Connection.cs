using Microsoft.Xna.Framework;
using Spectrum.Framework.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Spectrum.Framework.Network
{
    struct PeerInformation
    {
        public IPAddress ip;
        public int port;
        public NetID id;
    }
    public enum HandshakeStage
    {
        Wait = 0,
        Begin = 1,
        AckBegin = 2,
        PartialRequest = 3,
        PartialResponse = 4,
        Completed = 5,
        Terminate = 6
    }
    public class Connection
    {
        public float PeerSyncTimeout = 5.0f;
        public volatile int RemoteDataPort;
        public volatile IPAddress RemoteIP;
        private List<Connection> partialConnectionWaitOn;
        private NetID _clientID;
        public NetID ClientID
        {
            get { lock (this) { return _clientID; } }
            set { lock (this) { _clientID = value; } }
        }
        public bool Running
        {
            get { return Sender != null && Sender.Running && (Receiver == null || Receiver.Running); }
        }
        public TcpClient client;
        public MultiplayerService MPService;
        private MultiplayerSender Sender;
        private MultiplayerReceiver Receiver;
        public HandshakeStage ConnectionStage { get; private set; }

        private void Init(MultiplayerService mp, HandshakeStage stage)
        {
            MPService = mp;
            lock (this)
            {
                Sender = new MultiplayerSender(this);
            }
            ConnectionStage = stage;
            switch (stage)
            {
                case HandshakeStage.Wait:
                    break;
                case HandshakeStage.Begin:
                    SendHandshake(HandshakeStage.Begin);
                    break;
                case HandshakeStage.PartialRequest:
                    SendHandshake(HandshakeStage.PartialRequest);
                    break;
                default:
                    throw new ArgumentException("Connection cannot be initialized at this stage");
            }
        }

        public Connection(MultiplayerService mp, ulong steamID, HandshakeStage stage)
        {
            Init(mp, stage);
            ClientID = new NetID(steamID);
        }

        public Connection(MultiplayerService mp, TcpClient client, HandshakeStage stage)
        {
            if (client == null) { throw new ArgumentNullException(); }
            this.client = client;
            lock (this)
            {
                Receiver = new MultiplayerReceiver(this);
            }
            Init(mp, stage);
        }

        private void SendHandshake(HandshakeStage stage)
        {
            ConnectionStage = stage;
            NetMessage handshakeData = new NetMessage();
            handshakeData.Write((int)stage);
            handshakeData.Write(MPService.ID);
            handshakeData.Write((int)MPService.ListenPort);
            IPAddress NATAddress = MPService.GetNATIP();
            if (NATAddress != null)
                handshakeData.Write(new NetMessage(NATAddress.GetAddressBytes()));
            else
                handshakeData.Write(new NetMessage());
            switch (stage)
            {
                case HandshakeStage.Begin:
                    List<NetMessage> hashMessages = new List<NetMessage>();
                    foreach (string type in TypeHelper.Helper.GetTypes())
                    {
                        NetMessage hashMessage = new NetMessage();
                        hashMessage.Write(type);
                        hashMessages.Add(hashMessage);
                    }
                    handshakeData.Write(hashMessages);
                    break;
                case HandshakeStage.AckBegin:
                    List<NetMessage> peerMessages = new List<NetMessage>();
                    foreach (Connection conn in MPService.connectedPeers.Values)
                    {
                        NetMessage peerMessage = new NetMessage();
                        peerMessage.Write(conn.ClientID);
                        peerMessage.Write(conn.RemoteDataPort);
                        peerMessage.Write(conn.RemoteIP.GetAddressBytes(), 4);
                        peerMessages.Add(peerMessage);
                    }
                    handshakeData.Write(peerMessages);
                    break;
                default:
                    break;
            }
            MPService.WriteHandshake(stage, ClientID, handshakeData);
            Sender.WriteToStream(FrameworkMessages.Handshake, handshakeData);
        }

        public void Allow()
        {
            if (ConnectionStage == HandshakeStage.Begin)
                SendHandshake(HandshakeStage.AckBegin);
        }
        public void HandleHandshake(NetMessage message)
        {
            HandshakeStage ReceivedStage = (HandshakeStage)message.ReadInt();
            ConnectionStage = ReceivedStage;
            ClientID = message.ReadNetID();
            RemoteDataPort = message.ReadInt();
            if (client != null)
                RemoteIP = (client.Client.RemoteEndPoint as IPEndPoint).Address;
            NetMessage ipMessage = message.ReadMessage();
            if (ipMessage != null)
            {
                ///TODO: Maybe notify the client of your perception of their address
            }
            switch (ConnectionStage)
            {
                case HandshakeStage.Begin:
                    List<string> types = TypeHelper.Helper.GetTypes();
                    List<NetMessage> hashMessages = message.ReadList<NetMessage>().ToList();
                    foreach (NetMessage hashMessage in hashMessages)
                    {
                        string name = hashMessage.ReadString();
                        if (types.Contains(name))
                        {
                            types.Remove(name);
                        }
                    }
                    if (types.Count == 0)
                    {
                        MPService.HandshakeWaiter = new ReplyWaiter(ClientID);
                        new EmptyAsyncDelegate(AsyncAddPeer).BeginInvoke(null, this);
                        MPService.PeerJoinRequested(this, new PeerEventArgs(ClientID));
                    }
                    else
                    {
                        DebugPrinter.print("Handshake failed, your plugins are inconsistent");
                        SendHandshake(HandshakeStage.Terminate);
                    }
                    break;
                case HandshakeStage.AckBegin:
                    List<PeerInformation> peerInfo = new List<PeerInformation>();
                    List<NetID> waitOn = new List<NetID>();
                    waitOn.Add(ClientID);
                    List<NetMessage> peerMessages = message.ReadList<NetMessage>().ToList();
                    foreach (NetMessage peerMessage in peerMessages)
                    {
                        PeerInformation pi = new PeerInformation();
                        pi.id = peerMessage.ReadNetID();
                        waitOn.Add(pi.id);
                        pi.port = peerMessage.ReadInt();
                        pi.ip = new IPAddress(peerMessage.ReadBytes(4));
                        peerInfo.Add(pi);
                    }
                    MPService.HandshakeWaiter = new ReplyWaiter(waitOn.ToArray());
                    new EmptyAsyncDelegate(AsyncWaitResponses).BeginInvoke(null, this);
                    partialConnectionWaitOn = new List<Connection>();
                    partialConnectionWaitOn.Add(this);
                    foreach (PeerInformation pi in peerInfo)
                    {
                        new AsyncConnectDelegate(AsyncConnect).BeginInvoke(pi, null, this);
                    }
                    SendHandshake(HandshakeStage.PartialRequest);
                    break;
                case HandshakeStage.PartialRequest:
                    SendHandshake(HandshakeStage.PartialResponse);
                    ConnectionStage = HandshakeStage.Completed;
                    break;
                case HandshakeStage.PartialResponse:
                    MPService.HandshakeWaiter.AddReply(ClientID);
                    ConnectionStage = HandshakeStage.Completed;
                    break;
                case HandshakeStage.Completed:
                    MPService.AddClient(this);
                    break;
                case HandshakeStage.Terminate:
                    Terminate();
                    break;
                default:
                    break;
            }
            MPService.ReadHandshake(ReceivedStage, ClientID, message);
        }

        delegate void AsyncConnectDelegate(PeerInformation pi);
        void AsyncConnect(PeerInformation pi)
        {
            partialConnectionWaitOn.Add(new Connection(MPService, new TcpClient(pi.ip.ToString(), pi.port), HandshakeStage.PartialRequest));
        }

        delegate void EmptyAsyncDelegate();
        void AsyncAddPeer()
        {
            if (Running)
            {
                MPService.HandshakeWaiter.WaitReplies();
                foreach (Connection pi in partialConnectionWaitOn)
                {
                    pi.SendHandshake(HandshakeStage.Completed);
                }
                MPService.AddClient(this);
            }
        }

        void AsyncWaitResponses()
        {
            if (Running)
            {
                MPService.HandshakeWaiter.WaitReplies();
                SendHandshake(HandshakeStage.Completed);
                MPService.AddClient(this);
            }
        }

        public void SendMessage(byte userType, NetMessage message)
        {
            Sender.WriteToStream(userType, message);
        }

        public void Terminate()
        {
            lock (this)
            {
                if (client != null)
                    client.Close();
                if (Sender != null)
                    Sender.Terminate();
            }
        }


    }
}
