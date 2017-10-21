### Cross-Platform WebSockets Proxy

This project is demonstrating bidirectional connection and data transfer via .NET Core WebSockets.

You can use it on your API - Service side to communicate among your trusted Backend API consumer 
Clients (for example MVC Web Application Hosting) and at the same time may 
client to be a Browser (User-Agent) then you can manage your connection and domain data transfer 
operation with same interface.

[Latest release on Nuget](https://www.nuget.org/packages/NetCoreStack.WebSockets/)


### Usage for API - Service Layer

#### Startup ConfigureServices
```csharp
// Add NetCoreStack Native Socket Services.
services.AddNativeWebSockets<ServerWebSocketCommandInvocator>();
```

#### Startup Configure
```csharp
 app.UseNativeWebSockets();
```

#### Controller with dependency injection
```csharp
public MyController(IConnectionManager connectionManager)
{
    _connectionManager = connectionManager;
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
```

### Clients
#### Startup ConfigureServices
```csharp
// WebSockets for Browsers
services.AddNativeWebSockets(options => {
    options.RegisterInvocator<AgentsWebSocketCommandInvocator>(WebSocketCommands.All);
});

// Client WebSocket - Proxy connections
services.AddProxyWebSockets()
    .Register<CustomWebSocketCommandInvocator>(connectorname, "localhost:7803")
    .Register<AnotherEndpointWebSocketCommandInvocator>(connectorname, "localhost:5000"); // Another endpoint registration, host address must be unique

// Add MVC framework services.
services.AddMvc();
```
#### Startup Configure
```csharp
// Client WebSocket - Proxy connections
app.UseProxyWebSockets();

// WebSockets for Browsers
app.UseNativeWebSockets();

// Use MVC
app.UseMvc();
```

#### Invocator With Dependency Injection on Clients

```csharp
public class CustomWebSocketCommandInvocator : IClientWebSocketCommandInvocator
{
    private readonly IConnectionManager _connectionManager;
    public CustomWebSocketCommandInvocator(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public Task InvokeAsync(WebSocketMessageContext context)
    {
        // Sending incoming data from backend to the clients (Browsers)
        _connectionManager.BroadcastAsync(context);
        return Task.CompletedTask;
    }
}
```

```csharp
public class ClientDiscoveryController : Controller
{
    private readonly IWebSocketConnector _connector;
    public ClientDiscoveryController(IWebSocketConnector<CustomWebSocketCommandInvocator> connector)
    {
        _connector = connector;
    }

    [HttpGet]
    public async Task<IActionResult> KeepAlive()
    {
        await _connector.SendAsync(new WebSocketMessageContext
        {
            Command = WebSocketCommands.DataSend,
            Value = new { Id = 1, Name = "Hello World!", DateTime = DateTime.Now }
        });

        return Ok();
    }
}
```

### Prerequisites
> [ASP.NET Core](https://github.com/aspnet/Home)
