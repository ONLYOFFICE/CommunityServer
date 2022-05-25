using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Represents a collection of resources.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <exclude />
    [JsonObject(MemberSerialization.OptIn)] // Using JsonObject to force the entire object to be serialized, ignoring the IEnumerable interface
    public class ResourceCollection<T> : IEnumerable<T>, IHaveExtraData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCollection{T}"/> class.
        /// </summary>
        public ResourceCollection()
        {
            Items = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCollection{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ResourceCollection(IEnumerable<T> items)
        {
            Items = items.ToNonNullList();
        }

        /// <summary>
        /// The requested items.
        /// </summary>
        public IList<T> Items { get; set; }

        /// <summary>
        /// Adds an item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            Items.Add(item);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}