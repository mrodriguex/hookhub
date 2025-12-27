using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HookHub.Core.ContractResolver
{
    /// <summary>
    /// A custom JSON contract resolver that ignores specific properties during serialization to prevent errors.
    /// </summary>
    public class IgnoreErrorPropertiesResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a JSON property for the given member, marking certain properties as ignored.
        /// </summary>
        /// <param name="member">The member information.</param>
        /// <param name="memberSerialization">The member serialization settings.</param>
        /// <returns>The configured JSON property.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (new string[]{"InputStream",
                "Filter",
                "Length",
                "Position",
                "ReadTimeout",
                "WriteTimeout",
                "LastActivityDate",
                "LastUpdatedDate",
                "Session"
            }.Contains(property.PropertyName))
            {
                property.Ignored = true;
            }
            return property;
        }
    }
}
