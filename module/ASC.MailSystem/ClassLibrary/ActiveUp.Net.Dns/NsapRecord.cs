using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Dns
{
    class NsapRecord : IRecordData
    {
        /// <summary>
        /// Implementation Rference RFC 1706
        /// RFC is unclear and self-contradictory record type not implemented
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
         public NsapRecord(DataBuffer buffer, int length)
        {
            buffer.Position += length;
            throw new NotImplementedException("Experimental Record Type Unable to Implement");
        }
    }
}
