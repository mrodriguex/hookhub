using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Connections;
using System.Net;
using HookHub.Core.Helpers;
using HookHub.Core.Workers;

namespace HookHub.Core.Hooks
{
    /// <summary>
    /// Client-side SignalR connection wrapper for hooks.
    /// Manages connection to the hub, message handling, and request/response cycles.
    /// </summary>
    public class CoreHook
    {

        #region FIELDS AND CONSTRUCTOR

        /// <summary>
        /// Logger instance for logging operations and errors.
        /// </summary>
        private readonly ILogger<Worker> _logger;

        /// <summary>
        /// The SignalR hub connection.
        /// </summary>
        private HubConnection? _connection;

        /// <summary>
        /// The hook connection metadata.
        /// </summary>
        private HookConnection? _hookConnection;

        /// <summary>
        /// Constructor. Initializes the hook with logger and configuration, and starts connection.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        public CoreHook(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        #region PROPERTIES
        /// <summary>
        /// Dictionary for tracking response completion sources.
        /// </summary>
        public ConcurrentDictionary<string, TaskCompletionSource<HookWebResponse>>? _connectionResponses;

        /// <summary>
        /// Gets or sets the dictionary for tracking response completion sources.
        /// </summary>
        public ConcurrentDictionary<string, TaskCompletionSource<HookWebResponse>> ConnectionResponses
        {
            get
            {
                _connectionResponses ??= new ConcurrentDictionary<string, TaskCompletionSource<HookWebResponse>>();
                return _connectionResponses;
            }
            set
            {
                _connectionResponses = value;
            }
        }

        /// <summary>
        /// Gets or sets the SignalR hub connection.
        /// </summary>
        public HubConnection? Connection
        {
            get
            {
                return _connection;
            }
        }


        /// <summary>
        /// Gets the current connection state of the hub.
        /// </summary>
        public HubConnectionState HubConnectionState
        {
            get
            {
                HubConnectionState hubConnectionState = HubConnectionState.Disconnected;
                if (Connection != null) hubConnectionState = Connection.State;
                return hubConnectionState;
            }
        }

        /// <summary>
        /// Gets or sets the hook connection metadata.
        /// </summary>
        public HookConnection HookConnection
        {
            get
            {
                _hookConnection ??= new HookConnection();
                return _hookConnection;
            }
            set { _hookConnection = value; }
        }
        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        #region CONNECT METHOD

