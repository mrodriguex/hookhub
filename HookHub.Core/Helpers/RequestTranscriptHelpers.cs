using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HookHub.Core.Helpers
{
    /// <summary>
    /// Static helper class providing extension methods for converting HttpRequest to HttpRequestMessage.
    /// </summary>
    public static class RequestTranscriptHelpers
    {
        /// <summary>
        /// Converts an HttpRequest to an HttpRequestMessage.
        /// </summary>
        /// <param name="req">The HttpRequest to convert.</param>
        /// <returns>The converted HttpRequestMessage.</returns>
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest req)
            => new HttpRequestMessage()
                .SetMethod(req)
                .SetAbsoluteUri(req)
                .SetHeaders(req)
                .SetContent(req)
                .SetContentType(req)
                ;

        /// <summary>
        /// Sets the absolute URI on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="req">The source HttpRequest.</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage SetAbsoluteUri(this HttpRequestMessage msg, HttpRequest req)
            => msg.Set(m => m.RequestUri = new UriBuilder
            {
                Scheme = req.Scheme,
                Host = req.Host.Host,
                Port = req.Host.Port.Value,
                Path = req.PathBase.Add(req.Path),
                Query = req.QueryString.ToString()
            }.Uri);

        /// <summary>
        /// Sets the HTTP method on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="req">The source HttpRequest.</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage SetMethod(this HttpRequestMessage msg, HttpRequest req)
            => msg.Set(m => m.Method = new HttpMethod(req.Method));

        /// <summary>
        /// Sets the headers on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="req">The source HttpRequest.</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage SetHeaders(this HttpRequestMessage msg, HttpRequest req)
            => req.Headers.Aggregate(msg, (acc, h) => acc.Set(m => m.Headers.TryAddWithoutValidation(h.Key, h.Value.AsEnumerable())));

        /// <summary>
        /// Sets the content on the HttpRequestMessage from the HttpRequest body.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="req">The source HttpRequest.</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage SetContent(this HttpRequestMessage msg, HttpRequest req)
            => msg.Set(m => m.Content = new StreamContent(req.Body), false);

        /// <summary>
        /// Sets the content type header on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="req">The source HttpRequest.</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage SetContentType(this HttpRequestMessage msg, HttpRequest req)
            => msg.Set(m => {
                m.Content?.Headers.Add("Content-Type", req.ContentType);
                
                }, applyIf: req.Headers.ContainsKey("Content-Type"));

        /// <summary>
        /// Applies a configuration action to the HttpRequestMessage if the condition is met.
        /// </summary>
        /// <param name="msg">The HttpRequestMessage to modify.</param>
        /// <param name="config">The configuration action to apply.</param>
        /// <param name="applyIf">Whether to apply the configuration (default true).</param>
        /// <returns>The modified HttpRequestMessage.</returns>
        private static HttpRequestMessage Set(this HttpRequestMessage msg, Action<HttpRequestMessage> config, bool applyIf = true)
        {
            if (applyIf)
            {
                try
                {
                    config.Invoke(msg);
                }
                catch (Exception ex)
                { }
            }

            return msg;
        }
    }


}
