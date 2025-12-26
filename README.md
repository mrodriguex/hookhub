# HookHub

A comprehensive .NET-based real-time communication system built on ASP.NET Core and SignalR, enabling bidirectional messaging between multiple connected services (hooks) through a centralized hub.

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
├── HookHub.Hub/                         # Central hub server (ASP.NET Core Web)
│   ├── HookHub.Hub.csproj
│   ├── Program.cs
│   ├── Startup.cs                       # Service configuration and middleware setup
│   ├── Config.cs
│   ├── appsettings.json
│   ├── Controllers/
│   │   ├── HomeController.cs            # Home page controller
│   │   ├── HubController.cs             # Hub management API endpoints
│   │   ├── MensajeController.cs         # Message handling controller
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
├── HookHub.Hook/                        # Example hook client (ASP.NET Core Web)
│   ├── HookHub.Hook.csproj
│   ├── Program.cs
│   ├── Startup.cs                       # Hook service configuration
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Controllers/
│   ├── Views/
│   ├── Properties/
│   │   └── launchSettings.json
│   └── wwwroot/
│
└── HookHub.Web/                         # Alternative web interface
    ├── HookHub.Web.csproj
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

**HookHub.Web**
- Additional web interface with Home and Hook controllers
- Displays Worker.Hook information and connection status
- Provides UI for hook management (Start, Stop, Restart)
