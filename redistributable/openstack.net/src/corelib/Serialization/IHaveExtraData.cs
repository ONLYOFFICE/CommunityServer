using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Identifies a resource as potentially containing more data than what is exposed via properties.
    /// </summary>
    /// <exclude />
    public interface IHaveExtraData
    {
        /// <summary>
        /// Contains any additional data returned from the cloud provider which has not already been mapped to a property.
        /// </summary>
        IDictionary<string, JToken> Data { get; set; }
    }

    /// <summary>
    /// Provides convenience methods for working with custom resource properties.
    /// </summary>
    /// <exclude />
    public static class IHaveExtraDataExtensions
    {
        /// <summary>
        /// Get a custom resource property.
        /// </summary>
        public static T GetExtraData<T>(this IHaveExtraData dataContainer, string key)
            where T : class
        {
            JToken jsonValue;
            if (dataContainer.Data.TryGetValue(key, out jsonValue))
                return jsonValue.Value<T>();
            
            return null;
        }

        /// <summary>
        /// Sets a custom resource property.
        /// </summary>
        public static void SetExtraData(this IHaveExtraData dataContainer, string key, object value)
        {
            dataContainer.Data[key] = value != null ? JToken.FromObject(value) : JValue.CreateNull();
        }
    }
}