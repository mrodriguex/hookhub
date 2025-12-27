using Newtonsoft.Json;

using HookHub.Core.Models;
using System.Text;
using System.Net.Http.Headers;
using HookHub.Core.ContractResolver;
using System.Net.Security;

namespace HookHub.Core.ViewModels
{
    /// <summary>
    /// Provides static methods for handling web requests, responses, encoding, and serialization in the HookHub system.
    /// </summary>
    public static class HookHubMessage
    {
        /// <summary>
        /// Processes a network message by deserializing the request, processing it asynchronously, and serializing the response.
        /// </summary>
        /// <param name="netMessage">The network message containing the request details.</param>
        /// <returns>A task that represents the asynchronous operation, containing the serialized response object.</returns>
        public static async Task<object> RequestAsync(NetMessage netMessage)
        {
            object response;
            netMessage.ResponseType = NetType.HttpResponseMessage;
            HookWebRequest solicitudWeb = Deserialize<HookWebRequest>(netMessage.Request);
            HookWebResponse deserializedResponse = await ProcesarRequestHttpAsync(solicitudWeb);
            response = Serialize(deserializedResponse);
            return (response);
        }

        /// <summary>
        /// Processes an HTTP web request asynchronously, handling different content types and proxying the request.
        /// </summary>
        /// <param name="hookWebRequest">The web request to process.</param>
        /// <returns>A task that represents the asynchronous operation, containing the web response.</returns>
        public static async Task<HookWebResponse> ProcesarRequestHttpAsync(HookWebRequest hookWebRequest)
        {
            HookWebResponse hookWebResponse = new HookWebResponse();
            hookWebResponse.HttpMethod = hookWebRequest.HttpMethod;

            try
            {

                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                    {
                        if (sslPolicyErrors == SslPolicyErrors.None)
                            return true;
                        Console.WriteLine($"Certificate error: {sslPolicyErrors}");
                        return true;
                    },
                    UseDefaultCredentials = false,
                    AllowAutoRedirect = false,
                    MaxConnectionsPerServer = 10
                };


                if (!(hookWebRequest is null))
                {
                    HttpClient client = new HttpClient(handler);
                    client.Timeout = TimeSpan.FromSeconds(512);

                    List<string> contentTypes;
                    hookWebRequest.Headers.TryGetValue("Content-Type", out contentTypes);
                    contentTypes ??= new List<string>();

                    string mediaTypeString = (contentTypes.FirstOrDefault() ?? "text/html").Split(";").FirstOrDefault();
                    MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaTypeString);

                    HttpContent contentPostBody = new StringContent(hookWebRequest.Content.Body, Encoding.UTF8, contentType.MediaType);
                    contentPostBody.Headers.ContentType = contentType;

                    HttpRequestMessage peticion = new HttpRequestMessage(hookWebRequest.HttpMethod, hookWebRequest.HookUri);

                    switch (contentType.MediaType)
                    {
                        case "application/x-www-form-urlencoded":
                            FormUrlEncodedContent formUrlEncodedContent = new FormUrlEncodedContent(hookWebRequest.Content.Form.ToDictionary(a => a.Key, a => string.Join(";", a.Value)));
                            hookWebRequest.Headers.ToList().ForEach(x => formUrlEncodedContent.Headers.TryAddWithoutValidation(x.Key, x.Value));
                            hookWebRequest.Content.Form.ForEach(x => x.Value.ForEach(y => formUrlEncodedContent.Headers.TryAddWithoutValidation(Encode(x.Key), y)));
                            peticion.Content = formUrlEncodedContent;
                            break;
                        case "multipart/form-data":
                            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                            hookWebRequest.Headers.ToList().ForEach(x => multipartFormDataContent.Headers.TryAddWithoutValidation(Encode(x.Key), Encode(x.Value)));
                            hookWebRequest.Content.Form.ForEach(x => x.Value.ForEach(y => multipartFormDataContent.Add(new StringContent(y), x.Key)));
                            peticion.Content = multipartFormDataContent;
                            break;
                        default:
                            peticion.Content = contentPostBody;
                            hookWebRequest.Headers.ToList().ForEach(x => contentPostBody.Headers.TryAddWithoutValidation(Encode(x.Key), Encode(x.Value)));
                            break;
                    }

                    hookWebRequest.Cookies.ForEach(x => peticion.Properties.Add(x.Key, x.Value));

                    HttpResponseMessage response = client.SendAsync(peticion, HttpCompletionOption.ResponseHeadersRead).Result;

                    string contentTypeResponse = response.IsSuccessStatusCode ? response.Content.Headers.ContentType.MediaType : "";
                    if (contentTypeResponse.Equals("text/html") || contentTypeResponse.Equals("text/css") || contentTypeResponse.Equals("text/javascript"))
                    {
                        string contentString = await response.Content.ReadAsStringAsync();
                        string scheme = response.RequestMessage.RequestUri.Scheme;
                        string host = response.RequestMessage.RequestUri.Host;
                        string port = response.RequestMessage.RequestUri.Port.ToString();
                        string hookUriSchemeHost = $"{scheme}://{host}";
                        string hookUriSchemeHostPort = $"{scheme}://{host}:{port}";
                        string hubUriReplace = $"{hookWebRequest.HubUri}{hookUriSchemeHost}";
                        string hubUriReplacePort = $"{hookWebRequest.HubUri}{hookUriSchemeHostPort}";
                        contentString = contentString.Replace("localhost", host);
                        contentString = contentString.Replace($"{hookUriSchemeHostPort}/", $"{hubUriReplacePort}/");
                        contentString = contentString.Replace($"{hookUriSchemeHost}/", $"{hubUriReplace}/");
                        //contentString = contentString.Replace(hookUriSchemeHost, hubUriReplace);
                        contentString = contentString.Replace("\"/", $"\"{hubUriReplacePort}/");
                        contentString = contentString.Replace("\"./", $"\"{hubUriReplacePort}/");
                        contentString = contentString.Replace("'/", $"'{hubUriReplacePort}/");
                        contentString = contentString.Replace("'./", $"'{hubUriReplacePort}/");
                        response.Content = new StringContent(contentString, Encoding.UTF8, contentTypeResponse);
                    }

                    hookWebResponse.IsSuccessStatusCode = response.IsSuccessStatusCode;
                    hookWebResponse.StatusCode = (int)response.StatusCode;
                    hookWebResponse.ReasonPhrase = response.ReasonPhrase;

                    var byteArray = await response.Content.ReadAsByteArrayAsync();
                    hookWebResponse.Content = byteArray;
                    hookWebResponse.Headers = response.Content?.Headers.ToDictionary(a => a.Key, a => a.Value.AsEnumerable().ToList());
                    /*****************************************************************************************/
                    //}
                }
            }
            catch (Exception ex)
            {
                hookWebResponse.Content = Encoding.UTF8.GetBytes(ex.Message);
                hookWebResponse.StatusCode = 500;
                hookWebResponse.IsSuccessStatusCode = false;
                hookWebResponse.ReasonPhrase = ex.Message;
            }

