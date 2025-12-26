using HookHub.Core.ViewModels;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Connections;

namespace HookHub.Core.Hooks
{
    public class CoreHook
    {
        private readonly ILogger<CoreHook> _logger;
        private readonly IConfiguration _configuration;
        private HubConnection _connection;
        private HookConnection _hookConnection;
        public ConcurrentDictionary<string, TaskCompletionSource<object>> _connectionResponses;

        public ConcurrentDictionary<string, TaskCompletionSource<object>> ConnectionResponses
        {
            get
            {
                _connectionResponses ??= new ConcurrentDictionary<string, TaskCompletionSource<object>>();
                return (_connectionResponses);
            }
            set
            {
                _connectionResponses = value;
            }
        }

        public string HookHubNetURL { get { return _configuration["URLs:HookHubNetURL"] ?? ""; } }

        public HubConnection Connection
        {
            get
            {
                string hookHubNetURL = $"{HookHubNetURL}?hookName={HookConnection.HookName}";
                _connection ??= new HubConnectionBuilder()
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Information))
                    .WithUrl(hookHubNetURL, HttpTransportType.WebSockets | HttpTransportType.LongPolling)
                    .WithAutomaticReconnect()
                    .Build();
                return (_connection);
            }
            set { _connection = value; }
        }

        public HookConnection HookConnection
        {
            get
            {
                _hookConnection ??= new HookConnection()
                {
                    HookName = _configuration["HookNames:HookNameFrom"] ?? "",
                    TimeIntervals_KeepAlive = _configuration.GetValue<int?>("TimeIntervals:KeepAlive") ?? 60000
                };
                return (_hookConnection);
            }
            set { _hookConnection = value; }
        }

        public CoreHook(ILogger<CoreHook> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            ConnectAsync();
        }

        public async void ConnectAsync()
        {
            await Connect();
        }

        public async Task Connect()
        {
            Connection.On<string, string, string>("OnClientReceiveMessage", OnClientReceiveMessage);
            Connection.On<string, string>("OnClientReceiveBroadcast", OnClientReceiveBroadcast);
            Connection.On<NetMessage>("OnRequest", OnRequest);
            Connection.On<NetMessage>("OnResponse", OnResponse);

            try
            {
                await StayRunning(timeOutMillis: 3000);
                if (Connection.State.Equals(HubConnectionState.Disconnected))
                {
                    await Connection.StopAsync();
                    await Connection.StartAsync();
                    _logger.LogInformation("Connection started");
                }
                HookConnection.ConnectionId = Connection?.ConnectionId ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Connect: " + ex.Message);
            }
        }

        /*********************************************************/
        /*** CLIENT LISTENERS ************************************/
        /*********************************************************/
        protected virtual void OnClientReceiveMessage(string hookNameFrom, string hookNameTo, string message)
        {
            _logger.LogInformation($"{hookNameFrom} -> {hookNameTo} : {message}");
        }

        protected virtual void OnClientReceiveBroadcast(string hookNameFrom, string message)
        {
            _logger.LogInformation($"{hookNameFrom} -> HookHubNet : {message}");
        }

        private void OnResponse(NetMessage netMessage)
        {
            try
            {
                TaskCompletionSource<object> tcs;
                if (ConnectionResponses.TryGetValue(netMessage.ConnectionResponseId, out tcs))
                {
                    tcs?.TrySetResult(netMessage.Response);
                    var shortRequest = netMessage.ToShortString(netMessage.Response);
                    _logger.LogInformation($"OnResponse: {netMessage.ConnectionResponseId} -> HookHubNet : {shortRequest}");
                }
                else
                {
                    _logger.LogInformation($"OnResponse: {netMessage.ConnectionResponseId} -> HookHubNet : Error: The Hook with ConnectionResponseId:{netMessage.ConnectionResponseId} is not registered in this Hook");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in OnResponse: " + ex.Message);
            }
        }

        private async void OnRequest(NetMessage netMessage)
        {
            try
            {
                var shortRequest = netMessage.ToShortString(netMessage.Request);
                _logger.LogInformation($"{netMessage.HookConnectionFrom.HookName} -> {netMessage.HookConnectionTo.HookName} : [OnRequest]{shortRequest}");

                netMessage.Response = await HookHubMessage.RequestAsync(netMessage);

                var shortResponse = netMessage.ToShortString(netMessage.Response);

                await Connection.SendAsync("SendResponse", netMessage);

                _logger.LogInformation($"{netMessage.HookConnectionFrom.HookName} -> {netMessage.HookConnectionTo.HookName} : [OnRequest]{shortResponse}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in OnRequest: " + ex.Message);
                netMessage.Response = ex.Message;
                await SendResponseError(netMessage);
            }
        }
        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        /*********************************************************/
        /*** WAITING THREADS *************************************/
        /*********************************************************/
        public async Task StayRunning(int timeOutMillis)
        {
            try
            {
                _logger.LogInformation($"The Hook thread stays running while {timeOutMillis} miliseconds");
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                await Task.WhenAny(tcs.Task, Task.Delay(timeOutMillis));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in StayRunning: " + ex.Message);
            }
            return;
        }

        private async Task WaitForReverseResponse(NetMessage netMessage, int timeOutMillis)
        {
            try
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                if (ConnectionResponses.TryAdd(netMessage.ConnectionResponseId, tcs))
                {
                    //SendRequestAsync(netMessage);

                    await Connection.InvokeAsync("SendRequest", netMessage);
                    _logger.LogInformation($"Waiting for Hook reverse response while {timeOutMillis} miliseconds");
                    await Task.WhenAny(tcs.Task, Task.Delay(timeOutMillis));
                    if (tcs.Task.IsCompleted)
                    {
                        netMessage.Response = tcs.Task.Result;
                    }
                    else
                    {
                        netMessage.Response = $"Time out after {timeOutMillis} miliseconds";
                    }
                    ConnectionResponses.TryRemove(netMessage.ConnectionResponseId, out tcs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in WaitForReverseResponse: " + ex.Message);
            }
        }

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/


        /*********************************************************/
        /*** CLIENT METHODS **************************************/
        /*********************************************************/

        public async Task SendMessage(string hookNameFrom, string hookNameTo, string message)
        {
            try
            {
                _logger.LogInformation($"{hookNameFrom} -> {hookNameTo} : {message}");
                await Connection.InvokeAsync("SendMessage", hookNameFrom, hookNameTo, message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SendMessage: " + ex.Message);
            }
        }

        public async Task<object> SendRequest(string hookNameTo, object request, NetType requestType = 0)
        {
            NetMessage netMessage = new NetMessage();
            try
            {
                if (!Connection.State.Equals(HubConnectionState.Connected)) { await Connect(); }
                netMessage.ConnectionResponseId = Guid.NewGuid().ToString();
                netMessage.HookConnectionFrom = HookConnection;
                netMessage.HookConnectionTo = await Connection.InvokeAsync<HookConnection>("GetHookConnection", hookNameTo);
                netMessage.RequestType = requestType;
                netMessage.Request = HookHubMessage.Serialize(request);

                if (!string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionId))
                {
                    _logger.LogInformation($"{netMessage.HookConnectionFrom.HookName}:{netMessage.ConnectionResponseId} -> {netMessage.HookConnectionTo.HookName} : [SendRequest]");

                    await WaitForReverseResponse(netMessage, timeOutMillis: 120000);
                    netMessage.Response = HookHubMessage.Deserialize<HookWebResponse>(netMessage.Response);
                }
                else
                {
                    netMessage.Response = $"The Hook {hookNameTo} is not online";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SendRequest: " + ex.Message);
                netMessage.Response = ex.Message;
            }
            return (netMessage.Response);
        }

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        private async Task SendResponseError(NetMessage netMessage)
        {
            try
            {
                await Connection.InvokeAsync("SendResponse", netMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Hook thread waiting: " + ex.Message);
            }
        }
        
    }
}
