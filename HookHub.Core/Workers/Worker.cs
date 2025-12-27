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

        /// <summary>
        /// The CoreHook instance managed by this worker.
        /// </summary>
        public CoreHook Hook { get; set; }

        /// <summary>
        /// Gets the current connection state of the hub.
        /// </summary>
        public HubConnectionState HubConnectionState
        {
            get
            {
                HubConnectionState hubConnectionState = Hook == null ? HubConnectionState.Disconnected : Hook.Connection.State;
                return hubConnectionState;
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

            var loggerCoreHook = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<CoreHook>();
            Hook = new CoreHook(logger: loggerCoreHook, configuration: _configuration);
        }

        /// <summary>
        /// Executes the background service loop, maintaining the hook connection and sending keep-alive messages.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            _logger.LogInformation("{time} : Initializing the Hook...", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Initializing Hook thread ({1})...");
                    await Hook.Connect();
                    await Hook.Connection.InvokeAsync("BroadcastMessage",
                      Hook.HookConnection.HookName, "Keep alive");
                    await Hook.StayRunning(timeOutMillis: Hook.HookConnection.TimeIntervals_KeepAlive);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error waiting for Hook theads: " + ex.Message);
                    await Hook.StayRunning(timeOutMillis: 30000);
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
            _logger.LogInformation("Finishing the Hook...");
            await this.StopAsync(stoppingToken);
            if (!Hook.Connection.State.Equals(HubConnectionState.Disconnected))
            {
                await Hook.Connection.StopAsync(stoppingToken);
            }
            await Hook.Connection.DisposeAsync();
            _logger.LogInformation("The Hook has been terminated");
        }
            
        /// <summary>
        /// Starts the hook service and establishes connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Start()
        {
            _logger.LogInformation("Initializing the Hook...");
            if (!Hook.Connection.State.Equals(HubConnectionState.Connected))
            {
                await Hook.Connect();
            }
            _logger.LogInformation("The Hook has been initialized");
        }
    }
}
