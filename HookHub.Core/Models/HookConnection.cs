namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents metadata for a connected hook, including connection details and keep-alive status.
    /// </summary>
    public class HookConnection
    {

        private int _timeIntervals_KeepAlive;
        private int _timeIntervals_TimeOutResponse;
        private string _hookName = default!;
        private string _connectionId = default!;
        private string _hookHubNetURL = default!;

        /// <summary>
        /// Gets or sets the keep-alive interval in milliseconds. Minimum value is 60000 (1 minute).
        /// </summary>
        public int TimeIntervals_KeepAlive
        {
            get
            {
                return Math.Max(_timeIntervals_KeepAlive, 60000);
            }
            set
            {
                _timeIntervals_KeepAlive = Math.Max(value, 60000);
            }
        }

        /// <summary>
        /// Gets or sets the name of the hook.
        /// </summary>
        public string HookName
        {
            get
            {
                _hookName ??= "";
                return (_hookName);
            }
            set { _hookName = value; }
        }

        /// <summary>
        /// Gets or sets the SignalR connection ID.
        /// </summary>
        public string ConnectionId
        {
            get
            {
                _connectionId ??= "";
                return (_connectionId);
            }
            set { _connectionId = value; }
        }

        /// <summary>
        /// Gets or sets the timestamp of the last keep-alive message.
        /// </summary>
        public DateTime LastKeepAlive
        {
            get;
            set;
        } = DateTime.UtcNow;
        public int TimeIntervals_TimeOutResponse
        {
            get
            {
                return Math.Max(_timeIntervals_TimeOutResponse, 10000);
            }
            set
            {
                _timeIntervals_TimeOutResponse = Math.Max(value, 10000);
            }
        }

        public string HookHubNetURL { 
            get
            {
                _hookHubNetURL ??= "";
                return (_hookHubNetURL);
            }
            set { _hookHubNetURL = value; } }
    }
}
