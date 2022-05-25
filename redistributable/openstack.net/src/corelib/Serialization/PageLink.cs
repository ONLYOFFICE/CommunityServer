using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenStack.Serialization
{
    /// <exclude />
    public class PageLink : IPageLink, IHaveExtraData
    {
        /// <summary />
        [JsonConstructor]
        protected PageLink()
        { }

        /// <summary />
        public PageLink(string relationship, string url)
        {
            Relationship = relationship;
            Url = url;
        }

        /// <summary />
        [JsonProperty("href")]
        public string Url { get; private set; }

        /// <summary />
        [JsonIgnore]
        public bool IsNextPage => Relationship == "next";

        /// <summary />
        [JsonProperty("rel")]
        public string Relationship { get; private set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}