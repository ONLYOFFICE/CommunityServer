using System.Collections.Generic;

namespace OpenStack.Serialization
{
    /// <exclude />
    public interface IQueryStringBuilder
    {
        /// <summary />
        IDictionary<string, object> Build();
    }
}