using HookHub.Core.ViewModels;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Connections;
using System.Net.Http;

namespace HookHub.Core.Hooks
{
    public class CoreHook
    {
        private readonly ILogger<CoreHook> _logger;
        private HubConnection _connection;
        private HookConnection _hookConnection;
        public ConcurrentDictionary<string, TaskCompletionSource<object>> _connectionResponses;

        public ConcurrentDictionary<string, TaskCompletionSource<object>> ConnectionResponses {
            get {
                if (_connectionResponses is null) { _connectionResponses = new ConcurrentDictionary<string, TaskCompletionSource<object>>(); }
                return (_connectionResponses);
            }
            set {
                _connectionResponses = value;
            }
        }

        public string HookHubNetURL { get { return Config.URL("HookHubNetURL"); } }

        public HubConnection Connection {
            get {
                if (_connection is null)
                {
                    _connection = new HubConnectionBuilder()
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information);
        //logging.AddConsole();
    })
                    .WithUrl($"{HookHubNetURL}?hookName={HookConnection.HookName}", HttpTransportType.WebSockets | HttpTransportType.LongPolling)
                    .WithAutomaticReconnect()
                    .Build();

                }
                return (_connection);
            }
            set { _connection = value; }
        }

        public HookConnection HookConnection {
            get {
                if (_hookConnection is null)
                {
                    _hookConnection = new HookConnection();
                    _hookConnection.HookName = Config.HookNames("HookNameFrom");
                }
                return (_hookConnection);
            }
            set { _hookConnection = value; }
        }

        public CoreHook()
        {
            _logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<CoreHook>();
            Connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await Connection.StartAsync();
            };
        }

        public CoreHook(ILogger<CoreHook> logger)
        {
            _logger = logger;
            Connection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await Connection.StartAsync();
                };
        }

        /*********************************************************/
        /*** CLIENT LISTENERS ************************************/
        /*********************************************************/
        protected virtual void OnClientReceiveMessage(string hookNameFrom, string hookNameTo, string message)
        {
            Console.WriteLine($"{hookNameFrom} -> {hookNameTo} : {message}");
        }

        protected virtual void OnClientReceiveBroadcast(string hookNameFrom, string message)
        {
            Console.WriteLine($"{hookNameFrom} -> HookHubNet : {message}");
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
                    Console.WriteLine($"OnResponse: {netMessage.ConnectionResponseId} -> HookHubNet : {shortRequest}");
                }
                else
                {
                    Console.WriteLine($"OnResponse: {netMessage.ConnectionResponseId} -> HookHubNet : Error: The Hook with ConnectionResponseId:{netMessage.ConnectionResponseId} is not registered");
                }
            }
            catch (Exception ex)
            {
                LogError("Error in OnResponse: " + ex.Message);
            }
        }

        private async void OnRequest(NetMessage netMessage)
        {
            try
            {
                var shortRequest = netMessage.ToShortString(netMessage.Request);
                Console.WriteLine($"{netMessage.HookConnectionFrom.HookName} -> {netMessage.HookConnectionTo.HookName} : [OnRequest]{shortRequest}");

                //                netMessage.Response = "asdfasdfsaf";   // await MensajeDAC.RequestAsync(netMessage);
                netMessage.Response = await HookHubMessage.RequestAsync(netMessage);

                var shortResponse = netMessage.ToShortString(netMessage.Response);

                await Connection.SendAsync("SendResponse", netMessage);

                Console.WriteLine($"{netMessage.HookConnectionFrom.HookName} -> {netMessage.HookConnectionTo.HookName} : [OnRequest]{shortResponse}");
            }
            catch (Exception ex)
            {
                LogError("Error in OnRequest: " + ex.Message);
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
                Console.WriteLine($"The Hook thread stays running while {timeOutMillis} miliseconds");
                TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                await Task.WhenAny(tcs.Task, Task.Delay(timeOutMillis));
            }
            catch (Exception ex)
            {
                LogError("Error in StayRunning: " + ex.Message);
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
                    SendRequestAsync(netMessage);
                    Console.WriteLine($"Waiting for Hook reverse response while {timeOutMillis} miliseconds");
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
                LogError("Error in WaitForReverseResponse: " + ex.Message);
            }
        }

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/


        /*********************************************************/
        /*** CLIENT METHODS **************************************/
        /*********************************************************/
        public async Task Connect()
        {
            Connection.On<string, string, string>("OnClientReceiveMessage", OnClientReceiveMessage);
            Connection.On<string, string>("OnClientReceiveBroadcast", OnClientReceiveBroadcast);
            Connection.On<NetMessage>("OnRequest", OnRequest);
            Connection.On<NetMessage>("OnResponse", OnResponse);

            try
            {
                await Connection.StartAsync();
                Console.WriteLine("Connection started");

                HookConnection.ConnectionID = Connection.ConnectionId;
                string hookName = HookConnection.HookName;
                string connectionID = HookConnection.ConnectionID;
            }
            catch (Exception ex)
            {
                LogError("Error in Connect: " + ex.Message);
            }
        }

        public async Task SendMessage(string hookNameFrom, string hookNameTo, string message)
        {
            try
            {
                Console.WriteLine($"{hookNameFrom} -> {hookNameTo} : {message}");
                await Connection.InvokeAsync("SendMessage", hookNameFrom, hookNameTo, message);
            }
            catch (Exception ex)
            {
                LogError("Error in SendMessage: " + ex.Message);
            }
        }

        public async Task<List<HookConnection>> GetHookConnections(string hookName)
        {
            List<HookConnection> connectionIds = new List<HookConnection>();
            try
            {
                connectionIds = await Connection.InvokeAsync<List<HookConnection>>("GetHookConnections", hookName);
            }
            catch (Exception ex)
            {
                LogError("Error in GetHookConnections: " + ex.Message);
            }
            return (connectionIds);
        }

        public async Task<HookConnection> GetHookConnection(string hookName)
        {
            HookConnection hookConnection = new HookConnection() { HookName = hookName };
            try
            {
                hookConnection = await Connection.InvokeAsync<HookConnection>("GetHookConnection", hookName);
            }
            catch (Exception ex)
            {
                LogError("Error in GetHookConnection: " + ex.Message);
            }
            return (hookConnection);
        }

        public async Task BroadcastMessage(string sender, string message)
        {
            try
            {
                Console.WriteLine($"{sender} -> HookHubNet : {message}");
                await Connection.InvokeAsync("BroadcastMessage",
                    sender, message);
            }
            catch (Exception ex)
            {
                LogError("Error in BroadcastMessage: " + ex.Message);
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
                netMessage.HookConnectionTo = await GetHookConnection(hookNameTo);
                netMessage.RequestType = requestType;
                netMessage.Request = HookHubMessage.Serialize(request);

                // netMessage.Request = MensajeDAC.SerializeRequest(netMessage);

                if (!string.IsNullOrEmpty(netMessage.HookConnectionTo.ConnectionID))
                {
                    Console.WriteLine($"{netMessage.HookConnectionFrom.HookName}:{netMessage.ConnectionResponseId} -> {netMessage.HookConnectionTo.HookName} : [SendRequest]");

                    await WaitForReverseResponse(netMessage, timeOutMillis: 120000);
                    switch (netMessage.RequestType)
                    {
                        case NetType.HttpRequestMessage:
                            netMessage.Response = HookHubMessage.Deserialize<HookWebResponse>(netMessage.Response);
                            break;
                        case NetType.String:
                            netMessage.Response = HookHubMessage.Deserialize<string>(netMessage.Response);
                            break;
                        default:
                            netMessage.Response = HookHubMessage.Deserialize<object>(netMessage.Response);
                            break;
                    }
                }
                else
                {
                    netMessage.Response = $"The Hook {hookNameTo} is not online";
                }
            }
            catch (Exception ex)
            {
                LogError("Error in SendRequest: " + ex.Message);
                netMessage.Response = ex.Message;
            }
            return (netMessage.Response);
        }

        /*********************************************************/
        /*********************************************************/
        /*********************************************************/

        private async void SendRequestAsync(NetMessage netMessage)
        {
            try
            {
                await Connection.InvokeAsync("SendRequest", netMessage);
            }
            catch (Exception ex)
            {
                LogError("Error in SendRequestAsync: " + ex.Message);
            }
        }

        private async Task SendResponseError(NetMessage netMessage)
        {
            try
            {
                await Connection.InvokeAsync("SendResponse", netMessage);
            }
            catch (Exception ex)
            {
                LogError("Error in Hook thread waiting: " + ex.Message);
            }
        }

        private void LogError(string message)
        {
            Console.Error.WriteLine(message);
            _logger.LogError(message);
        }
    }
}
