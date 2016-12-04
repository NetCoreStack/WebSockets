using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.Internal;
using ServerTestApp.Models;
using System;
using System.Threading.Tasks;

namespace ServerTestApp.Controllers
{
    [Route("api/[controller]")]
    public class DiscoveryController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConnectionManager _connectionManager;

        public DiscoveryController(IConnectionManager connectionManager, 
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
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
            var echo = $"Echo from server '{model.Message}' - {DateTime.Now}";
            var obj = new { message = echo };
            var webSocketContext = new WebSocketMessageContext { Command = WebSocketCommands.DataSend, Value = obj };
            await _connectionManager.BroadcastAsync(webSocketContext);
            return Ok();
        }
    }
}
