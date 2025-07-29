using HookHub.Web.Models;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HookHub.Web.Tasks
{
    public static class WebTask
    {
        public static async void HttpPostAsync(string getUrl)
        {
            await HttpGet(getUrl);
        }
        public static async void HttpGetAsync(string getUrl)
        {
            await HttpGet(getUrl);
        }

        public static async Task<ResultModel> HttpGet(string getUrl)
        {
            ResultModel resultado = new ResultModel();
            try
            {
                var httpClient = new HttpClient();
                var httpResponse = await httpClient.GetAsync(getUrl);
                resultado.Success = httpResponse.IsSuccessStatusCode;
                resultado.Data = httpResponse.ReasonPhrase ?? "";
            }
            catch (Exception ex)
            {
                resultado.Success = false;
                resultado.Data = $"Ocurrió un error al enviar petición HttpGet a ({getUrl}).";
                resultado.Errors.Add($"{ex.Message}.");
                Console.Error.Write(ex.Message);
            }
            return (resultado);
        }
    }
}
