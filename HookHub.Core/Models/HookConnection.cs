namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents metadata for a connected hook, including connection details and keep-alive status.
    /// </summary>
    public class HookConnection
    {

        int _timeIntervals_KeepAlive;

        private string _hookName = default!;
        private string _connectionId = default!;

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

        /// <summary>
        /// Gets a value indicating whether the connection has timed out based on the keep-alive interval.
        /// </summary>
        public bool IsTimedOut
        {
            get
            {
                DateTime nowTimeOut = LastKeepAlive.AddMilliseconds(TimeIntervals_KeepAlive);
                return (DateTime.UtcNow.CompareTo(nowTimeOut) > 0);
            }
        }

    }
}
