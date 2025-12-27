using System.Collections.Concurrent;

namespace HookHub.Core.Models
{
   /// <summary>
   /// Static registry for managing hook connections in a thread-safe manner.
   /// Maps connection IDs to hook names.
   /// </summary>
   public static class HookConnetionsHub
    {

        private static ConcurrentDictionary<string, string?>? _hookConnections;

        /// <summary>
        /// Gets or sets the concurrent dictionary of hook connections (connectionId -> hookName).
        /// </summary>
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
