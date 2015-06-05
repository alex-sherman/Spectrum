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
                    if (!writeSem.WaitOne())
                    {
                        Thread.Sleep(1);
                        continue;
                    };
                    QueueItem item;
                    lock (dataQueue)
                    {
                        if (dataQueue.Count == 0) { throw new Exception("Write semaphore was signaled without anything to dequeue"); }
                        item = dataQueue.Dequeue();
                    }
                    MemoryStream toWrite = new MemoryStream();
                    toWrite.WriteByte((byte)item.EventType);
                    NetMessage header = new NetMessage();
                    header.Write(conn.MPService.ID);
                    header.WriteTo(toWrite);
                    item.toWrite.WriteTo(toWrite);
                    toWrite.WriteByte(255);

                    if (netStream != null)
                    {
                        lock (client)
                        {
                            toWrite.WriteTo(netStream);
                        }
                    }
                    else if (conn.ClientID.SteamID != null)
                    {
                        Steamworks.CSteamID steamID = new Steamworks.CSteamID(conn.ClientID.SteamID.Value);
                        Steamworks.SteamNetworking.SendP2PPacket(steamID, toWrite.GetBuffer(), (uint)toWrite.Length, Steamworks.EP2PSend.k_EP2PSendReliable);
                    }
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            finally { Running = false; }
        }
        public void WriteToStream(byte messageType, NetMessage toWrite)
        {
            if (toWrite == null) { toWrite = new NetMessage(); }
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
            WriteToStream(FrameworkMessages.Termination, null);
        }
    }
}
