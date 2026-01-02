using System.Text;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net;
using HookHub.Core.Models;

namespace HookHub.Core.Helpers
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
        public static async Task<HookWebResponse> RequestAsync(HookNetRequest hookNetRequest)
        {
            HookWebRequest hookWebRequest = hookNetRequest.Request;            
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
                    hookWebResponse.StatusCode = response.StatusCode;
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
                hookWebResponse.StatusCode = HttpStatusCode.InternalServerError;
                hookWebResponse.ReasonPhrase = ex.Message;
            }

            return hookWebResponse;
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

    }
}
