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
    /// Static helper class providing extension methods for converting HttpResponseMessage to HttpResponse.
    /// </summary>
    public static class ResponseTranscriptHelpers
    {
        /// <summary>
        /// Converts an HttpResponseMessage to an HttpResponse by setting status, headers, and body.
        /// </summary>
        /// <param name="resp">The HttpResponse to modify.</param>
        /// <param name="msg">The HttpResponseMessage to convert from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task FromHttpResponseMessage(this HttpResponse resp, HttpResponseMessage msg)
        {
            resp.SetStatusCode(msg)
                .SetHeaders(msg)
                .SetContentType(msg);

            await resp.SetBodyAsync(msg);
        }

        /// <summary>
        /// Sets the status code on the HttpResponse from the HttpResponseMessage.
        /// </summary>
        /// <param name="resp">The HttpResponse to modify.</param>
        /// <param name="msg">The source HttpResponseMessage.</param>
        /// <returns>The modified HttpResponse.</returns>
        private static HttpResponse SetStatusCode(this HttpResponse resp, HttpResponseMessage msg)
            => resp.Set(r => r.StatusCode = (int)msg.StatusCode);

        /// <summary>
        /// Sets the headers on the HttpResponse from the HttpResponseMessage.
        /// </summary>
        /// <param name="resp">The HttpResponse to modify.</param>
        /// <param name="msg">The source HttpResponseMessage.</param>
        /// <returns>The modified HttpResponse.</returns>
        private static HttpResponse SetHeaders(this HttpResponse resp, HttpResponseMessage msg)
            => msg.Headers.Aggregate(resp, (acc, h) => acc.Set(r => r.Headers[h.Key] = new StringValues(h.Value.ToArray())));

        /// <summary>
        /// Sets the body content on the HttpResponse from the HttpResponseMessage.
        /// </summary>
        /// <param name="resp">The HttpResponse to modify.</param>
        /// <param name="msg">The source HttpResponseMessage.</param>
        /// <returns>A task representing the asynchronous operation, returning the modified HttpResponse.</returns>
        private static async Task<HttpResponse> SetBodyAsync(this HttpResponse resp, HttpResponseMessage msg)
        {
            using (var stream = await msg.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();

                return resp.Set(async r => await r.WriteAsync(content));
            }
        }

        /// <summary>
        /// Sets the content type header on the HttpResponse from the HttpResponseMessage.
        /// </summary>
        /// <param name="resp">The HttpResponse to modify.</param>
        /// <param name="msg">The source HttpResponseMessage.</param>
        /// <returns>The modified HttpResponse.</returns>
        private static HttpResponse SetContentType(this HttpResponse resp, HttpResponseMessage msg)
            => resp.Set(r => r.ContentType = msg.Content.Headers.GetValues("Content-Type").Single(), applyIf: msg.Content.Headers.Contains("Content-Type"));

        /// <summary>
        /// Applies a configuration action to the HttpResponse if the condition is met.
        /// </summary>
        /// <param name="msg">The HttpResponse to modify.</param>
        /// <param name="config">The configuration action to apply.</param>
        /// <param name="applyIf">Whether to apply the configuration (default true).</param>
        /// <returns>The modified HttpResponse.</returns>
        private static HttpResponse Set(this HttpResponse msg, Action<HttpResponse> config, bool applyIf = true)
        {
            if (applyIf)
            {
                config.Invoke(msg);
            }

            return msg;
        }
    }
}