        /// <summary>
        /// Establishes the connection to the hub and sets up event handlers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Connect()
        {
            try
            {
                string hookHubNetURL = $"{HookConnection.HookHubNetURL}?hookName={HookConnection.HookName}";
                _connection ??= new HubConnectionBuilder()
                .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Information))
                    .WithUrl(hookHubNetURL, options =>
                    {
                        options.HttpMessageHandlerFactory = handler =>
                        {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                clientHandler.ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                            }
                            return handler;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<string, string, string>("OnClientReceiveMessage", OnClientReceiveMessage);
                _connection.On<string, string>("OnClientReceiveBroadcast", OnClientReceiveBroadcast);
                _connection.On<HookNetRequest>("OnRequest", OnRequest);
                _connection.On<HookNetRequest>("OnResponse", OnResponse);

                //await StayRunning(timeOutMillis: 3000);
                if (_connection.State.Equals(HubConnectionState.Disconnected))
                {
                    await _connection.StopAsync();
                    await _connection.StartAsync();
                    _logger.LogInformation($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [Connection]: Connected to HookHubNet at {HookConnection.HookHubNetURL}");
                }
                HookConnection.ConnectionId = _connection?.ConnectionId ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [Connection]: " + ex.Message);
            }
        }

        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        #region CLIENT LISTENERS

        /// <summary>
        /// Handles incoming direct messages from other hooks.
        /// </summary>
        /// <param name="hookNameFrom">The sender hook name.</param>
        /// <param name="hookNameTo">The recipient hook name.</param>
        /// <param name="message">The message content.</param>
        protected virtual void OnClientReceiveMessage(string hookNameFrom, string hookNameTo, string message)
        {
            _logger.LogInformation($"{DateTime.UtcNow:o} | [{hookNameFrom}] -> [{hookNameTo}] | [OnClientReceiveMessage] {message}");
        }

        /// <summary>
        /// Handles incoming broadcast messages.
        /// </summary>
        /// <param name="hookNameFrom">The sender hook name.</param>
        /// <param name="message">The broadcast message.</param>
        protected virtual void OnClientReceiveBroadcast(string hookNameFrom, string message)
        {
            _logger.LogInformation($"{DateTime.UtcNow:o} | [HubHookNet] -> [{hookNameFrom}] | [OnClientReceiveBroadcast] {message}");
        }

        /// <summary>
        /// Handles incoming response messages.
        /// </summary>
        /// <param name="netMessage">The response HookNetRequest.</param>
        private void OnResponse(HookNetRequest netMessage)
        {
            try
            {
                TaskCompletionSource<HookWebResponse>? tcs;
                if (!string.IsNullOrEmpty(netMessage.ConnectionResponseId) &&
                    ConnectionResponses.TryGetValue(netMessage.ConnectionResponseId, out tcs))
                {
                    tcs?.TrySetResult(netMessage.Response);
                    var shortRequest = netMessage.ToShortString(netMessage.Response);
                    _logger.LogInformation($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [OnResponse]: {shortRequest}");
                }
                else
                {
                    _logger.LogError($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [OnResponse]: The Hook with Id [{netMessage.ConnectionResponseId}] is not registered in this Hook");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [OnResponse]: " + ex.Message);
            }
        }

        /// <summary>
        /// Handles incoming request messages.
        /// </summary>
        /// <param name="netMessage">The request HookNetRequest.</param>
        private async void OnRequest(HookNetRequest netMessage)
        {
            try
            {
                var shortRequest = netMessage.ToShortString(netMessage.Request);
                _logger.LogInformation($"{DateTime.UtcNow:o} | {netMessage.HookConnectionFrom.HookName} -> {netMessage.HookConnectionTo.HookName} | [OnRequest]: {shortRequest}");

                netMessage.Response = await HookHubMessage.RequestAsync(netMessage);

                var shortResponse = netMessage.ToShortString(netMessage.Response);

                await Connection.SendAsync("SendResponse", netMessage);

                _logger.LogInformation($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [{netMessage.HookConnectionTo.HookName}] | [OnRequest]: {shortResponse}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [{netMessage.HookConnectionTo.HookName}] | [OnRequest]: " + ex.Message);
                netMessage.Response.StatusCode = HttpStatusCode.InternalServerError;
                netMessage.Response.ReasonPhrase = ex.Message;
                await SendResponseError(netMessage);
            }
        }

        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        #region WAITING THREADS

        /// <summary>
        /// Keeps the hook running for a specified timeout period.
        /// </summary>
        /// <param name="timeOutMillis">The timeout in milliseconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task StayRunning(int timeOutMillis)
        {
            try
            {
                _logger.LogInformation($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [StayRunning]: The Hook thread stays running while {timeOutMillis} miliseconds");
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                await Task.WhenAny(tcs.Task, Task.Delay(timeOutMillis));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{HookConnection.HookName}] -> [HubHookNet] | [StayRunning]: " + ex.Message);
            }
            return;
        }

        /// <summary>
        /// Waits for a reverse response to a sent request.
        /// </summary>
        /// <param name="netMessage">The HookNetRequest for the request.</param>
        /// <param name="timeOutMillis">The timeout in milliseconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task WaitForReverseResponse(HookNetRequest netMessage, int timeOutMillis)
        {
            try
            {
                TaskCompletionSource<HookWebResponse> tcs = new TaskCompletionSource<HookWebResponse>();
                if (!string.IsNullOrEmpty(netMessage.ConnectionResponseId) && ConnectionResponses.TryAdd(netMessage.ConnectionResponseId, tcs))
                {
                    await Connection.InvokeAsync("SendRequest", netMessage);
                    _logger.LogInformation($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [HubHookNet] | [WaitForReverseResponse]: Waiting for Hook reverse response while {timeOutMillis} miliseconds");
                    await Task.WhenAny(tcs.Task, Task.Delay(timeOutMillis));
                    if (tcs.Task.IsCompleted)
                    {
                        netMessage.Response = tcs.Task.Result;
                    }
                    else
                    {
                        netMessage.Response.StatusCode = HttpStatusCode.RequestTimeout;
                        netMessage.Response.ReasonPhrase = $"Time out after {timeOutMillis} miliseconds";
                    }
                    TaskCompletionSource<HookWebResponse>? removedTcs;
                    ConnectionResponses.TryRemove(netMessage.ConnectionResponseId, out removedTcs);
                }
            }
            catch (Exception ex)
            {
                netMessage.Response.StatusCode = HttpStatusCode.InternalServerError;
                netMessage.Response.ReasonPhrase = $"{ex.Message}";
                _logger.LogError($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [HubHookNet] | [WaitForReverseResponse]: {ex.Message}");
            }
        }

        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        #region CLIENT COMMUNICATION METHODS

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
                _logger.LogInformation($"{DateTime.UtcNow:o} | [{hookNameFrom}] -> [{hookNameTo}] | [SendMessage]: {message}");
                await Connection.InvokeAsync("SendMessage", hookNameFrom, hookNameTo, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{hookNameFrom}] -> [{hookNameTo}] | [SendMessage]: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to another hook and waits for response.
        /// </summary>
        /// <param name="hookNameTo">The target hook name.</param>
        /// <param name="request">The request object.</param>
        /// <param name="requestType">The type of request.</param>
        /// <returns>The response object.</returns>
        public async Task<HookWebResponse> SendRequest(string hookNameTo, HookWebRequest request)
        {
            HookNetRequest netMessage = new HookNetRequest
            {
                Request = request, // or set to a default value if appropriate
                Response = new HookWebResponse()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = $"Uninitialized response in SendRequest."
                }
            };
            try
            {
                if (!Connection.State.Equals(HubConnectionState.Connected)) { await Connect(); }
                netMessage.ConnectionResponseId = Guid.NewGuid().ToString();
                netMessage.HookConnectionFrom = HookConnection;
                netMessage.HookConnectionTo = await Connection.InvokeAsync<HookConnection>("GetHookConnection", hookNameTo);
                netMessage.Request = request;

                if (!string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionId))
                {
                    _logger.LogInformation($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [{netMessage.HookConnectionTo.HookName}] | [SendRequest]: Sending request to Hook with Id [{netMessage.HookConnectionTo.ConnectionId}]");

                    await WaitForReverseResponse(netMessage, timeOutMillis: HookConnection.TimeIntervals_TimeOutResponse);
                    netMessage.Response = netMessage.Response;
                }
                else
                {
                    netMessage.Response.ReasonPhrase = $"The Hook '{hookNameTo}' is not connected to HookHubNet.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [{netMessage.HookConnectionTo.HookName}] | [SendRequest]: {ex.Message}");
                netMessage.Response.ReasonPhrase = ex.Message;
            }
            return netMessage.Response;
        }

        /// <summary>
        /// Sends an error response back to the hub.
        /// </summary>
        /// <param name="netMessage">The HookNetRequest with error response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SendResponseError(HookNetRequest netMessage)
        {
            try
            {
                await Connection.InvokeAsync("SendResponse", netMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.UtcNow:o} | [{netMessage.HookConnectionFrom.HookName}] -> [HubHookNet] | [SendResponseError]: {ex.Message}");
            }
        }

        #endregion

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

    }
}
