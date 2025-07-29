using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using HookHub.Core.Hooks;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HookHub.Core.Workers
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private CoreHook Hook { get; set; }

        public HubConnectionState HubConnectionState {
            get {
                HubConnectionState hubConnectionState = HubConnectionState.Disconnected;
                if (!(Hook is null))
                {
                    hubConnectionState = Hook.Connection.State;
                }
                return hubConnectionState;
            }
        }

        public HookConnection HookConnection {
            get {
                HookConnection hookConnection = new HookConnection() ;
                if (!(Hook is null))
                {
                    hookConnection = Hook.HookConnection;
                }
                return hookConnection;
            }
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            ConnectClientAsync();
        }

        private async void ConnectClientAsync()
        {
            Hook = new CoreHook();
            await Hook.Connect();
        }

        private async Task ConnectClient()
        {
            Hook = new CoreHook();
            await Hook.Connect();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Initializing the Hook...");
            _logger.LogInformation("{time} : Initializing the Hook...", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Hook.BroadcastMessage($"{Hook.HookConnection.HookName}", "Keep alive");
                    _logger.LogInformation($"Initializing Hook thread ({1})...");
                    await Hook.StayRunning(timeOutMillis: 30000);
                    if (Hook.Connection.State.Equals(HubConnectionState.Disconnected))
                    {
                        await Hook.Connection.StopAsync();
                        await Hook.Connection.StartAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error waiting for Hook theads: " + ex.Message);
                    _logger.LogError("Error waiting for Hook theads: " + ex.Message);
                }
            }
            await this.StopAsync(stoppingToken);
            Console.WriteLine("The Hook has been terminated");

        }

        public async Task Restart(CancellationToken stoppingToken)
        {
            await Stop(stoppingToken);
            await Start();
        }

        public async Task Stop(CancellationToken stoppingToken)
        {
            Console.WriteLine("Finishing the Hook...");
            await this.StopAsync(stoppingToken);
            if (!Hook.Connection.State.Equals(HubConnectionState.Disconnected))
            {
                await Hook.Connection.StopAsync(stoppingToken);
            }
            await Hook.Connection.DisposeAsync();
            Console.WriteLine("The Hook has been terminated");
        }

        public async Task Start()
        {
            Console.WriteLine("Initializing the Hook...");
            if (!Hook.Connection.State.Equals(HubConnectionState.Connected))
            {
                await ConnectClient();
            }
            Console.WriteLine("The Hook has been initialized");
        }
    }
}
