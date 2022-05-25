using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace net.openstack.Core.Domain
{
    /// <summary>
    /// Represents metadata for servers and images in the Compute Service.
    /// </summary>
    /// <remarks>
    /// The metadata keys for the compute provider are case-sensitive.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonDictionary]
    [Serializable]
    public class Metadata : Dictionary<string, string>
    {
    }
}
