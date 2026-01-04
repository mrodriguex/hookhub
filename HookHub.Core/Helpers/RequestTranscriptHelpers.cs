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
        /// Converts an HttpRequest to an HttpRequestMessage for proxying.
        /// </summary>
        /// <param name="req">The HttpRequest to convert.</param>
        /// <param name="targetUrl">The target URL to proxy to (from query string).</param>
        /// <returns>The converted HttpRequestMessage.</returns>
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest req, string targetUrl = null)
        {
            var msg = new HttpRequestMessage()
                .SetMethod(req)
                .SetHeaders(req)
                .SetContent(req)
                .SetContentType(req);
            
            // Set the target URL for proxying
            msg = msg.SetRequestUri(req, targetUrl);
            
            return msg;
        }

        /// <summary>
        /// Sets the request URI to the target proxy URL.
        /// </summary>
        private static HttpRequestMessage SetRequestUri(this HttpRequestMessage msg, HttpRequest req, string targetUrl)
        {
            if (!string.IsNullOrEmpty(targetUrl))
            {
                // Use the provided target URL
                try
                {
                    // Fix common URL formatting issues
                    targetUrl = targetUrl.Trim();
                    
                    // Fix missing double slash
                    if (targetUrl.StartsWith("http:/") && !targetUrl.StartsWith("http://"))
                        targetUrl = "http://" + targetUrl.Substring(6);
                    else if (targetUrl.StartsWith("https:/") && !targetUrl.StartsWith("https://"))
                        targetUrl = "https://" + targetUrl.Substring(7);
                    
                    msg.RequestUri = new Uri(targetUrl);
                    return msg;
                }
                catch (UriFormatException ex)
                {
                    throw new ArgumentException($"Invalid target URL: {targetUrl}", ex);
                }
            }
            else
            {
                // Fallback: Extract from path (for backward compatibility)
                return msg.SetAbsoluteUriFromPath(req);
            }
        }

        /// <summary>
        /// Extracts target URL from the path (old logic for backward compatibility).
        /// Path format: /Proxy/{userId}/{*targetUrl}
        /// </summary>
        private static HttpRequestMessage SetAbsoluteUriFromPath(this HttpRequestMessage msg, HttpRequest req)
        {
            try
            {
                var path = req.Path.ToString();
                
                // Look for /Proxy/{userId}/ pattern
                if (path.StartsWith("/Proxy/"))
                {
                    var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length >= 3)
                    {
                        // Reconstruct target URL from remaining segments
                        var targetUrl = string.Join("/", segments, 2, segments.Length - 2);
                        
                        // URL decode
                        targetUrl = Uri.UnescapeDataString(targetUrl);
                        
                        // Add scheme if missing
                        if (!targetUrl.Contains("://"))
                        {
                            targetUrl = "http://" + targetUrl;
                        }
                        
                        msg.RequestUri = new Uri(targetUrl);
                        return msg;
                    }
                }
                
                // If pattern not found, use the current request URL (fallback)
                msg.RequestUri = new UriBuilder
                {
                    Scheme = req.Scheme,
                    Host = req.Host.Host,
                    Port = req.Host.Port ?? (req.Scheme == "https" ? 443 : 80),
                    Path = req.PathBase.Add(req.Path),
                    Query = req.QueryString.ToString()
                }.Uri;
                
                return msg;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse target URL from path: {req.Path}", ex);
            }
        }

        /// <summary>
        /// Sets the HTTP method on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        private static HttpRequestMessage SetMethod(this HttpRequestMessage msg, HttpRequest req)
            => msg.Set(m => m.Method = new HttpMethod(req.Method));

        /// <summary>
        /// Sets the headers on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        private static HttpRequestMessage SetHeaders(this HttpRequestMessage msg, HttpRequest req)
        {
            foreach (var header in req.Headers)
            {
                // Skip certain headers that shouldn't be forwarded
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    continue;
                    
                msg.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            return msg;
        }

        /// <summary>
        /// Sets the content on the HttpRequestMessage from the HttpRequest body.
        /// </summary>
        private static HttpRequestMessage SetContent(this HttpRequestMessage msg, HttpRequest req)
        {
            // Only set content for methods that can have a body
            if (req.Method != "GET" && req.Method != "HEAD" && req.Method != "DELETE")
            {
                // Reset the stream position if possible
                if (req.Body.CanSeek)
                {
                    req.Body.Position = 0;
                }
                
                msg.Content = new StreamContent(req.Body);
                
                // Preserve the original Content-Type
                if (!string.IsNullOrEmpty(req.ContentType))
                {
                    msg.Content.Headers.TryAddWithoutValidation("Content-Type", req.ContentType);
                }
            }
            
            return msg;
        }

        /// <summary>
        /// Sets the content type header on the HttpRequestMessage from the HttpRequest.
        /// </summary>
        private static HttpRequestMessage SetContentType(this HttpRequestMessage msg, HttpRequest req)
        {
            if (!string.IsNullOrEmpty(req.ContentType) && msg.Content != null)
            {
                msg.Content.Headers.TryAddWithoutValidation("Content-Type", req.ContentType);
            }
            return msg;
        }

        /// <summary>
        /// Applies a configuration action to the HttpRequestMessage.
        /// </summary>
        private static HttpRequestMessage Set(this HttpRequestMessage msg, Action<HttpRequestMessage> config, bool applyIf = true)
        {
            if (applyIf)
            {
                try
                {
                    config.Invoke(msg);
                }
                catch (Exception)
                {
                    // Silent fail for header additions
                }
            }
            return msg;
        }
    }
}