using Microsoft.Extensions.Configuration;
using System.IO;

namespace HookHub.Core
{
    public static class Config
    {

        public static string URL(string connectionString)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("URLs").GetSection(connectionString).Value;
        }

        internal static string TimeIntervals(string timeIntervals)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("TimeIntervals").GetSection(timeIntervals).Value;
        }

        public static string HookNames(string claveUsuarioTipo)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("HookNames").GetSection(claveUsuarioTipo).Value;
        }
        public static string ConnectionStrings(string connectionString)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            var root = configurationBuilder.Build();
            return root.GetSection("ConnectionStrings").GetSection(connectionString).Value;
        }

    }
}
