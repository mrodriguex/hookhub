using System;
using System.Net;
using System.Net.Sockets;

namespace HookHub.Core.Models
{
    public class HookConnection
    {
        public virtual int TimeIntervals_KeepAlive
        {
            get
            {
                int timeIntervals_KeepAlive = 0;
                int.TryParse(Config.TimeIntervals("KeepAlive"), out timeIntervals_KeepAlive);
                return timeIntervals_KeepAlive;
            }
        }

        private string _hookName;
        private string _connectionID;

        public string HookName
        {
            get
            {
                _hookName ??= "";
                return (_hookName);
            }
            set { _hookName = value; }
        }

        public string ConnectionID
        {
            get
            {
                _connectionID ??= "";
                return (_connectionID);
            }
            set { _connectionID = value; }
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
