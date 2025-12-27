using System;

namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents a network message exchanged between hooks, containing request/response data and connection info.
    /// </summary>
    public class NetMessage
    {
        private HookConnection? _hookConnectionFrom;
        private HookConnection? _hookConnectionTo;
        private string? _connectionResponseId;

        /// <summary>
        /// Gets or sets the originating hook connection.
        /// </summary>
        public HookConnection HookConnectionFrom {
            get {
                _hookConnectionFrom ??= new HookConnection(); 
                return (_hookConnectionFrom);
            }
            set { _hookConnectionFrom = value; }
        }

        /// <summary>
        /// Gets or sets the target hook connection.
        /// </summary>
        public HookConnection HookConnectionTo {
            get {
                _hookConnectionTo ??= new HookConnection();
                return (_hookConnectionTo);
            }
            set { _hookConnectionTo = value; }
        }

        /// <summary>
        /// Gets or sets the unique ID for tracking request/response pairs.
        /// </summary>
        public string? ConnectionResponseId {
            get {
                if (string.IsNullOrEmpty(_connectionResponseId)) { _connectionResponseId = ""; }
                return (_connectionResponseId);
            }
            set { _connectionResponseId = value; }
        }

        /// <summary>
        /// Gets or sets the request data.
        /// </summary>
        public object Request { get; set; }

        /// <summary>
        /// Gets or sets the type of the request.
        /// </summary>
        public NetType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public object Response { get; set; }

        /// <summary>
        /// Gets or sets the type of the response.
        /// </summary>
        public NetType ResponseType { get; set; }

        /// <summary>
        /// Converts the data object to a short string representation (max 128 characters).
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <returns>A short string representation of the data.</returns>
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

    /// <summary>
    /// Enumeration of network message types.
    /// </summary>
    public enum NetType
    {
        /// <summary>
        /// String message type.
        /// </summary>
        String = 0,

        /// <summary>
        /// HTTP request message type.
        /// </summary>
        HttpRequestMessage = 1,

        /// <summary>
        /// HTTP response message type.
        /// </summary>
        HttpResponseMessage = 2
    }
}
