using Microsoft.AspNetCore.Mvc;
using NetCoreStack.WebSockets;
using System.Linq;

namespace WebClientTestApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IConnectionManager _connectionManager;

        public ChatController(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        [HttpPost]
        public IActionResult Broadcast([FromBody]BroadcastMessageContext context)
        {
            var messageContext = new WebSocketMessageContext
            {
                Command = WebSocketCommands.DataSend,
                Value = context
            };

            _connectionManager.BroadcastAsync(messageContext);
            return Json(messageContext);
        }

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
    }
}
