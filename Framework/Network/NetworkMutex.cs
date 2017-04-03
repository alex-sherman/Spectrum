using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Spectrum.Framework.Network
{
    public class QEntry : IComparable
    {
        public volatile int time;
        public readonly NetID id;
        public QEntry(int time, NetID id, NetID[] requiredReplies)
        {
            this.time = time;
            this.id = id;
            replyWaiter = new ReplyWaiter(requiredReplies);
        }
        public ReplyWaiter replyWaiter;
        public int CompareTo(object obj)
        {
            if (obj == null) { throw new ArgumentNullException(); }
            if (!(obj is QEntry)) { throw new ArgumentException("Cannot compare to this type"); }
            QEntry other = (QEntry)obj;
            if (time.CompareTo(other.time) != 0)
            {
                return time.CompareTo(other.time);
            }
            return id.CompareTo(other.id);
        }
    }
    delegate void AsyncResponder(QEntry request);

    /// <summary>
    /// Guarantees mutual exclusion between peers
    /// </summary>
    public class NetworkMutex
    {
        private Semaphore localSemaphore = new Semaphore(1, 1);
        private volatile List<QEntry> delayedResponses = new List<QEntry>();
        private object localRequestMutex = new object();
        private QEntry localRequest;
        private volatile int counter = 0;
        private readonly string name;

        private static DefaultDict<string, NetworkMutex> Mutexes = new DefaultDict<string, NetworkMutex>();
        private static MultiplayerService MPService;
        public static void Init(MultiplayerService mpService)
        {
            MPService = mpService;
            MPService.RegisterMessageCallback(FrameworkMessages.RequestMutex, ReceiveRequest);
            MPService.RegisterMessageCallback(FrameworkMessages.ReplyMutex, ReceiveReply);
        }
        public static readonly NetworkMutex TerrainMutex = new NetworkMutex("terrain");
        private NetworkMutex(string name)
        {
            this.name = name;
            lock (Mutexes)
            {
                Mutexes[name] = this;
            }
        }
        public void WaitOne()
        {
            //This allows us to use it as a semaphore locally as well, gaurenteeing only 1 request is sent at a time
            localSemaphore.WaitOne();
            lock (this)
            {
                counter++;
                if (localRequest != null) { throw new Exception("Tried to request the same network mutex twice"); }
                localRequest = new QEntry(counter, MPService.ID, MPService.connectedPeers.Keys.ToArray());
                SendRequest(name, counter);
            }
            localRequest.replyWaiter.WaitReplies();
        }
        public void Release()
        {
            lock (this)
            {
                counter++;
                localRequest = null;
                foreach (QEntry delayedRequest in delayedResponses)
                {
                    SendReply(delayedRequest.id, name, counter);
                }
            }
            localSemaphore.Release();
        }

        private static void SendRequest(string name, int counter)
        {
            NetMessage request = new NetMessage();
            request.Write(name);
            request.Write(counter);
            MPService.SendMessage(FrameworkMessages.RequestMutex, request);
        }
        public static void ReceiveRequest(NetID peerGuid, NetMessage message)
        {
            NetID id = peerGuid;
            string name = message.Read<string>();
            int time = message.Read<int>();
            NetworkMutex mut;
            lock (Mutexes)
            {
                if (Mutexes[name] == null) { Mutexes[name] = new NetworkMutex(name); }
                mut = Mutexes[name];
            }
            mut.ReceiveRequest(new QEntry(time, id, new NetID[] { }));
        }
        private void ReceiveRequest(QEntry request)
        {
            lock (this)
            {
                counter = Math.Max(request.time, counter) + 1;
                //Delay the response if you're requesting the mutex and have higher priority
                if (localRequest == null || localRequest.CompareTo(request) > 0)
                {
                    SendReply(request.id, name, counter);
                    return;
                }
                delayedResponses.Add(request);
            }
        }

        private static void SendReply(NetID peerDestination, string name, int counter)
        {
            NetMessage reply = new NetMessage();
            reply.Write(name);
            reply.Write(counter);
            MPService.SendMessage(FrameworkMessages.ReplyMutex, reply, peerDestination);
        }
        public static void ReceiveReply(NetID peerGuid, NetMessage message)
        {
            NetID id = peerGuid;
            string name = message.Read<string>();
            int time = message.Read<int>();
            QEntry reply = new QEntry(time, id, MPService.connectedPeers.Keys.ToArray());
            NetworkMutex mut;
            lock (Mutexes)
            {
                if (Mutexes[name] == null) { throw new InvalidOperationException("Received a reply from a mutex that we don't have"); }
                mut = Mutexes[name];
            }
            mut.ReceiveReply(reply);
        }
        private void ReceiveReply(QEntry reply)
        {
            lock (this)
            {
                counter = Math.Max(reply.time, counter) + 1;
                if (localRequest == null) { throw new Exception("Got a mutex lock reply but sent no request"); }
                localRequest.replyWaiter.AddReply(reply.id);
            }
        }
    }
}
