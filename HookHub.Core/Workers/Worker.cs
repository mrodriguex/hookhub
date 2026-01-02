using HookHub.Core.Hooks;
using Microsoft.AspNetCore.SignalR.Client;

namespace HookHub.Core.Workers
{
    /// <summary>
    /// Background service that manages the lifecycle and connection state of a hook.
    /// Handles keep-alive messages, reconnection, and hook operations.
    /// </summary>
    public class Worker : BackgroundService
    {
        /// <summary>
        /// Logger instance for logging operations and errors.
        /// </summary>
        private readonly ILogger<Worker> _logger;

        /// <summary>
        /// Configuration instance for accessing app settings.
        /// </summary>
        private readonly IConfiguration _configuration;
        private CoreHook? _hook;

        /// <summary>
        /// The CoreHook instance managed by this worker.
        /// </summary>
        public CoreHook Hook
        {
            get
            {
                _hook ??= new CoreHook(logger: _logger);
                return _hook;
            }
            set
            {
                _hook = value;
            }
        }

        /// <summary>
        /// Constructor. Initializes the worker with logger and configuration, and creates the CoreHook.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="configuration">The configuration instance.</param>
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Executes the background service loop, maintaining the hook connection and sending keep-alive messages.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Start();

                    if (Hook.Connection != null && Hook.Connection.State == HubConnectionState.Connected)
                    {
                        await Hook.Connection.InvokeAsync("BroadcastMessage", Hook.HookConnection.HookName, "Keep alive");
                        Hook.HookConnection.LastKeepAlive = DateTime.UtcNow;
                    }
                    await Hook.StayRunning(timeOutMillis: Hook.HookConnection.TimeIntervals_KeepAlive);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [ExecuteAsync]: Error waiting for Hook theads: " + ex.Message);
                    await Hook.StayRunning(timeOutMillis: 5000);
                }
            }
            await this.StopAsync(stoppingToken);
        }

        /// <summary>
        /// Restarts the hook service.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Restart(CancellationToken stoppingToken)
        {
            await Stop(stoppingToken);
            await Start();
        }

        /// <summary>
        /// Stops the hook service and disposes the connection.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Stop(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [Stop]: Finishing the Hook...");
            await this.StopAsync(stoppingToken);
            if (Hook.Connection != null && !Hook.Connection.State.Equals(HubConnectionState.Disconnected))
            {
                await Hook.Connection.StopAsync(stoppingToken);
            }
            _logger.LogInformation($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [Stop]: The Hook has been terminated");
        }

        /// <summary>
        /// Starts the hook service and establishes connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Start()
        {
            if (!Hook.HookConnection.Equals(HubConnectionState.Connected))
            {
                _logger.LogInformation($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [Start]: Initializing the Hook...");
                Hook.HookConnection.HookHubNetURL = _configuration["URLs:HookHubNetURL"] ?? "";
                Hook.HookConnection.HookName = _configuration["HookNames:HookNameFrom"] ?? "";
                Hook.HookConnection.TimeIntervals_KeepAlive = _configuration.GetValue<int?>("TimeIntervals:KeepAlive") ?? 60000;
                Hook.HookConnection.TimeIntervals_TimeOutResponse = _configuration.GetValue<int?>("TimeIntervals:TimeOutResponse") ?? 10000;
                await Hook.Connect();
                _logger.LogInformation($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [Start]: The Hook has been initialized");
            }
            else
            {
                _logger.LogInformation($"{DateTime.UtcNow:o} | [{Hook.HookConnection.HookName}] -> [HubHookNet] | [Start]: The Hook is already running");
            }
        }
    }
}
