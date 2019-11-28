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
    struct ConnectionInformation
    {
        public IPAddress ip;
        public int port;
        public NetID id;
    }
    public class Connection
    {
        public const float PEERTIMEOUT = 10.0f;
        private float PeerTimeOut = PEERTIMEOUT;
        public bool TimedOut
        {
            get
            {
                return ConnectionStage != HandshakeStage.Begin && PeerTimeOut <= 0;
            }
        }
        public volatile int RemoteDataPort;
        public volatile IPAddress RemoteIP;
        private List<Connection> partialConnectionWaitOn;
        private NetID _peerID;
        private string _peerNick;
        public string PeerNick
        {
            get { lock (this) { return _peerNick; } }
            set { lock (this) { _peerNick = value; } }
        }
        public NetID PeerID
        {
            get { lock (this) { return _peerID; } }
            set { lock (this) { _peerID = value; } }
        }
        private bool _running = true;
        public bool Running
        {
            get { return !TimedOut && _running; }
        }
        public TcpClient client;
        public MultiplayerService MPService;
        private MultiplayerSender Sender;
        private MultiplayerReceiver Receiver;
        private Action<bool> onConnectCallback = null;
        public HandshakeStage ConnectionStage { get; private set; }

        private void Init(MultiplayerService mp, HandshakeStage stage, Action<bool> callback)
        {
            onConnectCallback = callback;
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

        public Connection(MultiplayerService mp, ulong steamID, HandshakeStage stage, Action<bool> callback = null)
        {
            Init(mp, stage, callback);
            PeerID = new NetID(steamID);
        }

        public Connection(MultiplayerService mp, TcpClient client, HandshakeStage stage, Action<bool> callback = null)
        {
            if (client == null) { throw new ArgumentNullException(); }
            this.client = client;
            lock (this)
            {
                Receiver = new MultiplayerReceiver(this);
            }
            Init(mp, stage, callback);
        }

        public void Update(GameTime time)
        {
            PeerTimeOut -= time.ElapsedGameTime.Milliseconds / 1000.0f;
        }

        public void ResetTimeout() { PeerTimeOut = PEERTIMEOUT; }
        public void Allow()
        {
            if (ConnectionStage == HandshakeStage.Begin)
                SendHandshake(HandshakeStage.AckBegin);
        }

        private void SendHandshake(HandshakeStage stage)
        {
            ConnectionStage = stage;
            NetMessage handshakeData = new NetMessage();
            handshakeData.Write((int)stage);
            handshakeData.Write(MPService.ID);
            handshakeData.Write(MPService.Nick);
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
                    foreach (string type in TypeHelper.Model.Select(t => t.Name))
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
                        peerMessage.Write(conn.PeerID);
                        if (conn.PeerID.Guid != null)
                        {
                            peerMessage.Write(conn.RemoteDataPort);
                            peerMessage.Write(conn.RemoteIP.GetAddressBytes(), 4);
                        }
                        peerMessages.Add(peerMessage);
                    }
                    handshakeData.Write(peerMessages);
                    break;
                case HandshakeStage.Completed:
                    MPService.AddClient(this);
                    if (onConnectCallback != null)
                    {
                        onConnectCallback(true);
                        onConnectCallback = null;
                    }
                    break;
                default:
                    break;
            }
            Handshake.WriteHandshake(stage, PeerID, handshakeData);
            Sender.WriteToStream(FrameworkMessages.Handshake, handshakeData);
        }

        public void HandleHandshake(NetMessage message)
        {
            HandshakeStage ReceivedStage = (HandshakeStage)message.Read<int>();
            ConnectionStage = ReceivedStage;
            PeerID = message.Read<NetID>();
            PeerNick = message.Read<string>();
            RemoteDataPort = message.Read<int>();
            if (client != null)
                RemoteIP = (client.Client.RemoteEndPoint as IPEndPoint).Address;
            NetMessage ipMessage = message.Read<NetMessage>();
            if (ipMessage != null)
            {
                ///TODO: Maybe notify the client of your perception of their address
            }
            switch (ConnectionStage)
            {
                case HandshakeStage.Begin:
                    List<string> types = TypeHelper.Model.Select(t => t.Name).ToList();
                    List<NetMessage> hashMessages = message.Read<List<NetMessage>>().ToList();
                    foreach (NetMessage hashMessage in hashMessages)
                    {
                        string name = hashMessage.Read<string>();
                        if (types.Contains(name))
                        {
                            types.Remove(name);
                        }
                    }
                    if (types.Count == 0)
                    {
                        MPService.HandshakeWaiter = new ReplyWaiter(PeerID);
                        //This should optionally transition to AckBegin
                        MPService.PeerJoinRequested(this, new PeerEventArgs(PeerID));
                    }
                    else
                    {
                        DebugPrinter.Print("Handshake failed, your plugins are inconsistent");
                        Terminate();
                    }
                    break;
                case HandshakeStage.AckBegin:
                    List<ConnectionInformation> peerInfo = new List<ConnectionInformation>();
                    List<NetID> waitOn = new List<NetID>();
                    waitOn.Add(PeerID);
                    List<NetMessage> peerMessages = message.Read<List<NetMessage>>().ToList();
                    foreach (NetMessage peerMessage in peerMessages)
                    {
                        ConnectionInformation pi = new ConnectionInformation();
                        pi.id = peerMessage.Read<NetID>();
                        waitOn.Add(pi.id);
                        if (pi.id.Guid != null)
                        {
                            pi.port = peerMessage.Read<int>();
                            pi.ip = new IPAddress(peerMessage.ReadBytes(4));
                        }
                        peerInfo.Add(pi);
                    }
                    MPService.HandshakeWaiter = new ReplyWaiter(true, waitOn.ToArray());
                    new Action(AsyncWaitResponses).BeginInvoke(null, this);
                    partialConnectionWaitOn = new List<Connection>();
                    partialConnectionWaitOn.Add(this);
                    foreach (ConnectionInformation pi in peerInfo)
                    {
                        new Action<ConnectionInformation>(AsyncConnect).BeginInvoke(pi, null, this);
                    }
                    SendHandshake(HandshakeStage.PartialRequest);
                    break;
                case HandshakeStage.PartialRequest:
                    SendHandshake(HandshakeStage.PartialResponse);
                    break;
                case HandshakeStage.PartialResponse:
                    MPService.HandshakeWaiter.AddReply(PeerID);
                    break;
                case HandshakeStage.Completed:
                    MPService.AddClient(this);
                    break;
                default:
                    break;
            }
            Handshake.ReadHandshake(ReceivedStage, PeerID, message);
        }

        void AsyncConnect(ConnectionInformation pi)
        {
            Connection newConnection = null;
            if (pi.id.Guid != null)
                newConnection = new Connection(MPService, new TcpClient(pi.ip.ToString(), pi.port), HandshakeStage.PartialRequest);
            else if (pi.id.SteamID != null)
                newConnection = new Connection(MPService, pi.id.SteamID.Value, HandshakeStage.PartialRequest);
            if (newConnection != null)
            {
                partialConnectionWaitOn.Add(newConnection);
                lock (MPService.allConnections)
                    MPService.allConnections.Add(newConnection);
            }
        }

        void AsyncWaitResponses()
        {
            if (Running)
            {
                MPService.HandshakeWaiter.WaitReplies();
                foreach (Connection pi in partialConnectionWaitOn)
                {
                    pi.SendHandshake(HandshakeStage.Completed);
                }
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
                if (onConnectCallback != null)
                {
                    onConnectCallback(false);
                    onConnectCallback = null;
                }
                _running = false;
                if (Sender != null)
                {
                    try
                    {
                        Sender.WriteToStreamImmediately(FrameworkMessages.Termination, null);
                    }
                    catch { }
                    finally
                    {
                        MPService.ReceiveMessage(FrameworkMessages.Termination, PeerID, new NetMessage());
                        Sender.Terminate();
                    }
                }
                if (client != null)
                    client.Close();
            }
        }


    }
}
