using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetCoreStack.WebSockets.Internal
{
    public class TransportLifetimeManager
    {
        private readonly Timer _timer = null;
        private readonly ConcurrentDictionary<string, List<MessageHolder>> _queueDict;
        public TransportLifetimeManager()
        {
            _timer = new Timer(CheckQueue, null, new TimeSpan(0, 0, 5), new TimeSpan(0, 1, 0));
            _queueDict = new ConcurrentDictionary<string, List<MessageHolder>>();
        }

        public void CheckQueue(object state)
        {
            var now = DateTime.Now;
            List<string> enableRemoveConnections = new List<string>();
            foreach (KeyValuePair<string, List<MessageHolder>> entry in _queueDict)
            {
                List<MessageHolder> timeoutItems = entry.Value.Where(p => p.KeepTime < now).ToList();
                entry.Value.RemoveAll(p => p.KeepTime < now);
                if (entry.Value.Count == 0)
                {
                    enableRemoveConnections.Add(entry.Key);
                }
            }

            foreach (var connection in enableRemoveConnections)
            {
                List<MessageHolder> queue = null;
                if (_queueDict.TryRemove(connection, out queue))
                {

                }
            }
        }

        public void AddQueue(string connectionId, MessageHolder holder)
        {
            List<MessageHolder> queue = null;
            if (_queueDict.TryGetValue(connectionId, out queue))
            {
                queue.Add(holder);
            }
            else
            {
                var items = new List<MessageHolder>(new List<MessageHolder> { holder });
                _queueDict.TryAdd(connectionId, items);
            }
        }

        public List<MessageHolder> TryDequeue(string connectionId)
        {
            List<MessageHolder> queue = null;
            if (_queueDict.TryRemove(connectionId, out queue))
            {
                var now = DateTime.Now;
                var items = queue.Where(x => x.KeepTime > now).ToList();
                return items;
            }

            return new List<MessageHolder>();
        }
    }
}