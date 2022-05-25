using System;
using System.Collections.Generic;
using System.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Security groups are sets of IP filter rules that are applied to an instance's networking.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/openstack-ops/content/security_groups.html"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "security_group")]
    public class SecurityGroup : SecurityGroupReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGroup"/> class.
        /// </summary>
        public SecurityGroup()
        {
            Rules = new List<SecurityGroupRule>();
        }

        /// <summary>
        /// The security group identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The security group description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The security group rules.
        /// </summary>
        [JsonProperty("rules")]
        public IList<SecurityGroupRule> Rules { get; set; }

        /// <inheritdoc cref="ComputeApi.UpdateSecurityGroupAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            var request = new SecurityGroupDefinition(Name, Description);

            var result = await compute.UpdateSecurityGroupAsync<SecurityGroup>(Id, request, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.CreateSecurityGroupRuleAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<SecurityGroupRule> AddRuleAsync(SecurityGroupRuleDefinition rule, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            rule.GroupId = Id;

            var result = await compute.CreateSecurityGroupRuleAsync<SecurityGroupRule>(rule, cancellationToken).ConfigureAwait(false);
            Rules.Add(result);
            return result;
        }
    }
}