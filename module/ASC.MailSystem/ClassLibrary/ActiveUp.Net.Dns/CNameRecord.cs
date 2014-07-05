using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Dns
{
    class CNameRecord : DomainNameOnly
    {
        /// <summary>
        /// Implementation Reference RFC 1035
        /// </summary>
        /// <param name="buffer"></param>
        public CNameRecord(DataBuffer buffer) : base(buffer){}
        /// <summary>
        /// Return domain name of record
        /// </summary>
        public new string Domain { get { return base.Domain; } }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return base.ToString(); }
    }
}
