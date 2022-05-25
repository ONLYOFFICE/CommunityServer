namespace Rackspace.RackConnect.v3
{
    /// <summary>
    /// Defines filters which can be applied when listing Public IPs.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ListPublicIPsFilter
    {
        /// <summary>
        /// Filter the results to only those associated with the specified server.
        /// </summary>
        public string ServerId { get; set; }

        /// <summary>
        /// Filter the results based on their <see cref="PublicIP.ShouldRetain"/> setting.
        /// </summary>
        public bool? IsRetained { get; set; }
    }
}