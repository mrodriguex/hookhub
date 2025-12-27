using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using System.Collections.Generic;
using System.Net.Http;

namespace HookHub.Core.Models
{
    /// <summary>
    /// Represents the content of a web request or response, including headers, body, and form data.
    /// </summary>
    public class HookWebContent
    {
        private Dictionary<string, List<string>> _headers;
        private string _body;
        private List<KeyValuePair<string, List<string>>> _form;

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
        /// Gets or sets the body content as a string.
        /// </summary>
        public string Body
        {
            get
            {
                _body ??= "";
                return (_body);
            }
            set { _body = value; }
        }

        /// <summary>
        /// Gets or sets the form data as a list of key-value pairs.
        /// </summary>
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