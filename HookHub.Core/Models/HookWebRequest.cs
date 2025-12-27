using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents a web request to be proxied or sent between hooks, including URI, headers, content, and cookies.
    /// </summary>
    public class HookWebRequest
    {
        private string _queryString;
        private Dictionary<string, List<string>> _headers;
        private HookWebContent _content;
        private List<KeyValuePair<string, string>> _cookies;
        private string _scheme;
        private string _host;
        private string _pathBase;
        private string _path;

        //public string QueryString
        //{
        //    get
        //    {
        //        _queryString ??= "";
        //        return (_queryString);
        //    }
        //    set { _queryString = value; }
        //}

        /// <summary>
        /// Gets or sets the headers as a dictionary of header names to lists of values.
        /// </summary>
        public Dictionary<string, List<string>> Headers
        {
            get
            {
                _headers ??= new Dictionary<string, List<string>>();
                return (_headers);
            }
            set { _headers = value; }
        }

        /// <summary>
        /// Gets or sets the content of the request.
        /// </summary>
        public HookWebContent Content
        {
            get
            {
                _content ??= new HookWebContent();
                return (_content);
            }
            set { _content = value; }
        }

        /// <summary>
        /// Gets or sets the HTTP method of the request.
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the cookies as a list of key-value pairs.
        /// </summary>
        public List<KeyValuePair<string, string>> Cookies
        {
            get
            {
                _cookies ??= new List<KeyValuePair<string, string>>();
                return (_cookies);
            }
            set { _cookies = value; }
        }

        /// <summary>
        /// Gets or sets the URI of the hook target.
        /// </summary>
        public Uri HookUri { get; set; }

        /// <summary>
        /// Gets or sets the URI of the hub.
        /// </summary>
        public Uri HubUri { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookWebRequest"/> class.
        /// </summary>
        public HookWebRequest()
        {
        }

    }
}