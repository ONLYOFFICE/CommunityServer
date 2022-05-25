using System.Collections.Generic;
using OpenStack.Serialization;
using OpenStack.Networking.v2.Layer3;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of security groups resources returned by the <see cref="NetworkingService"/>.
    /// <para>Intended for custom implementations and stubbing responses in unit tests.</para>
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_groups")]
    public class SecurityGroupCollection : List<SecurityGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGroupCollection"/> class.
        /// </summary>
        public SecurityGroupCollection()
        {

        }

        /// <summary>
        /// Initializes a new instance of the<see cref="SecurityGroupCollection"/> class.
        /// </summary>
        /// <param name="items">items</param>
        public SecurityGroupCollection(IEnumerable<SecurityGroup> items)
            : base(items)
        {

        }
    }
}
