using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace Spectrum.Framework.Network
{
    class MultiplayerSender
    {
        struct QueueItem
        {
            public byte EventType;
            public NetMessage toWrite;
        }
        private Semaphore writeSem = new Semaphore(0, 10);
        private TcpClient client;
        private NetworkStream netStream;
        private Queue<QueueItem> dataQueue;
        private Timer timer;
        private Connection conn;
        public bool Running { get; private set; }
        public MultiplayerSender(Connection conn)
        {
            this.conn = conn;
            timer = new Timer(new TimerCallback(SendKeepAlive), null, 1000, 1000);
            dataQueue = new Queue<QueueItem>();
            this.client = conn.client;
            if (client != null)
                this.netStream = client.GetStream();
            Running = true;
            new Action(SendData).BeginInvoke(null, this);
        }

        private void SendData()
        {
            try
            {
                byte[] buffer = new byte[MultiplayerService.MAX_MSG_SIZE];
                while (Running)
                {
                    if (!writeSem.WaitOne(100))
                    {
                        Thread.Sleep(1);
                        Running = !conn.TimedOut;
                        continue;
                    };
                    QueueItem item;
                    lock (dataQueue)
                    {
                        if (dataQueue.Count == 0) { throw new Exception("Write semaphore was signaled without anything to dequeue"); }
                        item = dataQueue.Dequeue();
                    }
                    WriteToStreamImmediately(item.EventType, item.toWrite);
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            finally { conn.Terminate(); }
        }
        public void WriteToStreamImmediately(byte messageType, NetMessage toWrite)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.WriteByte(messageType);
            NetMessage header = new NetMessage();
            header.Write(conn.MPService.ID);
            header.WriteTo(memStream);
            (toWrite ?? new NetMessage()).WriteTo(memStream);
            memStream.WriteByte(255);

            if (netStream != null)
            {
                lock (client)
                {
                    memStream.WriteTo(netStream);
                }
            }
            else if (conn.PeerID.SteamID != null)
            {
                Steamworks.CSteamID steamID = new Steamworks.CSteamID(conn.PeerID.SteamID.Value);
                Steamworks.SteamNetworking.SendP2PPacket(steamID, memStream.GetBuffer(), (uint)memStream.Length, Steamworks.EP2PSend.k_EP2PSendReliable);
            }
        }
        public void WriteToStream(byte messageType, NetMessage toWrite)
        {
            QueueItem item = new QueueItem();
            item.EventType = messageType;
            item.toWrite = toWrite;
            lock (dataQueue)
            {
                try
                {
                    dataQueue.Enqueue(item);
                    writeSem.Release();
                }
                catch (SemaphoreFullException)
                {
                    DebugPrinter.print("Write queue is full");
                }
            }
        }
        private void SendKeepAlive(object state)
        {
            if (conn.ConnectionStage != HandshakeStage.Completed) return;
            List<NetID> peerGuids = conn.MPService.connectedPeers.Keys.ToList();
            NetMessage message = new NetMessage();
            message.Write(peerGuids.Count);
            foreach (NetID guid in peerGuids)
            {
                if (guid != new NetID())
                {
                    message.Write(guid);
                }
            }
            WriteToStream(FrameworkMessages.KeepAlive, message);
        }
        public void Terminate()
        {
            timer.Dispose();
        }
    }
}
