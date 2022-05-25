using Newtonsoft.Json;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// Represents a range of IP addresses
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class AllocationPool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationPool" /> class.
        /// </summary>
        /// <param name="start">The initial IP address.</param>
        /// <param name="end">The final IP address.</param>
        public AllocationPool(string start, string end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// The initial IP address.
        /// </summary>
        [JsonProperty("start")]
        public string Start { get; set; }

        /// <summary>
        /// The final IP address.
        /// </summary>
        [JsonProperty("end")]
        public string End { get; set; }

        #region Equality
        private bool Equals(AllocationPool other)
        {
            return string.Equals(Start, other.Start) && string.Equals(End, other.End);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as AllocationPool;
            return other != null && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode()*397) ^ End.GetHashCode();
            }
        }
        #endregion
    }
}