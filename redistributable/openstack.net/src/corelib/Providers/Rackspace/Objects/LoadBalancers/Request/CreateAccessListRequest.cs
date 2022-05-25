namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateAccessListRequest
    {
        [JsonProperty("accessList")]
        private NetworkItem[] _accessList;

        public CreateAccessListRequest(IEnumerable<NetworkItem> accessList)
        {
            if (accessList == null)
                throw new ArgumentNullException("accessList");

            _accessList = accessList.ToArray();
        }
    }
}
