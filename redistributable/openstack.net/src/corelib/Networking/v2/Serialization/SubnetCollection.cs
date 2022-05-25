using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of subnet resources returned by the <see cref="NetworkingService"/>.
    /// <para>Intended for custom implementations and stubbing responses in unit tests.</para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <exclude />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "subnets")]
    public class SubnetCollection : List<Subnet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubnetCollection"/> class.
        /// </summary>
        public SubnetCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubnetCollection"/> class.
        /// </summary>
        /// <param name="subnets">The networks.</param>
        public SubnetCollection(IEnumerable<Subnet> subnets) : base(subnets)
        {
        }
    }
}