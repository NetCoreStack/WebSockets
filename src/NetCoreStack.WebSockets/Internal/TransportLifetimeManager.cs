using System.Collections.Concurrent;

namespace NetCoreStack.WebSockets.Internal
{
    public class TransportLifetimeManager
    {
        public ConcurrentQueue<MessageHolder> Queue { get; }

        public TransportLifetimeManager()
        {
            Queue = new ConcurrentQueue<MessageHolder>();
        }

        public void AddQueue(MessageHolder holder)
        {
            Queue.Enqueue(holder);
        }
    }
}