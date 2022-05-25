using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Reference to a security group.
    /// </summary>
    public class SecurityGroupReference : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The security group name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        private async Task<SecurityGroup> LoadSecurityGroup(CancellationToken cancellationToken)
        {
            var securityGroup = this as SecurityGroup;
            if (securityGroup != null)
                return securityGroup;

            var owner = this.GetOwnerOrThrow<ComputeApi>();

            // In some cases, such as when working with the groups on a server, we only have the name and not the id
            var groups = await owner.ListSecurityGroupsAsync<SecurityGroupCollection>(cancellationToken: cancellationToken).ConfigureAwait(false);
            securityGroup = groups.FirstOrDefault(x => x.Name == Name);
            if(securityGroup == null)
                throw new Exception($"Unable to find the security group named: {Name}.");

            return securityGroup;
        }

        /// <inheritdoc cref="ComputeApi.GetSecurityGroupAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<SecurityGroup> GetSecurityGroupAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return LoadSecurityGroup(cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DeleteSecurityGroupAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var securityGroup = await LoadSecurityGroup(cancellationToken).ConfigureAwait(false);

            await owner.DeleteSecurityGroupAsync(securityGroup.Id, cancellationToken).ConfigureAwait(false);
        }
    }
}