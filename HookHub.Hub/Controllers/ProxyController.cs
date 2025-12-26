using HookHub.Core.Helpers;
using HookHub.Core.Models;
using HookHub.Core.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using System.Text;

namespace HookHub.Hub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : Controller
    {
        private readonly ILogger<ProxyController> _logger;
        public Worker Worker { get; set; }

        public ProxyController(Worker worker, ILogger<ProxyController> logger)
        {
            Worker = worker;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

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
    
    public static class HttpRequestMessageExtensions
    {
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

    public class HttpResponseMessageResult : IActionResult
    {
        private readonly HttpResponseMessage _responseMessage;

        public HttpResponseMessageResult(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage; // could add throw if null
        }

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
