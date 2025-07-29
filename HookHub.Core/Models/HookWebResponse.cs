using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace HookHub.Core.Models
{
    public class HookWebResponse
    {
        private string _queryString;
        private string _content;
        private Dictionary<string, List<string>> _headers;

        public string QueryString
        {
            get
            {
                _queryString ??= "";
                return (_queryString);
            }
            set { _queryString = value; }
        }

        public Dictionary<string, List<string>> Headers
        {
            get
            {
                _headers ??= new Dictionary<string, List<string>>();
                return (_headers);
            }
            set { _headers = value; }
        }

        public string Content
        {
            get
            {
                _content ??= "";
                return (_content);
            }
            set { _content = value; }
        }

        public HttpMethod HttpMethod { get; set; } = HttpMethod.Get;
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public bool IsSuccessStatusCode { get; set; }

        public HookWebResponse()
        {
        }

    }
}
