//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace HookHub.Hub
//{
//    public static class Config
//    {

//        public static string URL(string connectionString)
//        {
//            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
//            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
//            configurationBuilder.AddJsonFile(path, false);

//            var root = configurationBuilder.Build();
//            return root.GetSection("URLs").GetSection(connectionString).Value;
//        }

//        public static string HookNames(string claveUsuarioTipo)
//        {
//            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
//            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
//            configurationBuilder.AddJsonFile(path, false);

//            var root = configurationBuilder.Build();
//            return root.GetSection("HookNames").GetSection(claveUsuarioTipo).Value;
//        }
//        public static string ConnectionStrings(string connectionString)
//        {
//            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
//            string path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
//            configurationBuilder.AddJsonFile(path, false);

//            var root = configurationBuilder.Build();
//            return root.GetSection("ConnectionStrings").GetSection(connectionString).Value;
//        }

//    }
//}
