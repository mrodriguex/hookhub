using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using System.Collections.Generic;
using System.Net.Http;

namespace HookHub.Core.Models
{
    public class HookWebContent
    {
        private Dictionary<string, List<string>> _headers;
        private string _body;
        private List<KeyValuePair<string, List<string>>> _form;

        public Dictionary<string, List<string>> Headers
        {
            get
            {
                _headers ??= new Dictionary<string, List<string>>();
                return (_headers);
            }
            set { _headers = value; }
        }
        public string Body
        {
            get
            {
                _body ??= "";
                return (_body);
            }
            set { _body = value; }
        }

        public List<KeyValuePair<string, List<string>>> Form
        {
            get
            {
                _form ??= new List<KeyValuePair<string, List<string>>>();
                return (_form);
            }
            set { _form = value; }
        }
    }
}