using OpenStack.Serialization;

namespace OpenStack.Compute.v2_2.Serialization
{
    /// <summary />
    /// <exclude />
    public class ServerCollection<TPage, TItem> : v2_1.Serialization.ServerCollection<TPage, TItem>
        where TPage : ServerCollection<TPage, TItem>
        where TItem : IServiceResource
    { }

    /// <summary />
    /// <exclude />
    public class ServerCollection : ServerCollection<ServerCollection, ServerReference>
    { }
}