            return (hookWebResponse);
        }

        /// <summary>
        /// Encodes a string from UTF-8 to ASCII.
        /// </summary>
        /// <param name="value">The string to encode.</param>
        /// <returns>The ASCII-encoded string.</returns>
        public static string Encode(string value)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] byteArray = Encoding.UTF8.GetBytes(value);
            byte[] asciiArray = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);
            string result = ascii.GetString(asciiArray);
            return (result);
        }

        /// <summary>
        /// Encodes a list of strings from UTF-8 to ASCII.
        /// </summary>
        /// <param name="value">The list of strings to encode.</param>
        /// <returns>An enumerable of ASCII-encoded strings.</returns>
        public static IEnumerable<string> Encode(List<string> value)
        {
            List<string> result = new List<string>();
            value.ForEach(x => result.Add(Encode(x)));
            return (result);
        }

        /// <summary>
        /// Deserializes a JSON string to the specified type using custom settings.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="response">The object to deserialize.</param>
        /// <returns>The deserialized object of type T.</returns>
        internal static T Deserialize<T>(object response)
        {
            T responseDeserialized = JsonConvert.DeserializeObject<T>(response.ToString(), new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new IgnoreErrorPropertiesResolver(),
                TypeNameHandling = TypeNameHandling.Auto
            });
            return (responseDeserialized);
        }

        /// <summary>
        /// Serializes an object to a JSON string using custom settings.
        /// </summary>
        /// <param name="request">The object to serialize.</param>
        /// <returns>The JSON string representation of the object.</returns>
        internal static string Serialize(object request)
        {
            string requestSerialized = JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new IgnoreErrorPropertiesResolver(),
                TypeNameHandling = TypeNameHandling.Auto
            });
            return (requestSerialized);
        }
    }
}
