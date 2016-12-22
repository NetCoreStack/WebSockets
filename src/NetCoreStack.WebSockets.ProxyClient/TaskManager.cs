using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public class TaskManager
    {
        private readonly IWebSocketConnector _connector;
        private readonly ConcurrentQueue<Task> _operations;

        public TaskManager(IWebSocketConnector connector)
        {
            _connector = connector;
            _operations = new ConcurrentQueue<Task>();
            _operations.Enqueue(_connector.ConnectAsync());
        }

        public void Start()
        {
            
        }
    }
}
