using HookHub.Core.Hooks;
using HookHub.Core.Helpers;
using HookHub.Core.Models;
using HookHub.Core.ViewModels;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HookHub.Hub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MensajeController : ControllerBase
    {
        public CoreHook CoreHook { get; set; }

        public MensajeController(CoreHook netClient)
        {
            CoreHook = netClient;
        }

        ////[HttpGet("Proxy"), HttpPost("Proxy")]

        //[HttpGet("Proxy/{claveUsuarioDestino}")]
        //public async Task<HttpResponseMessage> Proxy(string claveUsuarioDestino)
        //{
        //    HttpRequestMessage request = Request.ToHttpRequestMessage();

        //    HttpResponseMessage response = await CoreHook.SendRequest(hookNameTo: claveUsuarioDestino, request: request, NetType.HttpRequestMessage) as HttpResponseMessage;

        //    return response;
        //}


        [HttpGet("{claveUsuarioDestino}/{*mensajeWebRecibido}")]
        public ContentResult Get(string claveUsuarioDestino, string mensajeWebRecibido)
        {
            var queryString = Request.QueryString.Value;
            var contentType = Request.Headers["ContentType"];
            HookWebRequest solicitudWeb = new HookWebRequest();
            solicitudWeb.HttpMethod = HttpMethod.Get;
            //solicitudWeb.QueryString = mensajeWebRecibido + queryString;
            solicitudWeb.Content.Body = "";
            string solicitudSerializada = JsonConvert.SerializeObject(solicitudWeb);
            string respuestaSerializada = EnviarMensajeDAC(claveUsuarioDestino, solicitudSerializada).Result;
            return this.Content(respuestaSerializada, "application/json");
        }
        

        [HttpPost("{claveUsuarioDestino}/{*mensajeWebRecibido}")]
        public ContentResult Post(string claveUsuarioDestino, string mensajeWebRecibido)
        {
            var queryString = Request.QueryString.Value;
            string cuerpoMensajeWebRecibido = new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEnd();
            HookWebRequest solicitudWeb = new HookWebRequest();
            solicitudWeb.HttpMethod = HttpMethod.Post;
            //solicitudWeb.QueryString = mensajeWebRecibido + queryString;
            solicitudWeb.Content.Body = cuerpoMensajeWebRecibido;
            string solicitudSerializada = JsonConvert.SerializeObject(solicitudWeb);
            string respuestaSerializada = EnviarMensajeDAC(claveUsuarioDestino, solicitudSerializada).Result;
            return this.Content(respuestaSerializada, "application/json");
        }

        [HttpPut("{claveUsuarioDestino}/{*mensajeWebRecibido}")]
        public ContentResult Put(string claveUsuarioDestino, string mensajeWebRecibido)
        {
            var queryString = Request.QueryString.Value;
            string cuerpoMensajeWebRecibido = new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEnd();
            HookWebRequest solicitudWeb = new HookWebRequest();
            solicitudWeb.HttpMethod = HttpMethod.Put;
            //solicitudWeb.QueryString = mensajeWebRecibido + queryString;
            solicitudWeb.Content.Body = cuerpoMensajeWebRecibido;
            string solicitudSerializada = JsonConvert.SerializeObject(solicitudWeb);
            string respuestaSerializada = EnviarMensajeDAC(claveUsuarioDestino, solicitudSerializada).Result;
            return this.Content(respuestaSerializada, "application/json");
        }

        [HttpDelete("{claveUsuarioDestino}/{*mensajeWebRecibido}")]
        public ContentResult Delete(string claveUsuarioDestino, string mensajeWebRecibido)
        {
            var queryString = Request.QueryString.Value;
            string cuerpoMensajeWebRecibido = new StreamReader(HttpContext.Request.Body, Encoding.UTF8).ReadToEnd();
            HookWebRequest solicitudWeb = new HookWebRequest();
            solicitudWeb.HttpMethod = HttpMethod.Delete;
            //solicitudWeb.QueryString = mensajeWebRecibido + queryString;
            solicitudWeb.Content.Body = cuerpoMensajeWebRecibido;
            string solicitudSerializada = JsonConvert.SerializeObject(solicitudWeb);
            string respuestaSerializada = EnviarMensajeDAC(claveUsuarioDestino, solicitudSerializada).Result;
            return this.Content(respuestaSerializada, "application/json");
        }

        private async Task<string> EnviarMensajeDAC(string claveUsuarioDestino, string datosMensaje)
        {
            string respuestaSerializada = "";
            try
            {
                respuestaSerializada = await CoreHook.SendRequest(hookNameTo: claveUsuarioDestino, request: datosMensaje) as string;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                respuestaSerializada = ex.Message;
            }
            return (respuestaSerializada);
        }
    }
}

