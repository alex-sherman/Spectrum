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
        private MultiplayerService mpService;
        public bool Running { get; private set; }
        public MultiplayerSender(MultiplayerService mpService, Connection conn)
        {
            this.mpService = mpService;
            this.conn = conn;
            timer = new Timer(new TimerCallback(SendKeepAlive), null, 1000, 1000);
            dataQueue = new Queue<QueueItem>();
            this.client = conn.client;
            this.netStream = client.GetStream();
            Running = true;
            new AsyncSendData(SendData).BeginInvoke(null, this);
        }
        private delegate void AsyncSendData();
        private void SendData()
        {
            try
            {
                while (Running)
                {
                    if (!writeSem.WaitOne()) { continue; };
                    QueueItem item;
                    lock (dataQueue)
                    {
                        if (dataQueue.Count == 0) { throw new Exception("Write semaphore was signaled without anything to dequeue"); }
                        item = dataQueue.Dequeue();
                    }
                    lock (client)
                    {
                        netStream.WriteByte((byte)item.EventType);
                        item.toWrite.WriteTo(netStream);
                        netStream.WriteByte(255);
                    }
                }
            }
            catch (IOException) { }
            catch (ObjectDisposedException) { }
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
            List<Guid> peerGuids = mpService.connectedPeers.Keys.ToList();
            NetMessage message = new NetMessage();
            message.Write(peerGuids.Count);
            foreach (Guid guid in peerGuids)
            {
                if (guid != new Guid())
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
