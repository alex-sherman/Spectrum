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
        public const byte EntityMessage = 2;
        public const byte ShowCreate = 3;
        public const byte EntityCreation = 4;
        public const byte EntityDeletion = 5;
        public const byte EntityReplication = 6;
        public const byte Termination = 7;
        public const byte FunctionReplication = 8;
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

    public struct HandshakeHandler
    {
        public Action<NetID, NetMessage> Receive;
        public Action<NetID, NetMessage> Write;
        public HandshakeHandler(Action<NetID, NetMessage> write, Action<NetID, NetMessage> receive)
        {
            Write = write;
            Receive = receive;
        }
    }

    public delegate void NetMessageHandler(NetID peerID, NetMessage message);
    struct MultiplayerMessage
    {
        public byte MessageType;
        public NetID PeerGuid;
        public NetMessage Message;
        public MultiplayerMessage(byte messageType, NetID peerGuid, NetMessage message)
        {
            MessageType = messageType;
            PeerGuid = peerGuid;
            Message = message;
        }
    }
    public class MultiplayerService : IDebug
    {
        #region Steam Callbacks
        public static Steamworks.CallResult<Steamworks.LobbyCreated_t> lobbyCreated;
        #endregion
        private Dictionary<byte, NetMessageHandler> userMessageHandlers = new Dictionary<byte, NetMessageHandler>();
        private Dictionary<HandshakeStage, List<HandshakeHandler>> handshakeHandlers = new Dictionary<HandshakeStage, List<HandshakeHandler>>();
        private List<MultiplayerMessage> receivedMessages = new List<MultiplayerMessage>();
        private List<Connection> allConnections = new List<Connection>();
        private RealDict<NetID, Connection> _connectedPeers = new RealDict<NetID, Connection>();
        public RealDict<NetID, Connection> connectedPeers
        {
            get { lock (this) { return _connectedPeers.Copy(); } }
        }
        public EventHandler<PeerEventArgs> OnPeerAdded;
        public EventHandler<PeerEventArgs> OnPeerRemoved;
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
        private int listenPort = -1;
        public int ListenPort { get { return listenPort; } }
        public NetID ID { get { return SpectrumGame.Game.ID; } }

        public MultiplayerService()
        {
            NetworkMutex.Init(this);
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.StartDiscovery();
            if (SpectrumGame.Game.UsingSteam)
                lobbyCreated = Steamworks.CallResult<Steamworks.LobbyCreated_t>.Create(_lobbyCreatedCallback);
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
            new AsyncAcceptConnect(_beginListening).BeginInvoke(port, null, this);
        }
        private void _beginListening(int port)
        {
            udpClient = new UdpClient(port);
            listenPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
            udpSender = new UDPSender(ID, udpClient);
            udpReceiver = new UDPReceiver(this, udpClient);
            bool natSuccess = false;
            if (HasNat)
            {
                try
                {
                    NatDevice.CreatePortMap(new Mapping(Protocol.Tcp, listenPort, listenPort));
                    NatDevice.CreatePortMap(new Mapping(Protocol.Udp, listenPort, listenPort));
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
                TcpListener toAdd = new TcpListener(ip, listenPort);
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

        public static void _lobbyCreatedCallback(Steamworks.LobbyCreated_t lobbyCreated, bool failed)
        {

        }

        public void StopListening()
        {
            if (Listening)
            {
                udpReceiver = null;
                udpSender = null;
                foreach (TcpListener serverListener in serverListeners)
                {
                    serverListener.Stop();
                }
            }
        }
        private delegate void AsyncConnect(string hostname, int port);
        private delegate void AsyncAcceptConnect(int port);

        public void Connect(string hostname, int port)
        {
            if (!Listening) { throw new Exception("Can't connect to anyone unless you're listening for connections"); }
            new AsyncConnect(_connect).BeginInvoke(hostname, port, null, this);
        }
        private void _connect(string hostname, int port)
        {
            try
            {
                lock (allConnections)
                    allConnections.Add(new Connection(this, new TcpClient(hostname, port), HandshakeStage.Begin));
            }
            catch (SocketException e)
            {
                DebugPrinter.print(e.Message);
                return;
            }
            DebugPrinter.print("Connected to " + hostname + ":" + port);
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

        public Dictionary<string, Guid> GetAssemblyHashes()
        {
            Dictionary<string, Guid> output = new Dictionary<string, Guid>();
            //foreach (Type type in Types.Values)
            //{
            //    output[type.Name] = type.Assembly.ManifestModule.ModuleVersionId;
            //}
            return output;
        }

        public void RegisterMessageCallback(byte userType, NetMessageHandler callback)
        {
            userMessageHandlers[userType] = callback;
        }
        public void RegisterHandshakeHandler(HandshakeStage stage, HandshakeHandler handler)
        {
            if (!handshakeHandlers.ContainsKey(stage)) { handshakeHandlers[stage] = new List<HandshakeHandler>(); }
            handshakeHandlers[stage].Add(handler);
        }
        public void WriteHandshake(HandshakeStage stage, NetID peer, NetMessage message)
        {
            List<HandshakeHandler> handlers;
            if (!handshakeHandlers.TryGetValue(stage, out handlers)) { return; }
            foreach (HandshakeHandler handler in handlers)
            {
                handler.Write(peer, message);
            }
        }
        public void ReadHandshake(HandshakeStage stage, NetID peer, NetMessage message)
        {
            List<HandshakeHandler> handlers;
            if (!handshakeHandlers.TryGetValue(stage, out handlers)) { return; }
            foreach (HandshakeHandler handler in handlers)
            {
                handler.Receive(peer, message);
            }
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
                if (!peer.Running)
                {
                    RemoveClient(peer);
                }
                if (peer.ConnectionStage == HandshakeStage.Completed) { continue; }
                else
                {
                    peer.PeerSyncTimeout -= time.ElapsedGameTime.Milliseconds / 1000.0f;
                    if (peer.PeerSyncTimeout <= 0)
                    {
                        DebugPrinter.print("Dropping client who couldn't connect to all of the already connected peers");
                        RemoveClient(peer);
                    }
                }
            }
            lock (this)
            {
                while (receivedMessages.Count > 0)
                {
                    MultiplayerMessage received = receivedMessages[0];
                    receivedMessages.RemoveAt(0);

                    NetMessageHandler handler;
                    if (userMessageHandlers.TryGetValue(received.MessageType, out handler))
                    {
                        try
                        {
                            handler(received.PeerGuid, received.Message);
                        }
                        catch (Exception e)
                        {
                            DebugPrinter.print("Message handler " + received.MessageType + " failed: " + e.Message);
                        }
                    }
                }
            }
        }
        public void ReceiveMessage(byte messageType, NetID peerGuid, NetMessage message)
        {
            lock (this) { receivedMessages.Add(new MultiplayerMessage(messageType, peerGuid, message)); }
        }

        public void RemoveClient(Connection conn)
        {
            lock (this)
            {
                DebugPrinter.print("Peer disconnected " + conn.RemoteIP);
                _connectedPeers.Remove(conn.ClientID);
                lock (allConnections)
                    allConnections.Remove(conn);
                if (OnPeerRemoved != null) { OnPeerRemoved(this, new PeerEventArgs(conn.ClientID)); }
                conn.Terminate();
                ReplyWaiter.PeerRemoved(conn.ClientID);
            }
        }
        public bool AddClient(Connection conn)
        {
            lock (this)
            {
                if (conn.ClientID == new NetID())
                {
                    DebugPrinter.print("Attempted to add a connection that hasn't handshaken yet");
                    return false;
                }
                if (_connectedPeers[conn.ClientID] == null)
                {
                    _connectedPeers[conn.ClientID] = conn;
                    if (OnPeerAdded != null) { OnPeerAdded(this, new PeerEventArgs(conn.ClientID)); }
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
            StopListening();
            foreach (Connection conn in connectedPeers.Values)
            {
                conn.Terminate();
            }
            if (HasNat && listenPort != -1)
            {
                try
                {
                    NatDevice.DeletePortMap(new Mapping(Protocol.Tcp, listenPort, listenPort));
                }
                catch { }
            }
        }

        public string Debug()
        {
            string output = "";
            foreach (Connection conn in connectedPeers.Values)
            {
                output += "" + conn.ClientID + "\n";
            }
            return output;
        }

        public void DebugDraw(GameTime gameTime, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
        }
    }
}
