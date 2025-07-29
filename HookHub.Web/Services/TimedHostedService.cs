using HookHub.Web.Data;
using HookHub.Web.Models;
using HookHub.Web.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NCrontab;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HookHub.Web.Services
{
    public class TimedHostedService : BackgroundService
    {
        private int executionDelayMins = 1;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        private List<ScheduledTaskModel> _tareas;

        public int ExecutionCount = 0;
        public DateTime StartDateTime = DateTime.Now;
        public string ContentRootPath { get; set; }
        public List<ScheduledTaskModel> Tareas
        {
            get
            {
                _tareas ??= new List<ScheduledTaskModel>();
                return (_tareas);
            }
            set { _tareas = value; }
        }

        public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            IHostEnvironment hostEnvironment = _serviceProvider.GetService<IHostEnvironment>();

            Config.Configure(hostEnvironment.ContentRootPath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("El servicio se encuentra ejecutándose.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(executionDelayMins));

            await base.StopAsync(stoppingToken);
        }

        private void DoWork(object state)
        {
            try
            {
                Tareas = TareasProgramadasDA.ObtenerTareasProgramadas(estatus: true);
                Tareas.ForEach(ExecuteScheduledTask);

                var count = Interlocked.Increment(ref ExecutionCount);
                _logger.LogInformation("El servicio se encuentra ejecutándose en segundo plano. Contador: {Count}", count);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                _logger.LogError("Error en el servicio. Error: {Error}", error);
            }
        }

        private void ExecuteScheduledTask(ScheduledTaskModel tarea)
        {
            try
            {
                CrontabSchedule crontabSchedule = CrontabSchedule.Parse(tarea.Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });

                var src = DateTime.Now;
                DateTime now = new DateTime(src.Year, src.Month, src.Day, src.Hour, src.Minute, 0);
                DateTime nextRunSrc = crontabSchedule.GetNextOccurrence(now);
                DateTime nextRun = new DateTime(nextRunSrc.Year, nextRunSrc.Month, nextRunSrc.Day, nextRunSrc.Hour, nextRunSrc.Minute, 0);

                if (now.CompareTo(nextRun).Equals(0))
                {
                    switch (tarea.ScheduledTaskType)
                    {
                        case ScheduledTaskType.Custom:
                            break;
                        case ScheduledTaskType.HttpGet:
                            WebTask.HttpGetAsync(tarea.Tarea);
                            break;
                        case ScheduledTaskType.HttpPost:
                            WebTask.HttpPostAsync(tarea.Tarea);
                            break;
                        case ScheduledTaskType.StoredProcedure:
                            SqlTask.SqlSPAsync(tarea.Tarea);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string tareaNombre = tarea.Nombre;
                string error = ex.Message;
                _logger.LogError("Error en la tarea asíncrona {TareaNombre}. Error: {Error}", tareaNombre, error);
            }
        }

        public async Task Restart(CancellationToken stoppingToken)
        {
            await Stop(stoppingToken).ContinueWith(stoped => Start());
        }

        public async Task Stop(CancellationToken stoppingToken)
        {
            Console.WriteLine("Terminando cliente DAC...");
            await this.StopAsync(stoppingToken);
            Console.WriteLine("El cliente DAC ha terminado");
        }

        public async Task Start()
        {
            Console.WriteLine("Iniciando cliente DAC...");
            this.StartAsync(CancellationToken.None);
            Console.WriteLine("El cliente DAC ha iniciado");
        }
    }
}
