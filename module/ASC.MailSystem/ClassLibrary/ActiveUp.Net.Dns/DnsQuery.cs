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
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
#if !PocketPC
using System.Net.NetworkInformation;
#endif

namespace ActiveUp.Net.Dns
{
    public enum RecordType
    {     
        None = 0,      
        A = 1,
        NS = 2,
        CNAME = 5, 
        SOA = 6,
        MB = 7,
        MG = 8,
        MR = 9,
        NULL = 10,
        WKS = 11,
        PTR = 12,
        HINFO = 13,
        MINFO = 14,
        MX = 15,
        TXT = 16,
        RP = 17,
        AFSDB = 18,
        X25 = 19,
        ISDN = 20,
        RT = 21,
        NSAP = 22,
        SIG = 24,
        KEY = 25,
        PX = 26, 
        AAAA = 28,
        LOC = 29,
        SRV = 33,
        NAPTR = 35,
        KX = 36,
        A6 = 38,         
        DNAME = 39, 
        DS = 43,                  
        TKEY = 249, 
        TSIG = 250,
        All = 255 
    };

    /// <summary>
    /// As per RFC1035 Query Types for Dns Querys  types 3-16 are reserved for future use
    /// </summary>
    public enum OpCode
    {
        StandardQuery   = 0,
        InverseQuery    = 1,
        StatusRequest   = 2        
    }
    /// <summary>
    /// Exception raised during Dns Query
    /// </summary>
    public class DnsQueryException : Exception
    {
        public DnsQueryException(string msg, Exception[] exs) : base(msg) { exceptions = exs; }

        private Exception[] exceptions;
    }

    public class DnsQuery
    {
        const int DNS_PORT = 53;
        //const int MAX_TRIES = 3;
        const int MAX_TRIES = 1;
        const byte IN_CLASS = 1;

        /// <summary>
        /// Default constructor for a Dns Query
        /// </summary>
        public DnsQuery() { }
        /// <summary>
        /// Create a Dns Entry from a server Url
        /// </summary>
        /// <param name="serverUrl"></param>
        public DnsQuery(string serverUrl)
        {            
            IPHostEntry hostAddress = System.Net.Dns.GetHostEntry(serverUrl);
            if (hostAddress.AddressList.Length > 0)
                dnsServer = new IPEndPoint(hostAddress.AddressList[0], DNS_PORT);
            else
                throw new DnsQueryException("Invalid DNS Server Name Specified", null);
        }
        /// <summary>
        /// Create a Dns Entry from an IP Address
        /// </summary>
        /// <param name="dnsAddress"></param>
        public DnsQuery(IPAddress dnsAddress)
        {            
            dnsServer = new IPEndPoint(dnsAddress, DNS_PORT);
        }


#if PocketPC
        /// <summary>
        /// This functions Injects Sleep wherever required in PocketPC. This is required so that
        /// Less powerful MessagePump in PocketPC gets chance to execute other things.
        /// </summary>
        private void PPCSleep()
        {
            System.Threading.Thread.Sleep(1);
        }
#endif

        /// <summary>
        /// Quey a Dns Seerver for records of a specific type and timeout.
        /// </summary>
        /// <param name="recType"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public DnsAnswer QueryServer(RecordType recType, int timeout)
        {
            if (dnsServer == null)
                throw new DnsQueryException("There is no Dns server set in Dns Query Component", null);
            if (!ValidRecordType(recType))
                throw new DnsQueryException("Invalid Record Type submitted to Dns Query Component", null);

            //Result object
            DnsAnswer res = null;

            //UDP being unreliable we may need to try multiple times 
            //therefore we count how many times we have tried
            numTries = 0;

            //as per RFC 1035  there is a max size on the dns response
            byte[] dnsResponse = new byte[512];
            Exception[] exceptions = new Exception[MAX_TRIES];

            while (numTries < MAX_TRIES)
            {
                try
                {
                    CreateDnsQuery(recType);
                    reqSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if !PocketPC
                    reqSocket.ReceiveTimeout = timeout;
#else
                    PPCSleep();
#endif
                    reqSocket.SendTo(query, query.Length, SocketFlags.None, dnsServer);
//#if !PocketPC
//                  reqSocket.ReceiveTimeout = timeout;
//#else
//                  PPCSleep();
//#endif
                    reqSocket.Receive(dnsResponse);

                    if (dnsResponse[0] == query[0] && dnsResponse[1] == query[1])
                        //this is our response so format and return the answer to the query
                        res = new DnsAnswer(dnsResponse);
                    numTries++;
                    if (res.ReturnCode == ReturnCode.Success)
                        return res;
                }
                catch (SocketException ex)
                {
                    exceptions[numTries] = ex;
                    numTries++;
                    reqId++;
                    if (numTries > MAX_TRIES)
                        throw new DnsQueryException("Failure Querying DNS Server", exceptions);
                }
                finally
                {
                    reqId++;
                    reqSocket.Close();
                    Query = null; //Force a new message be built
                }
#if PocketPC
                //PocketPC CF .NET's Socket does require a latency to Connect.
                //.NET Sockets in 3.5 is improved, this will be taken out there.
                PPCSleep();
#endif
            }
            return res;
        }

