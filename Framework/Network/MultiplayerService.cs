using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Microsoft.Xna.Framework;
using Spectrum.Framework.Network;
using System.Threading;
using Mono.Nat.Upnp;
using Mono.Nat;
using System.Reflection;

namespace Spectrum.Framework.Network
{
    public class FrameworkMessages
    {
        public const byte Handshake = 0;
        public const byte KeepAlive = 1;
        public const byte ShowCreate = 3;
        public const byte EntityCreation = 4;
        public const byte EntityDeletion = 5;
        public const byte EntityReplication = 6;
        public const byte FunctionReplication = 7;
        public const byte Termination = 8;
        public const byte RequestMutex = 9;
        public const byte ReplyMutex = 10;
        public const byte RequestPositions = 11;
        public const byte SetTilePosition = 12;
        public const byte RequestTile = 13;
        public const byte TileData = 14;
    }

    public class PeerEventArgs : EventArgs
    {
        public NetID PeerID { get; private set; }
        public PeerEventArgs(NetID peerID)
        {
            PeerID = peerID;
        }
    }

    public delegate void NetMessageHandler(NetID peerID, NetMessage message);
    struct MultiplayerMessage
    {
        public byte MessageType;
        public NetID PeerID;
        public NetMessage Message;
        public MultiplayerMessage(byte messageType, NetID peerGuid, NetMessage message)
        {
            MessageType = messageType;
            PeerID = peerGuid;
            Message = message;
        }
    }
    public class MultiplayerService : IDebug
    {
        public const uint MAX_MSG_SIZE = 70000;

        #region Steam Stuff
        private Steamworks.CallResult<Steamworks.LobbyCreated_t> lobbyCreated;
        private void _lobbyCreatedCallback(Steamworks.LobbyCreated_t lobbyCreated, bool failed)
        {

        }
        private Steamworks.Callback<Steamworks.GameLobbyJoinRequested_t> lobbyJoinRequested;
        private void _lobbyJoinRequestedCallback(Steamworks.GameLobbyJoinRequested_t lobbyCreated)
        {
            new Action<ulong, Action<bool>>(_connectSteam).BeginInvoke(lobbyCreated.m_steamIDFriend.m_SteamID, null, null, this);
        }
        private Steamworks.Callback<Steamworks.P2PSessionRequest_t> p2pSessionRequest;
        private void _p2pSessionRequestCallback(Steamworks.P2PSessionRequest_t sessionRequest)
        {
            Steamworks.SteamNetworking.AcceptP2PSessionWithUser(sessionRequest.m_steamIDRemote);
        }
        private SteamP2PReceiver steamReceiver;
        #endregion

        private Dictionary<byte, List<NetMessageHandler>> userMessageHandlers = new Dictionary<byte, List<NetMessageHandler>>();
        private List<MultiplayerMessage> receivedMessages = new List<MultiplayerMessage>();
        internal List<Connection> allConnections = new List<Connection>();
        private Dictionary<NetID, Connection> _connectedPeers = new Dictionary<NetID, Connection>();
        public Dictionary<NetID, Connection> connectedPeers
        {
            get
            {
                lock (this)
                {
                    return _connectedPeers.ToDictionary(entry => entry.Key,
                                   entry => entry.Value);
                }
            }
        }

        #region Events
        /// <summary>
        /// If no event handler is specified, all connections will be dropped!
        /// This event handler should call Connection.Allow()
        /// </summary>
        public event Action<Connection, PeerEventArgs> OnPeerJoinRequested;
        public void PeerJoinRequested(Connection connection, PeerEventArgs args)
        {
            if (OnPeerJoinRequested != null)
                OnPeerJoinRequested(connection, args);
            else
            {
                DebugPrinter.print("Dropping client because no OnPeerJoinRequested has been specified");
                connection.Terminate();
            }
        }
        public event Action<PeerEventArgs> OnPeerJoined;
        public event Action<PeerEventArgs> OnPeerLeft;
        #endregion

