using System.Collections.Generic;
using OpenStack.Networking.v2.Layer3;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of security group rule resources returned by the <see cref="NetworkingService"/>.
    /// <para>Intended for custom implementations and stubbing responses in unit tests.</para>
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group_rules")]
    public class SecurityGroupRuleCollection : List<SecurityGroupRule>
    {

        /// <summary>
        /// Initializes a new instance of the<see cref="SecurityGroupRuleCollection"/> class.
        /// </summary>
        public SecurityGroupRuleCollection()
        {

        }

        /// <summary>
        /// Initializes a new instance of the<see cref="SecurityGroupRuleCollection"/> class.
        /// </summary>
        /// <param name="items"></param>
        public SecurityGroupRuleCollection(IEnumerable<SecurityGroupRule> items) : base(items)
        {

        }
    }
}