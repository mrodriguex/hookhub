namespace HookHub.Core.Models
{
    public class HookConnection
    {

        int _timeIntervals_KeepAlive;

        private string _hookName = default!;
        private string _connectionId = default!;

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

        public string HookName
        {
            get
            {
                _hookName ??= "";
                return (_hookName);
            }
            set { _hookName = value; }
        }

        public string ConnectionId
        {
            get
            {
                _connectionId ??= "";
                return (_connectionId);
            }
            set { _connectionId = value; }
        }

        public DateTime LastKeepAlive
        {
            get;
            set;
        } = DateTime.UtcNow;

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
