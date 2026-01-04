# HookHub

A distributed real-time communication platform built on ASP.NET Core and SignalR, enabling bidirectional messaging between multiple connected services (hooks) through a centralized hub.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Build Instructions](#build-instructions)
- [Running the Application](#running-the-application)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [License](#license)

## Overview

HookHub is a distributed communication platform that allows multiple services (called "Hooks") to connect to a central Hub and exchange messages in real-time. It leverages ASP.NET Core's SignalR technology for WebSocket and Long Polling support, providing reliable bidirectional communication between services.

**Key Capabilities:**
- Real-time message broadcasting between connected hooks
- Automatic reconnection and keep-alive mechanisms
- Connection monitoring and management
- RESTful API endpoints for hook management
- Web-based monitoring dashboard
- Comprehensive XML documentation for all source code

## Features

### Core Functionality
- **SignalR Hub Integration**: Leverage SignalR's robust WebSocket and Long Polling support for real-time communication
- **Hook Management**: Connect, disconnect, and manage multiple hook services with automatic health checks
- **Keep-Alive Mechanism**: Configurable keep-alive intervals to maintain persistent connections
- **Message Broadcasting**: Send messages from one hook to another or broadcast to all connected hooks
- **Connection Pooling**: Track and manage multiple concurrent connections using ConcurrentDictionary
- **Automatic Reconnection**: Built-in reconnection logic with configurable intervals
- **Comprehensive Logging**: Structured logging with Microsoft.Extensions.Logging integration
- **Error Handling**: Graceful error handling with detailed error messages and logging

### Administrative Features
- **Connection Monitoring Dashboard**: Web UI to monitor all connected hooks and their status
- **Hook Information Panel**: Real-time display of hook connection status, keep-alive timers, and timeouts
- **Connection Purging**: Remove stale or timed-out connections automatically
- **REST API Endpoints**: Query hook connections and perform administrative operations
- **Hook Control Interface**: Start, stop, and restart hooks from the web interface

## Project Structure

```
HookHub/
├── HookHub.sln                          # Visual Studio Solution file
├── Directory.Build.props                # Common build properties
├── Directory.Packages.props             # Centralized NuGet package versioning
├── README.md                            # This file
│
├── HookHub.Core/                        # Shared core library
│   ├── HookHub.Core.csproj
│   ├── appsettings.json
│   ├── Config.cs                        # Configuration utilities
│   ├── Hooks/
│   │   └── CoreHook.cs                  # SignalR client connection wrapper
│   ├── Hubs/
│   │   └── CoreHub.cs                   # SignalR hub server implementation
│   ├── Models/
│   │   ├── HookConnection.cs            # Connection metadata model
│   │   ├── HookConnetionsHub.cs         # Thread-safe connection registry
│   │   ├── HookNetRequest.cs            # Network request model
│   │   ├── HookWebContent.cs            # Web content container
│   │   ├── HookWebRequest.cs            # Web request wrapper
│   │   └── HookWebResponse.cs           # Web response wrapper
│   ├── ContractResolver/
│   │   └── IgnoreErrorPropertiesResolver.cs  # JSON serialization customization
│   ├── Helpers/
│   │   ├── RequestTranscriptHelpers.cs
│   │   └── ResponseTranscriptHelper.cs
│   ├── ViewModels/
│   │   └── HookHubMessage.cs            # Message view model
│   └── Workers/
│       └── Worker.cs                    # Background service for hook connection management
│
|
├── HookHub.Hub/                         # Central hub server (ASP.NET Core Web)
│   ├── HookHub.Hub.csproj
│   ├── Program.cs
│   ├── Startup.cs                       # Service configuration and middleware setup
│   ├── Config.cs
│   ├── appsettings.json
│   ├── Controllers/
│   │   ├── HomeController.cs            # Home page controller
│   │   ├── HubController.cs             # Hub management API endpoints
│   │   └── ProxyController.cs           # Proxy request routing
│   ├── Models/
│   │   ├── CoreHookInfoModel.cs         # Hub-specific hook information model
│   │   └── ErrorViewModel.cs
│   ├── Views/
│   │   ├── _ViewImports.cshtml
│   │   ├── _ViewStart.cshtml
│   │   ├── Home/
│   │   ├── Hub/
│   │   └── Shared/
│   ├── Properties/
│   │   └── launchSettings.json
│   └── wwwroot/
│       ├── index.html                   # Real-time communication test page
│       ├── indexTesting.html            # Multi-instance monitoring page
│       ├── css/                         # Stylesheets
│       ├── js/
│       │   ├── hub.js                   # Hub client-side communication logic
│       │   ├── signalr.min.js           # SignalR client library
│       │   └── jquery-3.3.1.min.js      # jQuery library
│       ├── lib/bootstrap/               # Bootstrap framework
│       └── images/                      # Static images
│
│
└── HookHub.Hook/                         # Alternative web interface
    ├── HookHub.Hook.csproj
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Controllers/
    │   ├── HomeController.cs            # Worker hook information display
    │   └── HookController.cs            # Hook control API
    ├── Views/
    │   └── Home/
    │       └── Index.cshtml             # Worker hook information panel
    ├── Services/
    └── wwwroot/
```

### Key Architecture Components

**HookHub.Core**
- Shared library containing the SignalR hub implementation, hook client wrapper, and common models
- Targets Net10.0 as a library
- Contains the `Worker` background service that manages hook connections

**HookHub.Hub**
- Central SignalR hub server running on port 5100
- Manages all hook connections and message routing
- Provides REST API for administrative operations
- Serves the monitoring dashboard

**HookHub.Hook**
- Example hook client service demonstrating how to connect to the hub
- Can be replicated for multiple hook instances
- Configured to connect to the central hub

**HookHub.Hook**
- Additional web interface with Home and Hook controllers
- Displays Worker.Hook information and connection status
- Provides UI for hook management (Start, Stop, Restart)

## Prerequisites

- **.NET SDK 10.0** or later
- **Visual Studio 2022** (recommended) or Visual Studio Code with C# extension
- **Windows, macOS, or Linux** operating system
- **Port 5100**: For HookHub.Hub (configurable)
- **Port 5101+**: For HookHub.Hook and other services (configurable)

## Installation

### 1. Clone the Repository

```bash
git clone https://gitlab.com/mrodriguex/hookhub.git
cd hookhub
```

### 2. Restore NuGet Packages

```bash
dotnet restore HookHub.sln
```

## Configuration

Configuration is managed through `appsettings.json` files in each project.

### HookHub.Core Configuration

**File:** `HookHub.Core/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "URLs": {
    "HookHubNetURL": "http://localhost:5100/HOOKHUBNET"
  },
  "TimeIntervals": {
    "KeepAlive": "60000"
  },
  "HookNames": {
    "HookNameFrom": "HookNameFrom-Hook",
    "HookNameTo": "HookHubNet"
  }
}
```

### HookHub.Hub Configuration

**File:** `HookHub.Hub/appsettings.json`

Same structure as above with `HookNameFrom` set to `"HookNameFrom-Hub"`.

### HookHub.Hook Configuration

**File:** `HookHub.Hook/appsettings.json`

Example hook client configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "URLs": {
    "HookHubNetURL": "http://localhost:5100/HOOKHUBNET"
  },
  "TimeIntervals": {
    "KeepAlive": "60000"
  },
  "HookNames": {
    "HookNameFrom": "HookClientName",
    "HookNameTo": "HookHubNet"
  }
}
```

### HookHub.Hook Configuration

**File:** `HookHub.Hook/appsettings.json`

Web interface configuration for displaying hook information:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Key Configuration Options

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `HookHubNetURL` | String | `http://localhost:5100/HOOKHUBNET` | SignalR hub endpoint URL |
| `KeepAlive` | Integer (ms) | `60000` | Keep-alive message interval (minimum 60 seconds) |
| `HookNameFrom` | String | Service-dependent | Identifier for the hook service |
| `HookNameTo` | String | `HookHubNet` | Default target for messages |

## Build Instructions

### Build All Projects

```bash
dotnet build HookHub.sln /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
```

### Build Individual Projects

```bash
# Build Core Library
dotnet build HookHub.Core/HookHub.Core.csproj

# Build Hub Server
dotnet build HookHub.Hub/HookHub.Hub.csproj

# Build Hook Client
dotnet build HookHub.Hook/HookHub.Hook.csproj
```

### Build in Release Mode

```bash
dotnet build HookHub.sln -c Release
```

## Running the Application

### Option 1: Run from Command Line

```bash
# Terminal 1: Start the Hub Server
cd HookHub.Hub
dotnet run

# Terminal 2: Start the Hook Client
cd HookHub.Hook
dotnet run
```

### Option 2: Run in Visual Studio

1. Set **HookHub.Hub** as the startup project
2. Press `F5` or click **Start Debugging**
3. Open another terminal and run HookHub.Hook

### Default URLs

| Service | URL | Port |
|---------|-----|------|
| HookHub.Hub | `http://localhost:5100` | 5100 |
| HookHub.Hook | `http://localhost:5200` | 5200 |
| SignalR Hub | `http://localhost:5100/HOOKHUBNET` | 5100 |

## Usage

### Web Dashboard

1. Navigate to `http://localhost:5100/hub/index` in your browser
2. View connected hooks and their status
3. Use the Hub Controller interface to monitor connections

### Testing Real-Time Communication

1. Open `http://localhost:5100/hub/index` in your browser
2. Enter hook names and messages
3. Send messages between connected hooks in real-time

### Monitoring Multiple Instances

1. Navigate to `http://localhost:5100/indexTesting.html`
2. View 4 instances of the communication test page simultaneously

### Hook Control Interface

1. Navigate to `http://localhost:5200/home/index` (HookHub.Hook)
2. Visit the **Home** page to see Worker.Hook information
3. Use the action buttons to:
   - **Start Hook**: Initiate hook connection
   - **Stop Hook**: Terminate hook connection
   - **Restart Hook**: Restart the hook service

### REST API Endpoints

#### HubController (HookHub.Hub)

```
GET /Hub/Index
```

#### ProxyController (HookHub.Hub)

The ProxyController acts as a reverse proxy, routing HTTP requests from clients to connected hook services through the hub. This enables:

- **Request Forwarding**: Forward HTTP requests (GET, POST, PUT, DELETE) to target hooks
- **Header Preservation**: Maintains original request headers, cookies, and body content
- **Response Handling**: Returns hook responses with appropriate HTTP status codes
- **Content-Type Support**: Handles various content types including JSON, form data, and binary content

**Supported HTTP Methods:**
- GET, POST, PUT, DELETE

**Endpoint Pattern:**
```
{HTTP_METHOD} /Proxy/{HookNameTo}/{TargetUrl}
```

**Parameters:**
- `{HookNameTo}`: The destination hook name (e.g., `HookClientName`)
- `{TargetUrl}`: The full URL to proxy to the hook service

**Examples:**

```bash
# Get hook status
GET http://localhost:5100/Proxy/HookClientName/http://localhost:5200/hook/index

# Post data to hook
POST http://localhost:5100/Proxy/HookClientName/http://localhost:5200/api/data
Content-Type: application/json
{
  "key": "value"
}

# Forward with query parameters
GET http://localhost:5100/Proxy/HookClientName/http://localhost:5200/api/search?q=test&page=1
```

**Response Handling:**
- **200 OK**: Successful response from hook
- **400 Bad Request**: Invalid request or hook error
- **404 Not Found**: Hook not found or target URL not available
- **500 Internal Server Error**: Hook service error

**Features:**
- Automatic URL correction for protocol prefixes
- Form data and file upload support
- Cookie forwarding
- Comprehensive error logging

#### HttpRequestMessageExtensions

Utility class providing extension methods for HttpRequestMessage:

```csharp
/// <summary>
/// Extension methods for HttpRequestMessage to support cloning.
/// </summary>
public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Clones an HttpRequestMessage asynchronously, including headers, content, and properties.
    /// </summary>
    public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    // Implementation...
}
```

#### HttpResponseMessageResult

Custom IActionResult for returning HttpResponseMessage in MVC actions:

```csharp
/// <summary>
/// ActionResult implementation that returns an HttpResponseMessage.
/// Allows MVC actions to return HttpResponseMessage directly.
/// </summary>
public class HttpResponseMessageResult : IActionResult
{
    /// <summary>
    /// Executes the result asynchronously, copying the response to the HTTP context.
    /// </summary>
    public async Task ExecuteResultAsync(ActionContext context)
    // Implementation...
}
```
  - Get current hub information

GET /Hub/GetAllHookConnections
  - Get list of all connected hooks

GET /Hub/PurgeDisconnections
  - Remove all timed-out connections

GET /Hub/PurgeDisconnection/{connectionId}
  - Remove specific connection by ID
```

#### HookController (HookHub.Hook)

```
GET /Hook/Index
  - Get current hook information (JSON)

GET /Hook/Start
  - Start the hook service

GET /Hook/Stop
  - Stop the hook service

GET /Hook/Restart
  - Restart the hook service
```

### SignalR Hub Methods

#### Available Methods on CoreHub

```csharp
// Broadcast to all connected hooks
BroadcastMessage(string hookNameFrom, string message)

// Send to specific hook
SendMessage(string hookNameFrom, string hookNameTo, string message)

// Get information about a hook
GetHookConnection(string hookName)

// Get all connections
GetAllHookConnections()

// Get connections by name
GetHookConnections(string hookName)

// Send request/response messages
SendRequest(HookNetRequest netMessage)
SendResponse(HookNetRequest netMessage)
```

## API Documentation

### Connection Flow

1. **Hook Initialization**: Hook service starts and initializes the `Worker` background service
2. **Connection**: Worker establishes SignalR connection to the hub using configured `HookHubNetURL`
3. **Registration**: Hub registers the hook and stores connection metadata in `HookConnetionsHub`
4. **Keep-Alive**: Worker sends periodic keep-alive messages at configured intervals
5. **Messaging**: Messages routed through the hub to target hooks
6. **Disconnection**: On shutdown, connection is properly closed and unregistered

### Data Models

#### HookConnection

Represents metadata for a connected hook:

```csharp
public class HookConnection
{
    public string HookName { get; set; }           // Unique identifier
    public string ConnectionId { get; set; }       // SignalR connection ID
    public DateTime LastKeepAlive { get; set; }    // Last keep-alive timestamp
    public int TimeIntervals_KeepAlive { get; set; } // Keep-alive interval (ms)
    public bool IsTimedOut { get; set; }           // Connection timeout status
}
```

#### CoreHook

Client-side SignalR connection wrapper with automatic reconnection and message handling.

#### Worker

Background service that manages hook lifecycle and connection state.

## Code Documentation

All source code files in the HookHub.Hook and HookHub.Hub projects have been thoroughly documented with XML comments following standard C# documentation conventions. This includes:

### Documented Components

- **Classes**: Purpose, responsibilities, and key behaviors
- **Methods**: Functionality, parameters, return values, and exceptions
- **Properties**: Purpose and data types
- **Constructors**: Initialization parameters and dependencies
- **Extension Methods**: Usage and behavior

### Documentation Standards

- **XML Comments**: All public members include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags where applicable
- **Class-Level Comments**: Describe the overall purpose and responsibilities
- **Method-Level Comments**: Explain what the method does, its parameters, and return behavior
- **Inline Comments**: Used for complex logic or non-obvious code sections

### Benefits

- **IntelliSense Support**: IDEs can display documentation when hovering over methods/properties
- **API Documentation Generation**: Tools like DocFX can generate comprehensive API docs
- **Code Maintainability**: Clear documentation helps developers understand and maintain the codebase
- **Onboarding**: New developers can quickly understand the system architecture and components

### Example Documentation

```csharp
/// <summary>
/// API controller for managing the hub service and hook connections.
/// Provides endpoints to view hub status, manage hook connections, and purge disconnections.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HubController : Controller
{
    /// <summary>
    /// Gets all current hook connections.
    /// </summary>
    /// <returns>JSON array of all hook connections.</returns>
    [HttpGet("{Action}")]
    public IActionResult GetAllHookConnections()
    {
        return Json(CoreHub.GetAllHookConnections());
    }
}
```

## NuGet Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.SignalR.Client | 8.0.5 | SignalR client library |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 8.0.5 | JSON serialization |
| Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation | 8.0.5 | Debug-time Razor compilation |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.5 | SQL Server database support |
| Microsoft.Data.SqlClient | 5.2.1 | SQL Server connectivity |
| Microsoft.Extensions.Hosting | 9.0.0 | Hosted services |
| Newtonsoft.Json | 13.0.3 | JSON utilities |
| FastMember.NetCore | 1.1.0 | Performance reflection utilities |
| ncrontab | 3.3.3 | Cron expression support |

## Troubleshooting

### Connection Issues

**Problem**: Hook cannot connect to hub
- **Solution**: Verify `HookHubNetURL` in appsettings.json
- **Check**: Ensure HookHub.Hub is running on the configured port
- **Check**: Verify firewall allows communication on the port

### Keep-Alive Timeout

**Problem**: Connection marked as timed out
- **Solution**: Increase `KeepAlive` interval in configuration
- **Check**: Ensure network connectivity is stable
- **Check**: Review logs for network errors

### Port Already in Use

**Problem**: Cannot start service on configured port
- **Solution**: Change port in `launchSettings.json` or via environment variables
- **Alternative**: Stop other services using the port

## Development

### Project Configuration

All projects use:
- **Target Framework**: .NET 10.0
- **Language Version**: Latest C# with implicit usings enabled
- **Nullable Reference Types**: Enabled
- **Documentation**: Comprehensive XML comments on all public members

### Code Documentation Standards

The codebase follows strict documentation standards:
- All public classes, methods, and properties include XML documentation comments
- IntelliSense-friendly descriptions with `<summary>`, `<param>`, and `<returns>` tags
- Clear explanations of complex logic and business rules
- Consistent formatting and terminology throughout the codebase

### Building with Visual Studio

1. Open `HookHub.sln`
2. Select desired configuration (Debug/Release)
3. Build → Build Solution (Ctrl+Shift+B)

### Testing Changes

1. Modify source files
2. Rebuild the affected project
3. Run tests using `dotnet test`
4. Test in browser at appropriate URLs

## License

This project is provided as-is. Ensure compliance with all included third-party library licenses:

- **Bootstrap**: MIT License
- **SignalR**: Apache 2.0 License
- **Microsoft.AspNetCore**: Apache 2.0 License
- **Newtonsoft.Json**: MIT License

## Support

For issues, questions, or contributions, please refer to the project repository:
https://gitlab.com/mrodriguex/hookhub


## Project Page

You can find more information and documentation on the [HookHub Project Page](https://home.hookhub.app/).


---

**Last Updated**: December 27, 2025
**Framework**: .NET 10.0
**License**: See individual component licenses
