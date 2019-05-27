using System;
using System.Runtime.Serialization;

namespace ASC.ElasticSearch.Core
{
    [DataContract]
    public class State
    {
        [DataMember]
        public string Indexing { get; set; }

        [DataMember]
        public DateTime? LastIndexed { get; set; }
    }
}
