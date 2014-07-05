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
    public class DnsEntry
    {
        /// <summary>
        /// Create a DNSEntry record and dispatch to data constructor 
        /// for thast record type
        /// </summary>
        /// <param name="buffer"></param>
        public DnsEntry(DataBuffer buffer)
        {
            try
            {
                domain = buffer.ReadDomainName();
                byte b = buffer.ReadByte();
                recType = (RecordType)buffer.ReadShortInt();
                classType = buffer.ReadShortInt();
                ttl = buffer.ReadInt();

                int length = buffer.ReadByte();
                switch (recType)
                {
                    case RecordType.A: data = new ARecord(buffer); break;
                    case RecordType.NS: data = new NSRecord(buffer); break;
                    case RecordType.CNAME: data = new CNameRecord(buffer); break;
                    case RecordType.SOA: data = new SoaRecord(buffer); break;
                    case RecordType.MB: data = new MBRecord(buffer); break;
                    case RecordType.MG: data = new MGRecord(buffer); break;
                    case RecordType.MR: data = new MRRecord(buffer); break;
                    case RecordType.NULL: data = new NullRecord(buffer, length); break;
                    case RecordType.WKS: data = new WksRecord(buffer, length); break;
                    case RecordType.PTR: data = new PtrRecord(buffer); break;
                    case RecordType.HINFO: data = new HInfoRecord(buffer, length); break;
                    case RecordType.MINFO: data = new MInfoRecord(buffer); break;
                    case RecordType.MX: data = new MXRecord(buffer); break;
                    case RecordType.TXT: data = new TxtRecord(buffer, length); break;
                    case RecordType.RP: data = new RPRecord(buffer); break;
                    case RecordType.AFSDB: data = new AfsdbRecord(buffer); break;
                    case RecordType.X25: data = new X25Record(buffer); break;
                    case RecordType.ISDN: data = new IsdnRecord(buffer); break;
                    case RecordType.RT: data = new RTRecord(buffer); break;
                    case RecordType.NSAP: data = new NsapRecord(buffer, length); break;
                    case RecordType.SIG: data = new SigRecord(buffer, length); break;
                    case RecordType.KEY: data = new KeyRecord(buffer, length); break;
                    case RecordType.PX: data = new PXRecord(buffer); break;
                    case RecordType.AAAA: data = new AAAARecord(buffer); break;
                    case RecordType.LOC: data = new LocRecord(buffer); break;
                    case RecordType.SRV: data = new SrvRecord(buffer); break;
                    case RecordType.NAPTR: data = new NaptrRecord(buffer); break;
                    case RecordType.KX: data = new KXRecord(buffer); break;
                    case RecordType.A6: data = new A6Record(buffer); break;
                    case RecordType.DNAME: data = new DNameRecord(buffer); break;
                    case RecordType.DS: data = new DSRecord(buffer, length); break;
                    case RecordType.TKEY: data = new TKeyRecord(buffer); break;
                    case RecordType.TSIG: data = new TSigRecord(buffer); break;
                    default: throw new DnsQueryException("Invalid DNS Record Type in DNS Response", null);
                }
            }
            catch (Exception ex)
            {
                data = new ExceptionRecord(ex.Message);
                throw ex;
            }
           
        }

        private string domain;
        /// <summary>
        /// Return Domain name of record
        /// </summary>
        public string Domain            {   get { return domain; }  }           
        private RecordType recType;
        /// <summary>
        /// return Record Type of record
        /// </summary>
        public RecordType RecType       {   get { return recType; } }        
        private int classType;
        /// <summary>
        /// return class type of record
        /// </summary>
        public int ClassType            {   get { return classType; }}        
        private int ttl;
        /// <summary>
        /// return Time To Live for record
        /// </summary>
        public int Ttl                  {   get { return ttl; }     }        
        private IRecordData data;
        public IRecordData Data         { get { return data; } }
    }
}
