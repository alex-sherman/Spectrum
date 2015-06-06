using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Spectrum.Framework.Network
{
    /// <summary>
    /// Allows you to block and wait for all current peers to respond to your request
    /// </summary>
    public class ReplyWaiter
    {
        private bool noUpdateOnRemove;
        private volatile List<NetID> replies = new List<NetID>();
        private List<NetID> requiredReplies = new List<NetID>();
        private static List<ReplyWaiter> waiters = new List<ReplyWaiter>();

        public ReplyWaiter(bool noUpdateOnRemove, params NetID[] requiredGuids)
        {
            this.noUpdateOnRemove = noUpdateOnRemove;
            requiredReplies = requiredGuids.ToList();
            lock (waiters) { waiters.Add(this); }
        }

        public ReplyWaiter(params NetID[] requiredGuids)
            : this(false, requiredGuids) { }

        public void AddReply(NetID id)
        {
            lock (this)
            {
                if (!replies.Contains(id))
                {
                    replies.Add(id);
                    Monitor.Pulse(this);
                }
            }
        }
        private bool allReplied
        {
            get
            {
                foreach (NetID peer in requiredReplies)
                {
                    if (!replies.Contains(peer))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public void WaitReplies()
        {
            lock (this)
            {
                while (!allReplied)
                {
                    Monitor.Wait(this);
                }
            }
            lock (waiters) { waiters.Remove(this); }
        }
        public static void PeerRemoved(NetID removedPeer)
        {
            lock (waiters)
            {
                foreach (ReplyWaiter waiter in waiters)
                {
                    lock (waiter)
                    {
                        if (waiter.noUpdateOnRemove)
                            continue;
                        waiter.requiredReplies.Remove(removedPeer);
                        Monitor.Pulse(waiter);
                    }
                }
            }
        }
    }
}
