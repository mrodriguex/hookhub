using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace HookHub.Web
{
    public static class Config
    {
        private static IConfiguration _configuration;
        private static string _contentRootPath;

        public static IConfiguration Configuration
        {
            get
            {
                if (_configuration is null)
                {
                    throw new Exception("La propiedad Configuration no ha sido inicializada en la clase estática Config.cs");
                }
                return (_configuration);
            }
            private set
            {
                _configuration = value;
            }
        }

        private static string ContentRootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_contentRootPath))
                {
                    _contentRootPath = "";
                }
                return (_contentRootPath);
            }
            set
            {
                _contentRootPath = value;
                Configuration = ConfigurationHelper.ResolveConfiguration(_contentRootPath);
            }
        }

        public static string URL(string connectionString)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("URLs").GetSection(connectionString).Value;
        }
        public static string HookNames(string claveUsuarioTipo)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("HookNames").GetSection(claveUsuarioTipo).Value;
        }

        public static string GetConnectionString(string connectionString)
        {
            //ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            //string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            //configurationBuilder.AddJsonFile(path, false);

            //var root = configurationBuilder.Build();
            return Configuration.GetSection("ConnectionStrings")[connectionString];
        }

        public static void Configure(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }
    }
}
