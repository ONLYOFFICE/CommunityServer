using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A record of an action applied to a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "instanceAction")]
    public class ServerAction : ServerActionSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerAction"/> class.
        /// </summary>
        public ServerAction()
        {
            Events = new List<ServerEvent>();
        }

        /// <summary />
        [JsonProperty("events")]
        public IList<ServerEvent> Events { get; set; } 
    }

    /// <summary>
    /// A record of an event triggered during a server action.
    /// </summary>
    /// <seealso cref="OpenStack.Serialization.IHaveExtraData" />
    public class ServerEvent : IHaveExtraData
    {
        /// <summary>
        /// The event name.
        /// </summary>
        [JsonProperty("event")]
        public string Name { get; set; }

        /// <summary>
        /// The resulting event status.
        /// </summary>
        [JsonProperty("result")]
        public ServerEventStatus Result { get; set; }

        /// <summary>
        /// The event start time.
        /// </summary>
        [JsonProperty("start_time")]
        public DateTimeOffset Started { get; set; }

        /// <summary>
        /// The event completion time.
        /// </summary>
        [JsonProperty("finish_time")]
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Additional error information, if any.
        /// </summary>
        [JsonProperty("traceback")]
        public string StackTrace { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}