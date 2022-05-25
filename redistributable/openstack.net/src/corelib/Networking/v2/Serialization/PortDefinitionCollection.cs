using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of port definition resources of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <exclude />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "ports")]
    internal class PortDefinitionCollection : List<object>
    {
        public PortDefinitionCollection(IEnumerable<object> ports) : base(ports)
        {
        }
    }
}