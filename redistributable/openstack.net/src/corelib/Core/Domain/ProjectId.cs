namespace net.openstack.Core.Domain
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a project, <see cref="Tenant"/>, or account.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ProjectId.Converter))]
    public sealed class ProjectId : ResourceIdentifier<ProjectId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The project identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public ProjectId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ProjectId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ProjectId FromValue(string id)
            {
                return new ProjectId(id);
            }
        }
    }
}
