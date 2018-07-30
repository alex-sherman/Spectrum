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
    class UDPSender
    {
        struct QueueItem
        {
            public byte MessageType;
            public IPEndPoint dest;
            public NetMessage toWrite;
        }
        private Semaphore writeSem = new Semaphore(0, 10);
        private UdpClient client;
        private Queue<QueueItem> dataQueue;
        private NetID mpID;

        public UDPSender(NetID mpID, UdpClient client)
        {
            this.mpID = mpID;
            dataQueue = new Queue<QueueItem>();
            this.client = client;
            new AsyncSendData(SendData).BeginInvoke(null, this);
        }
        private delegate void AsyncSendData();
        private void SendData()
        {
            try
            {
                while (true)
                {
                    if (!writeSem.WaitOne(100)) { continue; };
                    QueueItem item;
                    lock (dataQueue)
                    {
                        if (dataQueue.Count == 0) { throw new Exception("Write semaphore was signaled without anything to dequeue"); }
                        item = dataQueue.Dequeue();
                    }
                    lock (client)
                    {
                        NetMessage data = new NetMessage();
                        data.Write(item.MessageType);
                        data.Write(mpID);
                        data.Write(item.toWrite);
                        byte[] dataArray = data.stream.ToArray();
                        client.Send(dataArray, dataArray.Length, item.dest);
                    }
                }
            }
            catch (Exception e)
            {
                DebugPrinter.Print(e.Message);
            }
        }
        public void SendMessage(IPEndPoint dest, byte messageType, NetMessage message)
        {
            WriteToStream(messageType, dest, message);
        }
        private void WriteToStream(byte EventType, IPEndPoint dest, NetMessage toWrite)
        {
            if (toWrite == null)
            {
                toWrite = new NetMessage();
            }
            QueueItem item = new QueueItem();
            item.MessageType = EventType;
            item.toWrite = toWrite;
            item.dest = dest;
            lock (dataQueue)
            {
                try
                {
                    writeSem.Release();
                    dataQueue.Enqueue(item);
                }
                catch (SemaphoreFullException)
                {
                    DebugPrinter.Print("Write queue is full");
                }
            }
        }
    }
}
