using HookHub.Core.Hooks;
using HookHub.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using System.Collections.Concurrent;

namespace HookHub.Hub.Models
{
    /// <summary>
    /// Model representing information about a core hook connection.
    /// Contains connection details, responses, and hub connection state.
    /// </summary>
    public class CoreHookInfoModel
    {
        /// <summary>
        /// Dictionary of connection responses keyed by some identifier.
        /// Used for tracking asynchronous responses from hook connections.
        /// </summary>
        private ConcurrentDictionary<string, TaskCompletionSource<HookWebResponse>>? _connectionResponses;

        /// <summary>
        /// The URL of the hook hub network.
        /// </summary>
        private string? _hookHubNetURL;

        /// <summary>
        /// The hook connection details.
        /// </summary>
        private HookConnection? _hookConnection;

        /// <summary>
        /// Gets or sets the connection responses dictionary.
        /// Lazily initializes if null.
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
        /// Constructor that initializes the model from a CoreHook instance.
        /// </summary>
        /// <param name="hook">The CoreHook to copy data from.</param>
        public CoreHookInfoModel(CoreHook hook)
        {
            CopyFrom(hook);
        }

        /// <summary>
        /// Gets or sets the hook hub network URL.
        /// </summary>
        public string HookHubNetURL { get { return _hookHubNetURL ?? ""; } set { _hookHubNetURL = value; } }

        /// <summary>
        /// The SignalR hub connection.
        /// </summary>
        public HubConnection? Connection { get; set; }

        /// <summary>
        /// Gets or sets the hook connection details.
        /// Lazily initializes if null.
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

        /// <summary>
        /// Copies data from a CoreHook instance to this model.
        /// </summary>
        /// <param name="hook">The CoreHook to copy from.</param>
        public void CopyFrom(CoreHook hook)
        {
            ConnectionResponses = hook.ConnectionResponses;
            HookHubNetURL = hook.HookConnection.HookHubNetURL;
            Connection = hook.Connection;
            HookConnection = hook.HookConnection;
        }
    }
}