        private UDPReceiver udpReceiver;
        private UDPSender udpSender;
        private UdpClient udpClient;
        private List<TcpListener> serverListeners = new List<TcpListener>();
        public ReplyWaiter HandshakeWaiter;
        public bool Listening { get { return serverListeners != null; } }
        public bool Connected { get { return connectedPeers.Values.Count > 0; } }
        private INatDevice NatDevice = null;
        private IPAddress _natIP = null;
        public bool HasNat { get { return NatDevice != null; } }
        public int ListenPort { get; private set; }
        public NetID ID { get; private set; }
        public string Nick { get; private set; }
        public string GetPeerNick(NetID peer)
        {
            return connectedPeers[peer].PeerNick;
        }

        public MultiplayerService(NetID ID, string nick)
        {
            this.ID = ID;
            Nick = nick;
            NetworkMutex.Init(this);
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
            RegisterMessageCallback(FrameworkMessages.Handshake, HandleHandshake);
            RegisterMessageCallback(FrameworkMessages.Termination, HandleTermination);
            RegisterMessageCallback(FrameworkMessages.KeepAlive, HandleKeepAlive);
            if (SpectrumGame.Game.UsingSteam)
            {
                Nick = Steamworks.SteamFriends.GetPersonaName();
                steamReceiver = new SteamP2PReceiver(this);
                lobbyCreated = Steamworks.CallResult<Steamworks.LobbyCreated_t>.Create(_lobbyCreatedCallback);
                lobbyJoinRequested = Steamworks.Callback<Steamworks.GameLobbyJoinRequested_t>.Create(_lobbyJoinRequestedCallback);
                lobbyJoinRequested.Register(_lobbyJoinRequestedCallback);
                p2pSessionRequest = Steamworks.Callback<Steamworks.P2PSessionRequest_t>.Create(_p2pSessionRequestCallback);
                p2pSessionRequest.Register(_p2pSessionRequestCallback);
            }
        }
        private void DeviceFound(object sender, DeviceEventArgs args)
        {
            NatDevice = args.Device;
            _natIP = NatDevice.GetExternalIP();
            DebugPrinter.print("Found NAT device");
        }
        public void BeginListening(int port = 27007)
        {
            if (SpectrumGame.Game.UsingSteam)
            {
                var output = Steamworks.SteamMatchmaking.CreateLobby(Steamworks.ELobbyType.k_ELobbyTypeFriendsOnly, 4);
                lobbyCreated.Set(output, _lobbyCreatedCallback);
            }
            new Action<int>(_beginListening).BeginInvoke(port, null, this);
        }
        private void _beginListening(int port)
        {
            udpClient = new UdpClient(port);
            ListenPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
            udpSender = new UDPSender(ID, udpClient);
            udpReceiver = new UDPReceiver(this, udpClient);
            bool natSuccess = false;
            if (HasNat)
            {
                try
                {
                    NatDevice.CreatePortMap(new Mapping(Protocol.Tcp, ListenPort, ListenPort));
                    NatDevice.CreatePortMap(new Mapping(Protocol.Udp, ListenPort, ListenPort));
                    natSuccess = true;
                }
                catch { }
            }
            if (!natSuccess)
            {
                DebugPrinter.print("Could not forward port using UpNP");
            }
            foreach (IPAddress ip in GetLocalIP())
            {
                TcpListener toAdd = new TcpListener(ip, ListenPort);
                toAdd.Start();
                serverListeners.Add(toAdd);
            }
            while (true)
            {
                try
                {
                    TcpListener toAccept = null;
                    foreach (TcpListener listener in serverListeners)
                    {
                        if (listener.Pending()) { toAccept = listener; }
                    }
                    if (toAccept == null) { Thread.Sleep(500); continue; }
                    TcpClient newClient = toAccept.AcceptTcpClient();
                    //The tcp client may get closed right away
                    DebugPrinter.print("Someone connected at " + newClient.Client.RemoteEndPoint.ToString());
                    lock (allConnections)
                        allConnections.Add(new Connection(this, newClient, HandshakeStage.Wait));
                }
                catch { }
            }
        }

