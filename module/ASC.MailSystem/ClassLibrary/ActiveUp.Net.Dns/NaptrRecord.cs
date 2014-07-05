// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveUp.Net.Dns
{
    class NaptrRecord : IRecordData
    {
        /// <summary>
        /// Implementation Reference RFC 3403
        /// </summary>
        /// <param name="buffer"></param>
         public NaptrRecord(DataBuffer buffer)
        {
            order = buffer.ReadShortUInt();
            priority = buffer.ReadShortUInt();
            flags = buffer.ReadCharString();
            services = buffer.ReadCharString();
            regexp = buffer.ReadCharString();
            replacement = buffer.ReadCharString();
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Order:{0}, Priority:{1} Flags:{2} Services:{3} RegExp:{4} Replacement:{5}",
                order, priority, flags, services, regexp, replacement);
        }

        ushort order;
        /// <summary>
        /// retiurn Order of record
        /// </summary>
        public ushort Order
        {
            get { return order; }
        }
        ushort priority;
        /// <summary>
        /// return Priority of record
        /// </summary>
        public ushort Priority
        {
            get { return priority; }
        }
        string flags;
        /// <summary>
        /// return flags of record
        /// </summary>
        public string Flags
        {
            get { return flags; }
        }
        string services;
        /// <summary>
        /// return services listed in record
        /// </summary>
        public string Services
        {
            get { return services; }
        }
        string regexp;
        /// <summary>
        /// return regexp of record
        /// </summary>
        public string Regexp
        {
            get { return regexp; }
        }
        string replacement;
        /// <summary>
        /// return replacement domain of record
        /// </summary>
        public string Replacement
        {
            get { return replacement; }
        }
    }
}
