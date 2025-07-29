using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace HookHub.Core.Models
{
   public static class HookConnetionsHub
    {

        private static ConcurrentDictionary<string, string> _hookConnections;

        public static ConcurrentDictionary<string, string> HookConnections {
            get {
                if (_hookConnections is null) { _hookConnections = new ConcurrentDictionary<string, string>(); }
                return (_hookConnections);
            }
            set {
                _hookConnections = value;
            }
        }

    }
}
