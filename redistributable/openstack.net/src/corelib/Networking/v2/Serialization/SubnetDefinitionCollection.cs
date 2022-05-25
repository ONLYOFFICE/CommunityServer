using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Represents a collection of subnet definition resources of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <exclude />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "subnets")]
    internal class SubnetDefinitionCollection : List<object>
    {
        public SubnetDefinitionCollection(IEnumerable<object> subnets) : base(subnets)
        {
        }
    }
}