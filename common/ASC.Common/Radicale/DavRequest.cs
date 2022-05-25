using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Common.Radicale
{
    public class DavRequest
    {
        public string Url { get; set; }
        public string Authorization { get; set; }
        public string Method { get; set; }
        public string Data { get; set; }
        public string Header { get; set; }
    }
}
