using HookHub.Core.Hooks;

using Microsoft.AspNetCore.SignalR.Client;

namespace HookHub.Core.Workers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        public CoreHook Hook { get; set; }

        public HubConnectionState HubConnectionState
        {
            get
            {
                HubConnectionState hubConnectionState = Hook == null ? HubConnectionState.Disconnected : Hook.Connection.State;
                return hubConnectionState;
            }
        }

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var loggerCoreHook = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<CoreHook>();
            Hook = new CoreHook(logger: loggerCoreHook, configuration: _configuration);
        }

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

        public async Task Restart(CancellationToken stoppingToken)
        {
            await Stop(stoppingToken);
            await Start();
        }

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
