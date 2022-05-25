using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of network definition resources of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <exclude />
    [JsonConverterWithConstructor(typeof (RootWrapperConverter), "networks")]
    internal class NetworkDefinitionCollection : List<object>
    {
        public NetworkDefinitionCollection(IEnumerable<object> networks) : base(networks)
        {
        }
    }
}