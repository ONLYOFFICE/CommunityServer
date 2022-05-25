using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl;

namespace OpenStack.Serialization
{
    /// <exclude />
    public interface IPageBuilder<TPage>
    {
        /// <summary>
        /// Sets the next page handler.
        /// </summary>
        /// <param name="value">The handler.</param>
        void SetNextPageHandler(Func<Url, CancellationToken, Task<TPage>> value);
    }
}