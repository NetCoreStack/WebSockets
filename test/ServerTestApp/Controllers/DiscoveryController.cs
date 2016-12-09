using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetCoreStack.WebSockets;
using ServerTestApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerTestApp.Controllers
{
    [Route("api/[controller]")]
    public class DiscoveryController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConnectionManager _connectionManager;
        private readonly IDistributedCache _distrubutedCache;
        private readonly IMemoryCache _memoryCache;

        public DiscoveryController(IConnectionManager connectionManager, 
            IDistributedCache distrubutedCache,
            IMemoryCache memoryCache,
            ILoggerFactory loggerFactory)
        {
            _connectionManager = connectionManager;
            _distrubutedCache = distrubutedCache;
            _memoryCache = memoryCache;
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
            if (bytes != null)
            {
                await _connectionManager.BroadcastBinaryAsync(bytes, new SocketObject { Key = model.Key });
            }
            return Ok();
        }

        [HttpPost(nameof(SendBinaryAsync))]
        public async Task<IActionResult> SendBinaryAsync([FromBody]Context model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Keys == null || !model.Keys.Any())
            {
                return NotFound();
            }

            foreach (var key in model.Keys)
            {
                try
                {
                    var bytes = _distrubutedCache.Get(key);
                    await _connectionManager.BroadcastBinaryAsync(bytes, new SocketObject { Key = key });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            
            return Ok();
        }
    }
}
