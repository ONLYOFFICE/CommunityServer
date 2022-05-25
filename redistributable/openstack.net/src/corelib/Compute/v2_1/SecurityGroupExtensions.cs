using OpenStack.Compute.v2_1;
using OpenStack.Synchronous.Extensions;

// ReSharper disable once CheckNamespace
namespace OpenStack.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for a <see cref="SecurityGroup"/> instance.
    /// </summary>
    public static class SecurityGroupExtensions_v2_1
    {
        /// <inheritdoc cref="SecurityGroup.AddRuleAsync"/>
        public static SecurityGroupRule AddRule(this SecurityGroup securityGroup, SecurityGroupRuleDefinition rule)
        {
            return securityGroup.AddRuleAsync(rule).ForceSynchronous();
        }

        /// <inheritdoc cref="SecurityGroupReference.DeleteAsync"/>
        public static void Delete(this SecurityGroupReference securityGroup)
        {
            securityGroup.DeleteAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="SecurityGroupRule.DeleteAsync"/>
        public static void Delete(this SecurityGroupRule rule)
        {
            rule.DeleteAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="SecurityGroupReference.GetSecurityGroupAsync"/>
        public static SecurityGroup GetSecurityGroup(this SecurityGroupReference securityGroup)
        {
            return securityGroup.GetSecurityGroupAsync().ForceSynchronous();
        }

        /// <inheritdoc cref="SecurityGroupReference.DeleteAsync"/>
        public static void Update(this SecurityGroup securityGroup)
        {
            securityGroup.UpdateAsync().ForceSynchronous();
        }
    }
}