        /// <summary>
        /// Quey a Dns Seerver for records of a specific type
        /// </summary>
        /// <param name="recType"></param>
        /// <returns></returns>
        public DnsAnswer QueryServer(RecordType recType)
        {
            return this.QueryServer(recType, 5000);
        }

        /// <summary>
        /// Create a byte array that conforms to a DNS Query Format
        /// </summary>
        /// <param name="recType"></param>
        private void CreateDnsQuery(RecordType recType)
        {
            List<Byte> queryBytes = new List<byte>();            
            queryBytes.Add((byte)(reqId >> 8));
            queryBytes.Add((byte)(reqId));
            
            //populate bit fields
            queryBytes.Add((byte)(((byte)OpCode.StandardQuery << 3) | ( RecursiveQuery ? 0x01 : 0x00)));
            queryBytes.Add(0x00);
            
            //set number of questions  we will always use to 1
            queryBytes.Add(0x00);
            queryBytes.Add(0x01);
            
            //no requests, no name servers, no additional records in standard request
            for (int i = 0; i < 6; i++)
                queryBytes.Add(0x00);

            InsertDomainName(queryBytes, domain);
            //query Type
            queryBytes.Add(0x00);
            queryBytes.Add((byte)recType);
            //query class
            queryBytes.Add(0x00);
            queryBytes.Add(IN_CLASS);
            Query = queryBytes.ToArray();
        }
        /// <summary>
        /// Insert domain name into byte array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="domain"></param>
        private void InsertDomainName(List<Byte> data, string domain)
        {
            //Write each segment of the domain name to the array
            //Each segment is seperated by a '.' token and each segment is 
            //preceded by the couint of characters in the segment.
            int length = 0;
            int pos = 0;
            while (pos < domain.Length)
            {
                int prev_pos = pos;
                pos = domain.IndexOf('.', pos);
                length = pos - prev_pos;
                if (length < 0) //last segment
                    length = domain.Length - prev_pos;

                //Add segment data to array
                data.Add((byte)length);
                for(int i = 0; i < length; i++)
                    data.Add((byte)domain[prev_pos++]);

                //step past the '.'
                pos = prev_pos;
                pos++;
                //pos++;
            }
            //Terminate with a zero
            data.Add(0x00);
        }
        /// <summary>
        /// Is this a valid DNS Record type
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        private bool ValidRecordType(RecordType t)
        {
            return (Enum.IsDefined(typeof(RecordType), t) || t == RecordType.All);           
        }

        private byte[] query;
        //Access the complete query
        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public byte[] Query
        {
            get { return query; }
            set { query = value; }
        }

        private string domain;
        //Access the domain name
        public string Domain
        {
            get { return domain; }
            set 
                {
                    if (value.Length == 0 || value.Length > 255 ||
                        !Regex.IsMatch(value, @"^[a-z|A-Z|0-9|-|_]{1,63}(\.[a-z|A-Z|0-9|\-]{1,63})+$"))
                        throw new DnsQueryException("Invalid Domain Name", null);
                    domain = value; 
                }
        }

        private IPEndPoint dnsServer;
        /// <summary>
        /// Gets or sets the DNS server.
        /// </summary>
        /// <value>The DNS server.</value>
        public IPEndPoint DnsServer
        {
            get { return dnsServer; }
            set { dnsServer = value; }
        }
        private bool recursiveQuery = true;
        //Reuest a recursive query
        public bool RecursiveQuery
        {
            get { return recursiveQuery; }
            set { recursiveQuery = value; }
        }

        private Socket reqSocket;        
        private int numTries;
        private int reqId;

        public static List<IPAddress> GetMachineDnsServers()
        {
            List<IPAddress> dnsServers = new List<IPAddress>();
            //IPAddressCollection dnsServers = null;

#if !PocketPC
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                
                    foreach (IPAddress ipAddress in properties.DnsAddresses)
                    {
                        dnsServers.Add(ipAddress);
                    }
                
            }
#endif

            return dnsServers;
        }
    }
}
