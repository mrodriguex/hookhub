using System;

namespace HookHub.Core.Models
{
    public class NetMessage
    {
        private HookConnection? _hookConnectionFrom;
        private HookConnection? _hookConnectionTo;
        private string? _connectionResponseId;

        public HookConnection HookConnectionFrom {
            get {
                _hookConnectionFrom ??= new HookConnection(); 
                return (_hookConnectionFrom);
            }
            set { _hookConnectionFrom = value; }
        }
        public HookConnection HookConnectionTo {
            get {
                _hookConnectionTo ??= new HookConnection();
                return (_hookConnectionTo);
            }
            set { _hookConnectionTo = value; }
        }

        public string? ConnectionResponseId {
            get {
                if (string.IsNullOrEmpty(_connectionResponseId)) { _connectionResponseId = ""; }
                return (_connectionResponseId);
            }
            set { _connectionResponseId = value; }
        }

        public object Request { get; set; }
        public NetType RequestType { get; set; }
        public object Response { get; set; }

        public NetType ResponseType { get; set; }

        public string ToShortString(object data)
        {
            var dataStr = data as string;
            var shortData = "";
            if (!string.IsNullOrEmpty(dataStr)) {
                shortData = (dataStr.Length > 128 ? dataStr.Substring(0, 128) : dataStr);
            }
            return (shortData);
        }
    }

    public enum NetType
    {
        String = 0,
        HttpRequestMessage = 1,
        HttpResponseMessage = 2
    }
}
