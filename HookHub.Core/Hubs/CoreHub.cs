using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HookHub.Core.Hubs
{
    public class CoreHub : Hub
    {
        private ILogger<CoreHub> _logger;
        public bool IsDisposed { get; private set; } = false;

        public CoreHub(ILogger<CoreHub> logger)
        {
            _logger = logger;
        }

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
                LogError(ex.Message);
            }
        }

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
                LogError(ex.Message);
            }
        }

        public HookConnection GetHookConnection(string hookName)
        {
            return GetHookConnections(hookName).FirstOrDefault();
        }


        public List<HookConnection> GetAllHookConnections()
        {
            List<HookConnection> hookConnections = new List<HookConnection>();
            try
            {
                hookConnections = HookConnetionsHub.HookConnections.OrderBy(x => Guid.NewGuid()).Select(x => new HookConnection() { HookName = x.Value, ConnectionID = x.Key }).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return (hookConnections);
        }


        public List<HookConnection> GetHookConnections(string hookName)
        {
            List<HookConnection> hookConnections = new List<HookConnection>();
            try
            {
                hookConnections = HookConnetionsHub.HookConnections.OrderBy(x => Guid.NewGuid()).Where(x => x.Value.Equals(hookName)).Select(x => new HookConnection() { HookName = hookName, ConnectionID = x.Key }).ToList();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return (hookConnections);
        }

        public async Task SendResponse(NetMessage netMessage)
        {
            try
            {
                var connectionIds = GetHookConnections(netMessage.HookConnectionFrom.HookName).Select(x => x.ConnectionID).ToList().AsReadOnly();
                await Clients.Clients(connectionIds).SendAsync("OnResponse", netMessage);
                //await CheckForUserConection(netMessage.HookConnectionFrom.ConnectionID, netMessage.HookConnectionFrom.HookName);
                //await Clients.Client(netMessage.HookConnectionFrom.ConnectionID).SendAsync("OnResponse", netMessage);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public async Task SendRequest(NetMessage netMessage)
        {
            try
            {
                await CheckForUserConection(netMessage.HookConnectionTo.ConnectionID, netMessage.HookConnectionTo.HookName);
                if (string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionID))
                {
                    netMessage.HookConnectionTo = GetHookConnection(netMessage.HookConnectionTo.HookName);
                }

                if (!string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionID))
                {
                    await Clients.Client(netMessage.HookConnectionTo.ConnectionID).SendAsync("OnRequest", netMessage);
                }
                else
                {
                    netMessage.Response = $"Error: The Hook {netMessage.HookConnectionTo.HookName} is not online.";
                    await SendResponse(netMessage);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string connectionID = Context.ConnectionId;
                string hookName = "";
                if (HookConnetionsHub.HookConnections.TryRemove(connectionID, out hookName))
                {
                    Task.Factory.StartNew(async () => await BroadcastMessage("HookHubNet", $"The Hook [{hookName}: {connectionID}] has been disconnected"));
                };
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                string hookName = httpContext.Request.Query["hookName"];
                string connectionId = Context.ConnectionId;
                await CheckForUserConection(connectionId, hookName);
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            await base.OnConnectedAsync();
            return;
        }

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
                LogError(ex.Message);
            }
            return;
        }


        public async Task<string> PurgeDisconnection(string connectionID)
        {
            string hookName = "";
            try
            {
                if (HookConnetionsHub.HookConnections.TryRemove(connectionID, out hookName))
                {
                    await BroadcastMessage("HookHubNet", $"The Hook [{hookName}: {connectionID}] has been purged");
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return hookName;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = disposing;
            base.Dispose(disposing);
        }

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
                LogError(ex.Message);
            }
        }

        private void LogError(string message)
        {
            Console.Error.WriteLine(message);
            _logger.LogError(message);
        }
    }
}
