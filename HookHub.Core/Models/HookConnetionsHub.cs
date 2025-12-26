using System.Collections.Concurrent;

namespace HookHub.Core.Models
{
   public static class HookConnetionsHub
    {

        private static ConcurrentDictionary<string, string?>? _hookConnections;

        public static ConcurrentDictionary<string, string?> HookConnections {
            get {
                _hookConnections ??= new ConcurrentDictionary<string, string?>();
                return (_hookConnections);
            }
            set {
                _hookConnections = value;
            }
        }

    }
}
