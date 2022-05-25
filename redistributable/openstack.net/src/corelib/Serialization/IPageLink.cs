namespace OpenStack.Serialization
{
    /// <exclude />
    public interface IPageLink
    {
        /// <summary />
        string Url { get; }

        /// <summary />
        bool IsNextPage { get; }
    }
}