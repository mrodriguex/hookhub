using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HookHub.Core.Models
{
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

        public Dictionary<string, List<string>> Headers
        {
            get
            {
                _headers ??= new Dictionary<string, List<string>>();
                return (_headers);
            }
            set { _headers = value; }
        }

        public HookWebContent Content
        {
            get
            {
                _content ??= new HookWebContent();
                return (_content);
            }
            set { _content = value; }
        }

        public HttpMethod HttpMethod { get; set; }

        public List<KeyValuePair<string, string>> Cookies
        {
            get
            {
                _cookies ??= new List<KeyValuePair<string, string>>();
                return (_cookies);
            }
            set { _cookies = value; }
        }

        public Uri HookUri { get; set; }
        public Uri HubUri { get; set; }

        public HookWebRequest()
        {
        }

    }
}