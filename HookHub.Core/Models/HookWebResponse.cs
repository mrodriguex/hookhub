using System.Net;

namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents a web response from a hook, including status, headers, and content.
    /// </summary>
    public class HookWebResponse
    {
        private string _queryString;
        private byte[] _content;
        private Dictionary<string, List<string>> _headers;

        /// <summary>
        /// Gets or sets the query string of the response.
        /// </summary>
        public string QueryString
        {
            get
            {
                _queryString ??= "";
                return _queryString;
            }
            set { _queryString = value; }
        }

        /// <summary>
        /// Gets or sets the headers as a dictionary of header names to lists of values.
        /// </summary>
        public Dictionary<string, List<string>> Headers
        {
            get
            {
                _headers ??= new Dictionary<string, List<string>>();
                return _headers;
            }
            set { _headers = value; }
        }

        /// <summary>
        /// Gets or sets the content as a byte array.
        /// </summary>
        public byte[] Content
        {
            get
            {
                _content ??= new byte[0];
                return (_content);
            }
            set { _content = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP method associated with the response.
        /// </summary>
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;

        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the reason phrase for the status code.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the status code indicates success.
        /// </summary>
        public bool IsSuccessStatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookWebResponse"/> class.
        /// </summary>
        public HookWebResponse()
        {
        }

    }
}
