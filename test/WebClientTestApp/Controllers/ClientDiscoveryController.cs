using Microsoft.AspNetCore.Mvc;
using NetCoreStack.WebSockets;
using NetCoreStack.WebSockets.ProxyClient;
using System;
using System.Threading.Tasks;

namespace WebClientTestApp.Controllers
{
    public class ClientDiscoveryController : Controller
    {
        private readonly IWebSocketConnector _connector;
        public ClientDiscoveryController(IWebSocketConnector connector)
        {
            _connector = connector;
        }

        [HttpGet]
        public async Task<IActionResult> KeepAlive()
        {
            await _connector.SendAsync(new WebSocketMessageContext
            {
                Command = WebSocketCommands.DataSend,
                Value = new { Id = 1, Name = "Jhon", DateTime = DateTime.Now }
            });

            return Ok();
        }
    }
}
