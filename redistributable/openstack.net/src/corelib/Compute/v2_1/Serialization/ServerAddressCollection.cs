using System.Collections.Generic;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of server IP address resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "addresses")]
    public class ServerAddressCollection<T> : Dictionary<string, IList<T>>
    {}

    /// <inheritdoc cref="ServerAddressCollection{T}" />
    /// <exclude />
    public class ServerAddressCollection : ServerAddressCollection<ServerAddress>
    { }
}