        public void Connect(string hostname, int port, Action<bool> callback = null)
        {
            if (!Listening) { throw new Exception("Can't connect to anyone unless you're listening for connections"); }
            new Action<string, int, Action<bool>>(_connect).BeginInvoke(hostname, port, callback, null, this);
        }
        public void Connect(ulong steamID, Action<bool> callback = null)
        {
            new Action<ulong, Action<bool>>(_connectSteam).BeginInvoke(steamID, callback, null, this);
        }
        private void _connect(string hostname, int port, Action<bool> callback)
        {
            try
            {
                lock (allConnections)
                    allConnections.Add(new Connection(this, new TcpClient(hostname, port), HandshakeStage.Begin, callback));
            }
            catch (SocketException e)
            {
                DebugPrinter.print(e.Message);
                callback(false);
                return;
            }
            DebugPrinter.print("Connected to " + hostname + ":" + port);
        }
        private void _connectSteam(ulong steamID, Action<bool> callback)
        {
            try
            {
                lock (allConnections)
                    allConnections.Add(new Connection(this, steamID, HandshakeStage.Begin, callback));
            }
            catch (SocketException e)
            {
                DebugPrinter.print(e.Message);
                return;
            }
        }

        public static List<IPAddress> GetLocalIP()
        {
            List<IPAddress> output = new List<IPAddress>();
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    output.Add(ip);
                }
            }
            return output;
        }
        public IPAddress GetNATIP()
        {
            return _natIP;
        }

        #region Framework Message Handlers
        private void HandleHandshake(NetID peer, NetMessage message)
        {
            Connection conn = allConnections.Find((Connection other) => (other.PeerID == peer));
            if (conn == null && peer.SteamID != null)
            {
                conn = new Connection(this, peer.SteamID.Value, HandshakeStage.Wait);
                lock (allConnections)
                {
                    allConnections.Add(conn);
                }
            }
            if (conn != null)
                conn.HandleHandshake(message);
        }
        private void HandleTermination(NetID peer, NetMessage message)
        {
            lock(_connectedPeers)
            {
                _connectedPeers.Remove(peer);
            }
        }
        private void HandleKeepAlive(NetID peer, NetMessage message)
        {
            //TODO: Handle the case where one peer drops a connection to a single other peer
            //Everyone should probably just panic and drop all connections
            List<NetID> peerGuids = new List<NetID>();
            int count = message.Read<int>();
            for (int i = 0; i < count; i++)
            {
                peerGuids.Add(message.Read<NetID>());
            }
            List<NetID> missingPeers = peerGuids.ToList();
            missingPeers.Remove(ID);
            foreach (NetID knownPeer in connectedPeers.Keys)
            {
                missingPeers.Remove(knownPeer);
            }
            if (missingPeers.Count() != 0)
            {
                DebugPrinter.print("Clients mismatched");
                //TODO: Start a timer or something
            }
        }
        #endregion

        public void RegisterMessageCallback(byte userType, NetMessageHandler callback)
        {
            if (!userMessageHandlers.ContainsKey(userType))
                userMessageHandlers[userType] = new List<NetMessageHandler>();
            userMessageHandlers[userType].Add(callback);
        }
        public void SendMessage(byte userType, NetMessage message, NetID peerDestination = default(NetID))
        {
            lock (this)
            {
                if (peerDestination == default(NetID))
                {
                    foreach (Connection conn in connectedPeers.Values)
                    {
                        conn.SendMessage(userType, message);
                    }
                }
                else
                {
                    Connection conn = connectedPeers[peerDestination];
                    if (conn != null) { conn.SendMessage(userType, message); }
                }
            }
        }
        public void SendUnreliableMessage(byte userType, NetMessage message, NetID peerDestination = default(NetID))
        {
            lock (this)
            {
                if (peerDestination == default(NetID))
                {
                    foreach (Connection conn in connectedPeers.Values)
                    {
                        IPEndPoint endpoint = (IPEndPoint)conn.client.Client.RemoteEndPoint;
                        endpoint.Port = conn.RemoteDataPort;
                        udpSender.SendMessage(endpoint, userType, message);
                    }
                }
                else
                {
                    Connection conn = connectedPeers[peerDestination];
                    if (conn != null)
                    {
                        IPEndPoint endpoint = (IPEndPoint)conn.client.Client.RemoteEndPoint;
                        endpoint.Port = conn.RemoteDataPort;
                        udpSender.SendMessage(endpoint, userType, message);
                    }
                }
            }
        }

        public void MakeCallbacks(GameTime time)
        {
            //Update the client sync value, changes to true if all other clients are connected to
            //the client in question
            foreach (Connection peer in allConnections.ToList())
            {
                peer.Update(time);
                if (!peer.Running)
                {
                    RemoveClient(peer);
                }
            }
            lock (receivedMessages)
            {
                while (receivedMessages.Count > 0)
                {
                    MultiplayerMessage received = receivedMessages[0];
                    receivedMessages.RemoveAt(0);
                    if (received.MessageType != FrameworkMessages.Handshake && !allConnections.Any(conn => conn.PeerID == received.PeerID))
                        continue;

                    Connection connection = allConnections.Find((Connection other) => (other.PeerID == received.PeerID));
                    if (connection != null)
                        connection.ResetTimeout();

                    List<NetMessageHandler> handlers;
                    if (userMessageHandlers.TryGetValue(received.MessageType, out handlers))
                    {
                        foreach (NetMessageHandler handler in handlers)
                        {
#if !DEBUG
                            try
                            {
                                handler(received.PeerID, Serialization.Copy(received.Message));
                            }
                            catch (Exception e)
                            {
                               DebugPrinter.print("Message handler " + received.MessageType + " failed: " + e.Message);
                            }
#else
                                handler(received.PeerID, Serialization.Copy(received.Message));
#endif
                        }
                    }
                }
            }
        }
        public void ReceiveMessage(byte messageType, NetID peerGuid, NetMessage message)
        {
            lock (receivedMessages) { receivedMessages.Add(new MultiplayerMessage(messageType, peerGuid, message)); }
        }

        public void RemoveClient(Connection conn)
        {
            lock (this)
            {
                DebugPrinter.print("Peer disconnected " + conn.RemoteIP);
                _connectedPeers.Remove(conn.PeerID);
                if (OnPeerLeft != null)
                    OnPeerLeft(new PeerEventArgs(conn.PeerID));
                lock (allConnections)
                    allConnections.Remove(conn);
                conn.Terminate();
                ReplyWaiter.PeerRemoved(conn.PeerID);
            }
        }
        public bool AddClient(Connection conn)
        {
            lock (this)
            {
                if (conn.PeerID == new NetID())
                {
                    DebugPrinter.print("Attempted to add a connection that hasn't handshaken yet");
                    return false;
                }
                if (!_connectedPeers.ContainsKey(conn.PeerID))
                {
                    _connectedPeers[conn.PeerID] = conn;
                    if (OnPeerJoined != null)
                        OnPeerJoined(new PeerEventArgs(conn.PeerID));
                    return true;
                }
                else
                {
                    DebugPrinter.print("A connection to an already attached peer was abandoned");
                    return false;
                }
            }
        }
        public void Dispose()
        {
            if (steamReceiver != null)
                steamReceiver.Terminate();
            if (Listening)
            {
                udpReceiver = null;
                udpSender = null;
                foreach (TcpListener serverListener in serverListeners)
                {
                    serverListener.Stop();
                }
            }
            foreach (Connection conn in connectedPeers.Values)
            {
                conn.Terminate();
            }
            if (HasNat && ListenPort != -1)
            {
                try
                {
                    NatDevice.DeletePortMap(new Mapping(Protocol.Tcp, ListenPort, ListenPort));
                }
                catch { }
            }
        }

        public string Debug()
        {
            string output = "";
            foreach (Connection conn in connectedPeers.Values)
            {
                output += "" + conn.PeerID + "\n";
            }
            return output;
        }
        public void DebugDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}
