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
        public Guid id;
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
        private Guid _clientID;
        public Guid ClientID
        {
            get { lock (this) { return _clientID; } }
            set { lock (this) { _clientID = value; } }
        }
        public bool Running
        {
            get { return Sender != null && Sender.Running && Receiver != null && Receiver.Running; }
        }
        public TcpClient client;
        private MultiplayerService mpService;
        private MultiplayerSender Sender;
        private MultiplayerReceiver Receiver;
        public HandshakeStage ConnectionStage { get; private set; }

        public Connection(MultiplayerService mp, TcpClient client, HandshakeStage stage)
        {
            mpService = mp;
            if (client == null) { throw new ArgumentNullException(); }
            this.client = client;
            lock (this)
            {
                Receiver = new MultiplayerReceiver(mp, this);
                Sender = new MultiplayerSender(mp, this);
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

        private void SendHandshake(HandshakeStage stage)
        {
            ConnectionStage = stage;
            NetMessage handshakeData = new NetMessage();
            handshakeData.Write((int)stage);
            handshakeData.Write(mpService.ID);
            handshakeData.Write((int)mpService.ListenPort);
            IPAddress NATAddress = mpService.GetNATIP();
            if (NATAddress != null)
                handshakeData.Write(new NetMessage(NATAddress.GetAddressBytes()));
            else
                handshakeData.Write(new NetMessage());
            switch (stage)
            {
                case HandshakeStage.Begin:
                    List<NetMessage> hashMessages = new List<NetMessage>();
                    foreach (KeyValuePair<string, Guid> typeHash in TypeHelper.Helper.GetAssemblyHashes())
                    {
                        NetMessage hashMessage = new NetMessage();
                        hashMessage.Write(typeHash.Key);
                        hashMessage.Write(typeHash.Value);
                        hashMessages.Add(hashMessage);
                    }
                    handshakeData.Write(hashMessages);
                    break;
                case HandshakeStage.AckBegin:
                    List<NetMessage> peerMessages = new List<NetMessage>();
                    foreach (Connection conn in mpService.connectedPeers.Values)
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
            mpService.WriteHandshake(stage, ClientID, handshakeData);
            Sender.WriteToStream(FrameworkMessages.Handshake, handshakeData);
        }
        public void HandleHandshake(NetMessage message)
        {
            HandshakeStage ReceivedStage = (HandshakeStage)message.ReadInt();
            ConnectionStage = ReceivedStage;
            ClientID = message.ReadGuid();
            RemoteDataPort = message.ReadInt();
            RemoteIP = (client.Client.RemoteEndPoint as IPEndPoint).Address;
            NetMessage ipMessage = message.ReadMessage();
            if (ipMessage != null)
            {
                ///TODO: Maybe notify the client of your perception of their address
            }
            switch (ConnectionStage)
            {
                case HandshakeStage.Begin:
                    Dictionary<string, Guid> types = TypeHelper.Helper.GetAssemblyHashes();
                    List<NetMessage> hashMessages = message.ReadList<NetMessage>().ToList();
                    foreach (NetMessage hashMessage in hashMessages)
                    {
                        string name = hashMessage.ReadString();
                        Guid hash = hashMessage.ReadGuid();
                        if (types.ContainsKey(name) && types[name] == hash)
                        {
                            types.Remove(name);
                        }
                    }
                    if (types.Count == 0)
                    {
                        mpService.HandshakeWaiter = new ReplyWaiter(ClientID);
                        new EmptyAsyncDelegate(AsyncAddPeer).BeginInvoke(null, this);
                        SendHandshake(HandshakeStage.AckBegin);
                    }
                    else
                    {
                        DebugPrinter.print("Handshake failed, your plugins are inconsistent");
                        SendHandshake(HandshakeStage.Terminate);
                    }
                    break;
                case HandshakeStage.AckBegin:
                    List<PeerInformation> peerInfo = new List<PeerInformation>();
                    List<Guid> waitOn = new List<Guid>();
                    List<NetMessage> peerMessages = message.ReadList<NetMessage>().ToList();
                    foreach (NetMessage peerMessage in peerMessages)
                    {
                        PeerInformation pi = new PeerInformation();
                        pi.id = peerMessage.ReadGuid();
                        //First response is our peer, don't wait for him
                        waitOn.Add(pi.id);
                        pi.port = peerMessage.ReadInt();
                        pi.ip = new IPAddress(peerMessage.ReadBytes(4));
                        peerInfo.Add(pi);
                    }
                    mpService.HandshakeWaiter = new ReplyWaiter(waitOn.ToArray());
                    foreach (PeerInformation pi in peerInfo)
                    {
                        new AsyncConnectDelegate(AsyncConnect).BeginInvoke(pi, null, this);
                    }
                    new EmptyAsyncDelegate(AsyncWaitResponses).BeginInvoke(null, this);
                    break;
                case HandshakeStage.PartialRequest:
                    SendHandshake(HandshakeStage.PartialResponse);
                    ConnectionStage = HandshakeStage.Completed;
                    mpService.AddClient(this);
                    break;
                case HandshakeStage.PartialResponse:
                    mpService.HandshakeWaiter.AddReply(ClientID);
                    ConnectionStage = HandshakeStage.Completed;
                    mpService.AddClient(this);
                    break;
                case HandshakeStage.Completed:
                    mpService.HandshakeWaiter.AddReply(ClientID);
                    break;
                case HandshakeStage.Terminate:
                    Terminate();
                    break;
                default:
                    break;
            }
            mpService.ReadHandshake(ReceivedStage, ClientID, message);
        }

        delegate void AsyncConnectDelegate(PeerInformation pi);
        void AsyncConnect(PeerInformation pi)
        {
            Connection newConn = new Connection(mpService, new TcpClient(pi.ip.ToString(), pi.port), HandshakeStage.PartialRequest);
        }

        delegate void EmptyAsyncDelegate();
        void AsyncAddPeer()
        {
            mpService.HandshakeWaiter.WaitReplies();
            mpService.AddClient(this);
        }

        void AsyncWaitResponses()
        {
            mpService.HandshakeWaiter.WaitReplies();
            SendHandshake(HandshakeStage.Completed);
            mpService.AddClient(this);
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
