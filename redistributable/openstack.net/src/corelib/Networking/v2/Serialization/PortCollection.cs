using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of port resources returned by the <see cref="NetworkingService"/>.
    /// <para>Intended for custom implementations and stubbing responses in unit tests.</para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <exclude />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "ports")]
    public class PortCollection : List<Port>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortCollection"/> class.
        /// </summary>
        public PortCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortCollection"/> class.
        /// </summary>
        /// <param name="ports">The networks.</param>
        public PortCollection(IEnumerable<Port> ports) : base(ports)
        {
        }
    }
}