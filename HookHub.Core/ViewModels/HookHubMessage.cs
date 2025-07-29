using Newtonsoft.Json;

using System;
using System.Threading.Tasks;
using HookHub.Core.Models;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using HookHub.Core.ContractResolver;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace HookHub.Core.ViewModels
{
    public static class HookHubMessage
    {
        public static async Task<object> RequestAsync(NetMessage netMessage)
        {
            object response;
            switch (netMessage.RequestType)
            {
                case NetType.HttpRequestMessage:
                    netMessage.ResponseType = NetType.HttpResponseMessage;
                    HookWebRequest solicitudWeb = Deserialize<HookWebRequest>(netMessage.Request);
                    HookWebResponse deserializedResponse = await ProcesarRequestHttpAsync(solicitudWeb);
                    response = Serialize(deserializedResponse);
                    break;
                case NetType.String:
                    netMessage.ResponseType = NetType.String;
                    string requestStr = Deserialize<string>(netMessage.Request);
                    response = Serialize(requestStr);
                    break;
                default:
                    netMessage.ResponseType = NetType.String;
                    response = "The request data type is not supported";
                    break;
            }
            return (response);
        }

        public static async Task<HookWebResponse> ProcesarRequestHttpAsync(HookWebRequest hookWebRequest)
        {
            HookWebResponse hookWebResponse = new HookWebResponse();
            //hookWebResponse.QueryString = hookWebRequest.QueryString;
            hookWebResponse.HttpMethod = hookWebRequest.HttpMethod;

            string mensajeDevuelto = "";
            try
            {
                if (!(hookWebRequest is null))
                {
                    //bool hayFuncionNombreGET = hookWebRequest.QueryString.Contains("(") && hookWebRequest.QueryString.Contains(")");
                    ///*** Procesar llamada de función en método GET *****************************************************************/
                    //if (hayFuncionNombreGET)
                    //{
                    //    string[] metodoGETTokens = hookWebRequest.QueryString.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    //    bool hayFuncionParamsGET = metodoGETTokens.Length > 1;
                    //    string funcionNombre = metodoGETTokens[0];
                    //    string[] funcionParametros = hayFuncionParamsGET ? metodoGETTokens[1].Split(new[] { '\'', ',', '\'' }, StringSplitOptions.RemoveEmptyEntries) : new string[] { };

                    //    switch (funcionNombre)
                    //    {
                    //        case "EcoDAC":
                    //            {
                    //                mensajeDevuelto = hayFuncionParamsGET ? funcionParametros[0] : "";
                    //                break;
                    //            }
                    //        default:
                    //            {
                    //                mensajeDevuelto = "ERROR: La función no está definida";
                    //                break;
                    //            }
                    //    }
                    //}
                    ///*****************************************************************************************/
                    //else
                    //{
                    HttpClient client = new HttpClient();
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
                    mensajeDevuelto = JsonConvert.SerializeObject(byteArray);
                    hookWebResponse.Headers = response.Content?.Headers.ToDictionary(a => a.Key, a => a.Value.AsEnumerable().ToList());
                    /*****************************************************************************************/
                    //}
                }
            }
            catch (Exception ex)
            {
                mensajeDevuelto = ex.Message;
            }

            hookWebResponse.Content = mensajeDevuelto;
            return (hookWebResponse);
        }

        public static string Encode(string value)
        {
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] byteArray = Encoding.UTF8.GetBytes(value);
            byte[] asciiArray = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, byteArray);
            string result = ascii.GetString(asciiArray);
            return (result);
        }

        public static IEnumerable<string> Encode(List<string> value)
        {
            List<string> result = new List<string>();
            value.ForEach(x => result.Add(Encode(x)));
            return (result);
        }

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
