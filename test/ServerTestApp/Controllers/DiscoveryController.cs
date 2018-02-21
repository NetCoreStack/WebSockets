using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.Internal;
using ServerTestApp.Models;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTestApp.Controllers
{
    [Route("api/[controller]")]
    public class DiscoveryController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConnectionManager _connectionManager;
        private readonly IDistributedCache _distrubutedCache;

        public DiscoveryController(IConnectionManager connectionManager, 
            IDistributedCache distrubutedCache,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _distrubutedCache = distrubutedCache;
            _loggerFactory = loggerFactory;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new { processorCount = Environment.ProcessorCount });
        }

        [HttpPost(nameof(SendAsync))]
        public async Task<IActionResult> SendAsync([FromBody]SimpleModel model)
        {
            var echo = $"Echo from server '{model.Key}' - {DateTime.Now}";
            var obj = new { message = echo };
            var webSocketContext = new WebSocketMessageContext { Command = WebSocketCommands.DataSend, Value = obj };
            await _connectionManager.BroadcastAsync(webSocketContext);
            return Ok();
        }

        [HttpPost(nameof(BroadcastBinaryAsync))]
        public async Task<IActionResult> BroadcastBinaryAsync([FromBody]SimpleModel model)
        {
            var bytes = _distrubutedCache.Get(model.Key);
            var routeValueDictionary = new RouteValueDictionary(new { Key = model.Key });
            if (bytes != null)
            {
                await _connectionManager.BroadcastBinaryAsync(bytes, routeValueDictionary);
            }
            return Ok();
        }

        [HttpPost(nameof(SendBinaryAsync))]
        public async Task<IActionResult> SendBinaryAsync()
        {
            var routeValueDictionary = new RouteValueDictionary(new { Key = "SomeKey" });
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            await _connectionManager.BroadcastBinaryAsync(bytes, routeValueDictionary);
            return Ok();
        }

        [HttpGet(nameof(GetConnections))]
        public IActionResult GetConnections()
        {
            var connections = _connectionManager.Connections
               .Select(x => new
               {
                   id = x.Value.ConnectionId,
                   name = x.Value.ConnectorName,
                   state = x.Value.WebSocket?.State.ToString()
               }).OrderBy(o => o.name).ToList();

            return Json(connections);
        }

        [HttpGet(nameof(CloseConnection))]
        public async Task<IActionResult> CloseConnection([FromQuery]string connectionId)
        {
            if(_connectionManager.Connections.TryGetValue(connectionId, out WebSocketTransport webSocketTransport))
            {
                await webSocketTransport.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server close", CancellationToken.None);
                return Json(new { status = 1 });
            }

            return Json(new { status = 0 });
        }
    }
}
