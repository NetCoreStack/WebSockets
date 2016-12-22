using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Diagnostics;

namespace NetCoreStack.WebSockets.ProxyClient
{
    public static class WebSocketConnectorExtensions
    {
        private static readonly object _syncLock = new object();
        private static Task _currentTask = null;

        public static void TryConnectAsync(this IWebSocketConnector connector)
        {
            if (connector.WebSocketState != WebSocketState.Open)
            {
                lock (_syncLock)
                {
                    // WebSocket state was not open before we got the lock, check again inside the lock
                    var isLoop = connector.WebSocketState != WebSocketState.Open;
                    if (isLoop)
                    {
                        if (_currentTask == null)
                        {
                            _currentTask = Task.Run(() => connector.ConnectAsync());
                        }
                        else
                        {
                            if (isLoop && _currentTask.IsCompleted)
                            {
#if DEBUG
                                Debug.WriteLine($"==Trying connect: {connector.Options.ConnectorName}==State: {connector.WebSocketState.ToString()}");
#endif
                                // WebSocket state is not open, so try again the connect
                                _currentTask = Task.Run(() => connector.ConnectAsync());
                            }
                        }
                    }
                }
            }
        }
    }
}
