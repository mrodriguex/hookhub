using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NCrontab;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HookHub.Web.Services
{
    public class ScheduledHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private List<ScheduledProcess> ScheduledProcesses;

        public ScheduledHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
                IHostEnvironment hostEnvironment = _serviceProvider.GetService<IHostEnvironment>();

            Config.Configure(hostEnvironment.ContentRootPath);
            ScheduledProcesses = Config.Configuration.GetSection("ScheduledProcesses").Get<List<ScheduledProcess>>();

            ScheduledProcesses.ForEach(_schProc =>
            {
                _schProc.ServiceProvider = _serviceProvider;
                //_schProc.ScheduledProcessType = ScheduledProcessType.HttpGet;
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            List<Task> runningTasks = new List<Task>();
            ScheduledProcesses.ForEach(x => runningTasks.Add(x.StartRunning(stoppingToken)));
        }
    }

    public class ScheduledProcess
    {
        private CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;

        private IServiceProvider _serviceProvider;
        private string _schedule;
        private string _processGetUrl;

        public ScheduledProcessType ScheduledProcessType { get; set; }

        public IServiceProvider ServiceProvider {
            get { return _serviceProvider; }
            set { _serviceProvider = value; }
        }
        public string ProcessGetUrl {
            get {
                if (_processGetUrl is null) { _processGetUrl = ""; }
                return (_processGetUrl);
            }
            set { _processGetUrl = value; }
        }
        public Action AsyncProcess { get; set; } = new Action(() => { return; });
        public async void AsyncProcessHttpGet()
        {
            string msg;
            try
            {
                var httpClient = new HttpClient();
                var httpResponse = await httpClient.GetAsync(ProcessGetUrl);

                if (httpResponse.IsSuccessStatusCode) { return; }
                else { msg = httpResponse.ReasonPhrase; }
            }
            catch (Exception ex) { msg = ex.Message; }

            Console.Error.Write(msg);
            FprintErr($"Error al obtener el recurso en: '{ProcessGetUrl}': {msg}");
            return;
        }

        public string Schedule {
            get {
                if (_schedule is null) { _schedule = "0 */1 * * * *"; }
                return (_schedule);
            }
            set {
                _schedule = value;
                _crontabSchedule = CrontabSchedule.Parse(_schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            }
        }

        public ScheduledProcess()
        {
        }

        public ScheduledProcess(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _crontabSchedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.Now);
        }

        public ScheduledProcess(IServiceProvider serviceProvider, string schedule, ScheduledProcessType scheduledProcessType, string processGetUrl)
        {
            ScheduledProcessType = scheduledProcessType;
            ProcessGetUrl = processGetUrl;
            _serviceProvider = serviceProvider;
            Schedule = schedule;
            _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.Now);
        }

        public async Task StartRunning(CancellationToken stoppingToken)
        {
            do
            {
                try
                {
                    DateTime now = DateTime.Now;
                    _nextRun = _crontabSchedule.GetNextOccurrence(now);
                    int diff = (int)(_nextRun - now).TotalMilliseconds;
                    await Task.Delay(diff, stoppingToken);
                    ProcessRunning();
                }
                catch (Exception ex)
                {
                    string errMsg = ex.Message;
                    FprintErr(errMsg);
                }
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private async void ProcessRunning()
        {
            try
            {
                switch (ScheduledProcessType)
                {
                    case ScheduledProcessType.Custom:
                        await Task.Run(AsyncProcess);
                        break;
                    case ScheduledProcessType.HttpGet:
                        await Task.Run(AsyncProcessHttpGet);
                        break;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                FprintErr(errMsg);
            }

        }
        public void FprintErr(string stdErr)
        {
            Console.Error.Write(stdErr);
        }
    }

    public enum ScheduledProcessType
    {
        Custom = 0,
        StoredProcedure = 1,
        HttpGet = 2,
        HttpPost = 3
    }
}
