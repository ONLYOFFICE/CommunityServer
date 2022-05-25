using System.Threading.Tasks;

namespace OpenStack.Synchronous.Extensions
{
    /// <summary>
    /// Extensions to <see cref="Task"/>.
    /// </summary>
    /// <exclude />
    public static class TaskExtensions
    {
        /// <summary>
        /// Calls an asynchronous method synchronously. Hopefully without causing deadlocks or mucking up any thrown exceptions.
        /// </summary>
        public static T ForceSynchronous<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calls an asynchronous method synchronously. Hopefully without causing deadlocks or mucking up any thrown exceptions.
        /// </summary>
        public static void ForceSynchronous(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}