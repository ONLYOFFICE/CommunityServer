using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Resources which want to expose service operations directly (e.g. resource.Delete())
    /// should implement this interface and the service will use it to add a reference to itself.
    /// </summary>
    /// <exclude />
    public interface IServiceResource
    {
        /// <summary>
        /// The service which originally constructed the resource. This instance will be used for further operations on the resource.
        /// </summary>
        object Owner { get; set; }
    }

    /// <summary>
    /// Resources which are children of another resource.
    /// </summary>
    /// <exclude />
    public interface IChildResource : IServiceResource
    {
        /// <summary>
        /// Called after deserialization to bootstrap a link from the child back to the parent resource.
        /// </summary>
        void SetParent(object parent);

        /// <summary>
        /// Called after deserialization to bootstrap a link from the child back to the parent resource.
        /// </summary>
        void SetParent(string parentId);
    }

    /// <exclude />
    public static class ServiceResourceExtensions
    {
        /// <summary />
        public static async Task<T> PropogateOwner<T>(this Task<T> task, object owner)
            where T : IServiceResource
        {
            T resource = await task.ConfigureAwait(false);
            return PropogateOwner(resource, owner);
        }

        /// <summary />
        public static T PropogateOwner<T>(this T resource, object owner)
            where T : IServiceResource
        {
            if (resource == null)
                return default(T);

            resource.Owner = owner;

            foreach (PropertyInfo prop in resource.GetType().GetProperties())
            {
                object propVal;
                try
                {
                    propVal = prop.GetValue(resource);
                }
                catch
                {
                    continue;
                }

                (propVal as IServiceResource)?.PropogateOwner(owner);
                (propVal as IEnumerable<IServiceResource>)?.PropogateOwnerToChildren(owner);
            }

            return resource;
        }

        /// <summary />
        public static async Task<T> PropogateOwnerToChildren<T>(this Task<T> task, object owner)
            where T : IEnumerable<IServiceResource>
        {
            T resources = await task.ConfigureAwait(false);
            return resources.PropogateOwnerToChildren(owner);
        }

        /// <summary />
        public static T PropogateOwnerToChildren<T>(this T resources, object owner)
            where T : IEnumerable<IServiceResource>
        {
            foreach (var resource in resources)
            {
                resource.PropogateOwner(owner);
            }
            return resources;
        }

        /// <summary />
        /// <exception cref="InvalidOperationException">Thrown when a resource as not constructed by the SDK.</exception>
        public static T GetOwnerOrThrow<T>(this IServiceResource resource, [CallerMemberName] string callerName = "")
            where T : class
        {
            var owner = resource.Owner as T;
            if (owner != null)
                return owner;

            var ownerName = typeof(T).Name;
            throw new InvalidOperationException(string.Format($"{callerName} can only be used on instances which were constructed by {ownerName}. Use {ownerName}.{callerName} instead."));
        }

        /// <summary />
        public static async Task<T> SetParent<T>(this Task<T> task, string parentId)
            where T : IChildResource
        {
            var resource = await task.ConfigureAwait(false);
            resource.SetParent(parentId);
            return resource;
        }

        /// <summary />
        public static async Task<T> SetParentOnChildren<T>(this Task<T> task, string parentId)
            where T : IEnumerable<IChildResource>
        {
            var resources = await task.ConfigureAwait(false);
            return SetParentOnChildren(resources, parentId);
        }

        /// <summary />
        public static T SetParentOnChildren<T>(this T resources, string parentId)
            where T : IEnumerable<IChildResource>
        {
            foreach (var resource in resources)
            {
                resource.SetParent(parentId);
            }

            return resources;
        }
    }
}