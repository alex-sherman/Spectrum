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
        private volatile List<Guid> replies = new List<Guid>();
        private List<Guid> requiredReplies = new List<Guid>();
        private static List<ReplyWaiter> waiters = new List<ReplyWaiter>();
        
        public ReplyWaiter(params Guid[] requiredGuids)
        {
            requiredReplies = requiredGuids.ToList();
            lock (waiters) { waiters.Add(this); }
        }
        public void AddReply(Guid id)
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
                foreach (Guid peer in requiredReplies)
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
        public static void PeerRemoved(Guid removedPeer)
        {
            lock (waiters)
            {
                foreach (ReplyWaiter waiter in waiters)
                {
                    lock (waiter)
                    {
                        waiter.requiredReplies.Remove(removedPeer);
                        Monitor.Pulse(waiter);
                    }
                }
            }
        }
    }
}
