using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Common.Radicale
{
    [DataContract]
    public class DavResponse
    {
        [DataMember]
        public bool Completed { get; set; }
        [DataMember]
        public int StatusCode { get; set; }
        [DataMember]
        public string Data { get; set; }
        [DataMember]
        public string Error { get; set; }

        public override string ToString()
        {
            return StatusCode.ToString() + " " + Error;
        }

    }
}
