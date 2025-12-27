using HookHub.Core.Helpers;
using HookHub.Core.Models;
using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using System.Text;

namespace HookHub.Hub.Controllers
{
    /// <summary>
    /// API controller that acts as a proxy for forwarding HTTP requests to connected hooks.
    /// Routes incoming requests to specific hook destinations based on user key.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : Controller
    {
        /// <summary>
        /// Logger for logging proxy operations and errors.
        /// </summary>
        private readonly ILogger<ProxyController> _logger;

        /// <summary>
        /// The background worker managing the hub and hook connections.
        /// </summary>
        public Worker Worker { get; set; }

        /// <summary>
        /// Constructor. Injects the Worker and logger dependencies.
        /// </summary>
        /// <param name="worker">The Worker instance managing the hub.</param>
        /// <param name="logger">The logger for logging operations.</param>
        public ProxyController(Worker worker, ILogger<ProxyController> logger)
        {
            Worker = worker;
            _logger = logger;
        }

        /// <summary>
        /// Displays the proxy index page.
        /// </summary>
        /// <returns>The proxy view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Proxies HTTP requests (GET, POST, PUT, DELETE) to a specific hook destination.
        /// Constructs a HookWebRequest from the incoming request and forwards it via the hub.
        /// </summary>
        /// <param name="claveUsuarioDestino">The destination user key for the hook.</param>
        /// <param name="proxedUrl">The URL to proxy the request to.</param>
        /// <returns>The response from the hook or an error result.</returns>
        [HttpGet("{claveUsuarioDestino}/{*proxedUrl}"),
            HttpPost("{claveUsuarioDestino}/{*proxedUrl}"),
            HttpPut("{claveUsuarioDestino}/{*proxedUrl}"),
            HttpDelete("{claveUsuarioDestino}/{*proxedUrl}")]
        public ActionResult Proxy(string claveUsuarioDestino, string proxedUrl)
        {
            ActionResult actionResult = NotFound();
            try
            {

                var requestQueryString = Request.QueryString.Value;
                HttpRequestMessage httpRequestMessage = Request.ToHttpRequestMessage();

                HookWebRequest hookWebRequest = new HookWebRequest();

                if ((proxedUrl.StartsWith("http:/") && !proxedUrl.StartsWith("http://")) || (proxedUrl.StartsWith("https:/") && !proxedUrl.StartsWith("https://")))
                {
                    proxedUrl = proxedUrl.Replace("http:/", "http://").Replace("https:/", "https://");
                }

                hookWebRequest.HookUri = new Uri($"{proxedUrl}{requestQueryString}");
                hookWebRequest.HubUri = new Uri($"{httpRequestMessage.RequestUri}".Replace($"{hookWebRequest.HookUri}", ""));
                hookWebRequest.HttpMethod = httpRequestMessage.Method;  // GetHttpMethod( Request.Method);
                hookWebRequest.Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.AsEnumerable().ToList()); //hookWebResponse.Headers = response.Headers.ToDictionary(a => a.Key, a => a.Value);
                hookWebRequest.Cookies = Request.Cookies.ToList();  // .ToDictionary(a => a.Key, a => a.Value.AsEnumerable().ToList()); //hookWebResponse.Headers = response.Headers.ToDictionary(a => a.Key, a => a.Value);

                string contentType = Request.ContentType;

                hookWebRequest.Content.Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.AsEnumerable().ToList());
                if (!hookWebRequest.HttpMethod.Equals(HttpMethod.Get))
                {
                    hookWebRequest.Content.Body = new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEnd();

                    if (HttpContext.Request.HasFormContentType)
                    {
                        hookWebRequest.Content.Form = HttpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.ToList()).ToList();
                    }
                }

                HookWebResponse hookWebResponse = EnviarMensajeHook(claveUsuarioDestino, hookWebRequest, NetType.HttpRequestMessage).Result;

