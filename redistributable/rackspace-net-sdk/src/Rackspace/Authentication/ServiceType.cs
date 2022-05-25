using System;

namespace Rackspace
{
    internal class ServiceType : OpenStack.Authentication.IServiceType
    {
        protected ServiceType(string type)
        {
            Type = type;
        }

        public string Type { get; private set; }

        #region Equality        

        /// <summary>
        /// Determines if this instance is equal to the specified instance.
        /// </summary>
        /// <param name="other">The other instance to which this instance should be compared.</param>
        /// <returns>If the two instances are equal.</returns>
        protected bool Equals(ServiceType other)
        {
            return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as ServiceType;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(ServiceType left, ServiceType right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(ServiceType left, ServiceType right)
        {
            return !Equals(left, right);
        }

        #endregion

        public static readonly ServiceType RackspaceCDN = new ServiceType("rax:cdn");
        public static readonly ServiceType CloudNetworks = new ServiceType("network");
        public static readonly ServiceType RackConnect = new ServiceType("rax:rackconnect");
    }
}
