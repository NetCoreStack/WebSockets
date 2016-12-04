### Cross-Platform WebSockets From Browsering (DMZ) Zone to API - Service (Backend) Zone

This project is demonstrate bidirectional connections and data transfers from low level layer (secure zone) to 
Browsering - DMZ (Public) zone via .NET Core WebSockets.

You can use it on your API - Service side to communicate each other your trusted Backend API consumer 
Clients (for example MVC Web Application Hosting) and at the same time may 
client can be a Browser (User-Agent) then you can manage your connections and domain data transfer 
operations with same interfaces.

![](https://ci.appveyor.com/api/projects/status/h8hcoiol8as0ok24?svg=true)

### Usage for API - Service Layer

#### Startup ConfigureServices
```csharp
// Add Net Core Stack socket services.
services.AddNativeWebSockets();
```

#### Startup Configure
```csharp
 app.UseNativeWebSockets();
```

#### Controller with Dependency Injection
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

### Usage for Trusted Clients or Browsering (DMZ) Layer
#### Startup ConfigureServices
```csharp
// Client WebSocket - DMZ to API side connections
services.AddProxyWebSockets(options => {
    options.WebSocketHostAddress = "localhost:7803";
    options.RegisterInvocator<CustomWebSocketCommandInvocator>(WebSocketCommands.All);
});

// WebSockets for Browsers - User Agent ( javascript clients )
services.AddNativeWebSockets();

// Add MVC framework services.
services.AddMvc();
```
#### Startup Configure
```csharp
// Proxy (Domain App) Client WebSocket - DMZ to API side connections
app.UseProxyWebSockets();
// User Agent WebSockets for Browsers
app.UseNativeWebSockets();
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
        // Sending incoming data from Backend zone to the Clients (Browsers)
        _connectionManager.BroadcastAsync(context);
        return Task.CompletedTask;
    }
}
```

### Prerequisites
> [ASP.NET Core](https://github.com/aspnet/Home)

### Installation

> dotnet restore
  
