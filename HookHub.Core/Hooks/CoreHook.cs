using HookHub.Core.ViewModels;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Connections;

namespace HookHub.Core.Hooks
{
    /// <summary>
    /// Client-side SignalR connection wrapper for hooks.
    /// Manages connection to the hub, message handling, and request/response cycles.
    /// </summary>
    public class CoreHook
    {
        /// <summary>
        /// Logger instance for logging operations and errors.
        /// </summary>
        private readonly ILogger<CoreHook> _logger;

        /// <summary>
        /// Configuration instance for accessing app settings.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The SignalR hub connection.
        /// </summary>
        private HubConnection _connection;

        /// <summary>
        /// The hook connection metadata.
        /// </summary>
        private HookConnection _hookConnection;

        /// <summary>
        /// Dictionary for tracking response completion sources.
        /// </summary>
        public ConcurrentDictionary<string, TaskCompletionSource<object>> _connectionResponses;

        /// <summary>
        /// Gets or sets the dictionary for tracking response completion sources.
        /// </summary>
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

        /// <summary>
        /// Gets the hub URL from configuration.
        /// </summary>
        public string HookHubNetURL { get { return _configuration["URLs:HookHubNetURL"] ?? ""; } }

        /// <summary>
        /// Gets or sets the SignalR hub connection.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the hook connection metadata.
        /// </summary>
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

        /// <summary>
        /// Constructor. Initializes the hook with logger and configuration, and starts connection.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        public CoreHook(ILogger<CoreHook> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            ConnectAsync();
        }

        /// <summary>
        /// Initiates the connection asynchronously.
        /// </summary>
        public async void ConnectAsync()
        {
            await Connect();
        }

        /// <summary>
        /// Establishes the connection to the hub and sets up event handlers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Handles incoming direct messages from other hooks.
        /// </summary>
        /// <param name="hookNameFrom">The sender hook name.</param>
        /// <param name="hookNameTo">The recipient hook name.</param>
        /// <param name="message">The message content.</param>
        protected virtual void OnClientReceiveMessage(string hookNameFrom, string hookNameTo, string message)
        {
            _logger.LogInformation($"{hookNameFrom} -> {hookNameTo} : {message}");
        }

        /// <summary>
        /// Handles incoming broadcast messages.
        /// </summary>
        /// <param name="hookNameFrom">The sender hook name.</param>
        /// <param name="message">The broadcast message.</param>
        protected virtual void OnClientReceiveBroadcast(string hookNameFrom, string message)
        {
            _logger.LogInformation($"{hookNameFrom} -> HookHubNet : {message}");
        }

        /// <summary>
        /// Handles incoming response messages.
        /// </summary>
        /// <param name="netMessage">The response NetMessage.</param>
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

        /// <summary>
        /// Handles incoming request messages.
        /// </summary>
        /// <param name="netMessage">The request NetMessage.</param>
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

        /// <summary>
        /// Keeps the hook running for a specified timeout period.
        /// </summary>
        /// <param name="timeOutMillis">The timeout in milliseconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Waits for a reverse response to a sent request.
        /// </summary>
        /// <param name="netMessage">The NetMessage for the request.</param>
        /// <param name="timeOutMillis">The timeout in milliseconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Sends a direct message to another hook.
        /// </summary>
        /// <param name="hookNameFrom">The sender hook name.</param>
        /// <param name="hookNameTo">The recipient hook name.</param>
        /// <param name="message">The message content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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

        /// <summary>
        /// Sends a request to another hook and waits for response.
        /// </summary>
        /// <param name="hookNameTo">The target hook name.</param>
        /// <param name="request">The request object.</param>
        /// <param name="requestType">The type of request.</param>
        /// <returns>The response object.</returns>
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

        /// <summary>
        /// Sends an error response back to the hub.
        /// </summary>
        /// <param name="netMessage">The NetMessage with error response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
