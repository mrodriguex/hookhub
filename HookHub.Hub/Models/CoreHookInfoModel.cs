using HookHub.Core.Hooks;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace HookHub.Hub.Models
{
    public class CoreHookInfoModel
    {
        private ConcurrentDictionary<string, TaskCompletionSource<object>> _connectionResponses;
        private string _cryoNETURL;
        private HookConnection _hookConnection;

        public ConcurrentDictionary<string, TaskCompletionSource<object>> ConnectionResponses {
            get {
                _connectionResponses ??= new ConcurrentDictionary<string, TaskCompletionSource<object>>();
                return (_connectionResponses);
            }
            set {
                _connectionResponses = value;
            }
        }

        public CoreHookInfoModel()
        {

        }
        public CoreHookInfoModel(CoreHook netClient)
        {
            CopyFrom(netClient);
        }

        public string HookHubNetURL { get { return _cryoNETURL ?? ""; } set { _cryoNETURL = value; } }

        public HubConnection Connection { get; set; }

        public HookConnection HookConnection {
            get {
                _hookConnection ??= new HookConnection();
                return (_hookConnection);
            }
            set { _hookConnection = value; }
        }
        public void CopyFrom(CoreHook netClient)
        {
            ConnectionResponses = netClient.ConnectionResponses;
            HookHubNetURL = netClient.HookHubNetURL;
            Connection = netClient.Connection;
            HookConnection = netClient.HookConnection;
        }
    }
}