                if (hookWebResponse is not null)
                {
                    try
                    {
                        switch ((System.Net.HttpStatusCode)hookWebResponse.StatusCode)
                        {
                            case System.Net.HttpStatusCode.OK:
                                List<string> contentTypesResponse;
                                //string contentTypeResponse = "";
                                hookWebResponse.Headers.TryGetValue("Content-Type", out contentTypesResponse);
                                contentTypesResponse ??= new List<string>();
                                string contentTypeResponse = contentTypesResponse.FirstOrDefault() ?? contentType ?? "";
                                actionResult = File(hookWebResponse.Content, contentTypeResponse);
                                break;
                            case System.Net.HttpStatusCode.BadRequest:
                                actionResult = BadRequest(hookWebResponse.ReasonPhrase);
                                break;
                            case System.Net.HttpStatusCode.NotFound:
                                actionResult = NotFound();
                                break;
                            case System.Net.HttpStatusCode.InternalServerError:
                                actionResult = BadRequest(hookWebResponse.ReasonPhrase);
                                break;
                            case System.Net.HttpStatusCode.Unauthorized:
                                actionResult = Unauthorized(hookWebResponse.ReasonPhrase);
                                break;
                            default:
                                actionResult = NotFound();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        actionResult = BadRequest($"{ex.Message}: {hookWebResponse.Content.ToString()}");
                    }
                }
                else
                {
                    actionResult = NotFound();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                actionResult = BadRequest($"{ex.Message}");
            }
            return actionResult;
        }

        /// <summary>
        /// Sends a request to the specified hook destination via the hub.
        /// </summary>
        /// <param name="claveUsuarioDestino">The destination user key.</param>
        /// <param name="request">The request object to send.</param>
        /// <param name="requestType">The type of the request.</param>
        /// <returns>The hook web response from the destination.</returns>
        private async Task<HookWebResponse> EnviarMensajeHook(string claveUsuarioDestino, object request, NetType requestType = NetType.HttpRequestMessage)
        {
            HookWebResponse hookWebResponse = new HookWebResponse();
            try
            {
                hookWebResponse = await Worker.Hook.SendRequest(hookNameTo: claveUsuarioDestino, request: request, requestType: requestType) as HookWebResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                hookWebResponse.Content = ex.Message != null ? Encoding.UTF8.GetBytes(ex.Message) : new byte[0];
                hookWebResponse.StatusCode = 500;
                hookWebResponse.IsSuccessStatusCode = false;
                hookWebResponse.ReasonPhrase = ex.Message;                
            }
            return (hookWebResponse);
        }
    }
    
    /// <summary>
    /// Extension methods for HttpRequestMessage to support cloning.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Clones an HttpRequestMessage asynchronously, including headers, content, and properties.
        /// </summary>
        /// <param name="req">The original HttpRequestMessage to clone.</param>
        /// <returns>A cloned HttpRequestMessage.</returns>
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;

                if ((ms.Length > 0 || req.Content.Headers.Any()) && clone.Method != HttpMethod.Get)
                {
                    clone.Content = new StreamContent(ms);

                    if (req.Content.Headers != null)
                        foreach (var h in req.Content.Headers)
                            clone.Content.Headers.Add(h.Key, h.Value);
                }
            }

            clone.Version = req.Version;

            foreach (var prop in req.Properties)
                clone.Properties.Add(prop);

            foreach (var header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;
        }
    }

    /// <summary>
    /// ActionResult implementation that returns an HttpResponseMessage.
    /// Allows MVC actions to return HttpResponseMessage directly.
    /// </summary>
    public class HttpResponseMessageResult : IActionResult
    {
        /// <summary>
        /// The HttpResponseMessage to return.
        /// </summary>
        private readonly HttpResponseMessage _responseMessage;

        /// <summary>
        /// Constructor. Initializes the result with the response message.
        /// </summary>
        /// <param name="responseMessage">The HttpResponseMessage to return.</param>
        public HttpResponseMessageResult(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage; // could add throw if null
        }

        /// <summary>
        /// Executes the result asynchronously, copying the response to the HTTP context.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_responseMessage.StatusCode;

            foreach (var header in _responseMessage.Headers)
            {
                context.HttpContext.Response.Headers.TryAdd(header.Key, new StringValues(header.Value.ToArray()));
            }

            using (var stream = await _responseMessage.Content.ReadAsStreamAsync())
            {
                await stream.CopyToAsync(context.HttpContext.Response.Body);
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }
}
