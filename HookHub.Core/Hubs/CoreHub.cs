using HookHub.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace HookHub.Core.Hubs
{
    /// <summary>
    /// SignalR hub for managing real-time communication between hooks.
    /// Handles message broadcasting, direct messaging, connection management, and hook lifecycle events.
    /// </summary>
    public class CoreHub : Hub
    {
        /// <summary>
        /// Logger instance for logging hub operations and errors.
        /// </summary>
        private ILogger<CoreHub> _logger;

        /// <summary>
        /// Indicates whether the hub has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Constructor. Initializes the hub with a logger.
        /// </summary>
        /// <param name="logger">The logger instance for logging.</param>
        public CoreHub(ILogger<CoreHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="hookNameFrom">The name of the hook sending the message.</param>
        /// <param name="message">The message to broadcast.</param>
        public async Task BroadcastMessage(string hookNameFrom, string message)
        {
            try
            {
                if (!IsDisposed)
                {
                    await Clients.All.SendAsync("OnClientReceiveBroadcast", hookNameFrom, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Sends a direct message to a specific hook.
        /// </summary>
        /// <param name="hookNameFrom">The name of the hook sending the message.</param>
        /// <param name="hookNameTo">The name of the hook receiving the message.</param>
        /// <param name="message">The message to send.</param>
        public async Task SendMessage(string hookNameFrom, string hookNameTo, string message)
        {
            try
            {
                foreach (KeyValuePair<string, string> hookConnection in HookConnetionsHub.HookConnections.Where(x => x.Value.Equals(hookNameTo)))
                {
                    await Clients.Client(hookConnection.Key).SendAsync("OnClientReceiveMessage", hookNameFrom, hookNameTo, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Gets the first hook connection for a given hook name.
        /// </summary>
        /// <param name="hookName">The name of the hook.</param>
        /// <returns>The first HookConnection for the hook, or null if none found.</returns>
        public HookConnection GetHookConnection(string hookName)
        {
            return GetHookConnections(hookName).FirstOrDefault();
        }

        /// <summary>
        /// Gets all hook connections from the registry.
        /// </summary>
        /// <returns>A list of all HookConnection objects.</returns>
        public List<HookConnection> GetAllHookConnections()
        {
            List<HookConnection> hookConnections = new List<HookConnection>();
            try
            {
                hookConnections = HookConnetionsHub.HookConnections.OrderBy(x => Guid.NewGuid()).Select(x => new HookConnection() { HookName = x.Value, ConnectionId = x.Key }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return (hookConnections);
        }

        /// <summary>
        /// Gets all hook connections for a specific hook name.
        /// </summary>
        /// <param name="hookName">The name of the hook.</param>
        /// <returns>A list of HookConnection objects for the hook.</returns>
        public List<HookConnection> GetHookConnections(string hookName)
        {
            List<HookConnection> hookConnections = new List<HookConnection>();
            try
            {
                hookConnections = HookConnetionsHub.HookConnections.OrderBy(x => Guid.NewGuid()).Where(x => x.Value.Equals(hookName)).Select(x => new HookConnection() { HookName = hookName, ConnectionId = x.Key }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return (hookConnections);
        }

        /// <summary>
        /// Sends a response message to the originating hook.
        /// </summary>
        /// <param name="netMessage">The NetMessage containing the response.</param>
        public async Task SendResponse(NetMessage netMessage)
        {
            try
            {
                var connectionIds = GetHookConnections(netMessage.HookConnectionFrom.HookName).Select(x => x.ConnectionId).ToList().AsReadOnly();
                await Clients.Clients(connectionIds).SendAsync("OnResponse", netMessage);
                //await CheckForUserConection(netMessage.HookConnectionFrom.ConnectionId, netMessage.HookConnectionFrom.HookName);
                //await Clients.Client(netMessage.HookConnectionFrom.ConnectionId).SendAsync("OnResponse", netMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Sends a request message to the target hook.
        /// </summary>
        /// <param name="netMessage">The NetMessage containing the request.</param>
        public async Task SendRequest(NetMessage netMessage)
        {
            try
            {
                await CheckForUserConection(netMessage.HookConnectionTo.ConnectionId, netMessage.HookConnectionTo.HookName);
                if (string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionId))
                {
                    netMessage.HookConnectionTo = GetHookConnection(netMessage.HookConnectionTo.HookName);
                }

                if (!string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionId))
                {
                    await Clients.Client(netMessage.HookConnectionTo.ConnectionId).SendAsync("OnRequest", netMessage);
                }
                else
                {
                    netMessage.Response = $"Error: The Hook {netMessage.HookConnectionTo.HookName} is not online.";
                    await SendResponse(netMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// Removes the connection from the registry and broadcasts the disconnection.
        /// </summary>
        /// <param name="exception">The exception that caused the disconnection, if any.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                string hookName = "";
                if (HookConnetionsHub.HookConnections.TryRemove(connectionId, out hookName))
                {
                    Task.Factory.StartNew(async () => await BroadcastMessage("HookHubNet", $"The Hook [{hookName}: {connectionId}] has been disconnected"));
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Called when a client connects to the hub.
        /// Registers the connection in the registry.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string? hookName = httpContext?.Request?.Query["hookName"];
                string connectionId = Context.ConnectionId;
                await CheckForUserConection(connectionId, hookName??"Unknown Hook");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            await base.OnConnectedAsync();
            return;
        }

        /// <summary>
        /// Purges all disconnected hooks from the registry.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PurgeDisconnections()
        {
            try
            {
                foreach (var hookConnection in HookConnetionsHub.HookConnections)
                {
                    await PurgeDisconnection(hookConnection.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return;
        }

        /// <summary>
        /// Purges a specific disconnected hook by connection ID.
        /// </summary>
        /// <param name="connectionId">The connection ID to purge.</param>
        /// <returns>The hook name that was purged, or empty string if not found.</returns>
        public async Task<string> PurgeDisconnection(string connectionId)
        {
            string hookName = "";
            try
            {
                if (HookConnetionsHub.HookConnections.TryRemove(connectionId, out hookName))
                {
                    await BroadcastMessage("HookHubNet", $"The Hook [{hookName}: {connectionId}] has been purged");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return hookName;
        }

        /// <summary>
        /// Disposes the hub and marks it as disposed.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = disposing;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Checks and registers a user connection if not already present.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="hookName">The hook name.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CheckForUserConection(string connectionId, string hookName)
        {
            try
            {
                string userDummy;
                if (!HookConnetionsHub.HookConnections.TryGetValue(connectionId, out userDummy))
                {
                    if (HookConnetionsHub.HookConnections.TryAdd(connectionId, hookName))
                    {
                        await BroadcastMessage("HookHubNet", $"The Hook [{hookName}: {connectionId}] has been connected");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        
    }
}
