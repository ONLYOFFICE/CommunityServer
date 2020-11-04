/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net.Dns.Client
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;

    #endregion

    /// <summary>
    /// Dns client.
    /// </summary>
    /// <example>
    /// <code>
    /// // Set dns servers
    /// Dns_Client.DnsServers = new string[]{"194.126.115.18"};
    /// 
    /// Dns_Client dns = Dns_Client();
    /// 
    /// // Get MX records.
    /// DnsServerResponse resp = dns.Query("lumisoft.ee",QTYPE.MX);
    /// if(resp.ConnectionOk &amp;&amp; resp.ResponseCode == RCODE.NO_ERROR){
    ///		MX_Record[] mxRecords = resp.GetMXRecords();
    ///		
    ///		// Do your stuff
    ///	}
    ///	else{
    ///		// Handle error there, for more exact error info see RCODE 
    ///	}	 
    /// 
    /// </code>
    /// </example>
    public class Dns_Client
    {
        #region Members

        private static IPAddress[] m_DnsServers;
        private static int m_ID = 100;
        private static bool m_UseDnsCache = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Dns_Client()
        {
            // Try to get system dns servers
            try
            {
                List<IPAddress> dnsServers = new List<IPAddress>();
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (IPAddress ip in nic.GetIPProperties().DnsAddresses)
                        {
                            if (ip.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (!dnsServers.Contains(ip))
                                {
                                    dnsServers.Add(ip);
                                }
                            }
                        }
                    }
                }

                m_DnsServers = dnsServers.ToArray();
            }
            catch {}
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets dns servers.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is raised when null value is passed.</exception>
        public static string[] DnsServers
        {
            get
            {
                string[] retVal = new string[m_DnsServers.Length];
                for (int i = 0; i < m_DnsServers.Length; i++)
                {
                    retVal[i] = m_DnsServers[i].ToString();
                }

                return retVal;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                IPAddress[] retVal = new IPAddress[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    retVal[i] = IPAddress.Parse(value[i]);
                }

                m_DnsServers = retVal;
            }
        }

        /// <summary>
        /// Gets or sets if to use dns caching.
        /// </summary>
        public static bool UseDnsCache
        {
            get { return m_UseDnsCache; }

            set { m_UseDnsCache = value; }
        }

        /// <summary>
        /// Get next query ID.
        /// </summary>
        internal static int ID
        {
            get
            {
                if (m_ID >= 65535)
                {
                    m_ID = 100;
                }
                return m_ID++;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resolves host names to IP addresses.
        /// </summary>
        /// <param name="hosts">Host names to resolve.</param>
        /// <returns>Returns specified hosts IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>hosts</b> is null.</exception>
        public static IPAddress[] Resolve(string[] hosts)
        {
            if (hosts == null)
            {
                throw new ArgumentNullException("hosts");
            }

            List<IPAddress> retVal = new List<IPAddress>();
            foreach (string host in hosts)
            {
                IPAddress[] addresses = Resolve(host);
                foreach (IPAddress ip in addresses)
                {
                    if (!retVal.Contains(ip))
                    {
                        retVal.Add(ip);
                    }
                }
            }

            return retVal.ToArray();
        }

        /// <summary>
        /// Resolves host name to IP addresses.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <returns>Return specified host IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null.</exception>
        public static IPAddress[] Resolve(string host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            // If hostName_IP is IP
            try
            {
                return new[] {IPAddress.Parse(host)};
            }
            catch {}

            // This is probably NetBios name
            if (host.IndexOf(".") == -1)
            {
                return Dns.GetHostEntry(host).AddressList;
            }
            else
            {
                // hostName_IP must be host name, try to resolve it's IP
                Dns_Client dns = new Dns_Client();
                DnsServerResponse resp = dns.Query(host, QTYPE.A);
                if (resp.ResponseCode == RCODE.NO_ERROR)
                {
                    DNS_rr_A[] records = resp.GetARecords();
                    IPAddress[] retVal = new IPAddress[records.Length];
                    for (int i = 0; i < records.Length; i++)
                    {
                        retVal[i] = records[i].IP;
                    }

                    return retVal;
                }
                else
                {
                    throw new Exception(resp.ResponseCode.ToString());
                }
            }
        }

        /// <summary>
        /// Queries server with specified query.
        /// </summary>
        /// <param name="queryText">Query text. It depends on queryType.</param>
        /// <param name="queryType">Query type.</param>
        /// <returns></returns>
        public DnsServerResponse Query(string queryText, QTYPE queryType)
        {
            if (queryType == QTYPE.PTR)
            {
                string ip = queryText;

                // See if IP is ok.
                IPAddress ipA = IPAddress.Parse(ip);
                queryText = "";

                // IPv6
                if (ipA.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // 4321:0:1:2:3:4:567:89ab
                    // would be
                    // b.a.9.8.7.6.5.0.4.0.0.0.3.0.0.0.2.0.0.0.1.0.0.0.0.0.0.0.1.2.3.4.IP6.ARPA

                    char[] ipChars = ip.Replace(":", "").ToCharArray();
                    for (int i = ipChars.Length - 1; i > -1; i--)
                    {
                        queryText += ipChars[i] + ".";
                    }
                    queryText += "IP6.ARPA";
                }
                    // IPv4
                else
                {
                    // 213.35.221.186
                    // would be
                    // 186.221.35.213.in-addr.arpa

                    string[] ipParts = ip.Split('.');
                    //--- Reverse IP ----------
                    for (int i = 3; i > -1; i--)
                    {
                        queryText += ipParts[i] + ".";
                    }
                    queryText += "in-addr.arpa";
                }
            }

            return QueryServer(2000, queryText, queryType, 1);
        }

        /// <summary>
        /// Gets specified host IP addresses(A and AAAA).
        /// </summary>
        /// <param name="host">Host name.</param>
        /// <returns>Returns specified host IP addresses.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>host</b> is null reference.</exception>
        public IPAddress[] GetHostAddresses(string host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }

            List<IPAddress> retVal = new List<IPAddress>();

            // This is probably NetBios name
            if (host.IndexOf(".") == -1)
            {
                return Dns.GetHostEntry(host).AddressList;
            }
            else
            {
                DnsServerResponse response = Query(host, QTYPE.A);
                if (response.ResponseCode != RCODE.NO_ERROR)
                {
                    throw new DNS_ClientException(response.ResponseCode);
                }

                foreach (DNS_rr_A record in response.GetARecords())
                {
                    retVal.Add(record.IP);
                }

                response = Query(host, QTYPE.AAAA);
                if (response.ResponseCode != RCODE.NO_ERROR)
                {
                    throw new DNS_ClientException(response.ResponseCode);
                }

                foreach (DNS_rr_A record in response.GetARecords())
                {
                    retVal.Add(record.IP);
                }
            }

            return retVal.ToArray();
        }

        #endregion

        #region Internal methods

        internal static bool GetQName(byte[] reply, ref int offset, ref string name)
        {
            try
            {
                // Do while not terminator
                while (reply[offset] != 0)
                {
                    // Check if it's pointer(In pointer first two bits always 1)
                    bool isPointer = ((reply[offset] & 0xC0) == 0xC0);

                    // If pointer
                    if (isPointer)
                    {
                        // Pointer location number is 2 bytes long
                        // 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7  # byte 2 # 0 | 1 | 2 | | 3 | 4 | 5 | 6 | 7
                        // empty | < ---- pointer location number --------------------------------->
                        int pStart = ((reply[offset] & 0x3F) << 8) | (reply[++offset]);
                        offset++;
                        return GetQName(reply, ref pStart, ref name);
                    }
                    else
                    {
                        // label length (length = 8Bit and first 2 bits always 0)
                        // 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7
                        // empty | lablel length in bytes 
                        int labelLength = (reply[offset] & 0x3F);
                        offset++;

                        // Copy label into name 
                        name += Encoding.ASCII.GetString(reply, offset, labelLength);
                        offset += labelLength;
                    }

                    // If the next char isn't terminator,
                    // label continues - add dot between two labels
                    if (reply[offset] != 0)
                    {
                        name += ".";
                    }
                }

                // Move offset by terminator length
                offset++;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads character-string from spefcified data and offset.
        /// </summary>
        /// <param name="data">Data from where to read.</param>
        /// <param name="offset">Offset from where to start reading.</param>
        /// <returns>Returns readed string.</returns>
        internal static string ReadCharacterString(byte[] data, ref int offset)
        {
            /* RFC 1035 3.3.
                <character-string> is a single length octet followed by that number of characters. 
                <character-string> is treated as binary information, and can be up to 256 characters 
                in length (including the length octet).
            */

            int dataLength = data[offset++];
            string retVal = Encoding.Default.GetString(data, offset, dataLength);
            offset += dataLength;

            return retVal;
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Sends query to server.
        /// </summary>
        /// <param name="timeout">Query timeout in milli seconds.</param>
        /// <param name="qname">Query text.</param>
        /// <param name="qtype">Query type.</param>
        /// <param name="qclass">Query class.</param>
        /// <returns></returns>
        private DnsServerResponse QueryServer(int timeout, string qname, QTYPE qtype, int qclass)
        {
            if (m_DnsServers == null || m_DnsServers.Length == 0)
            {
                throw new Exception("Dns server isn't specified !");
            }

            // See if query is in cache
            if (m_UseDnsCache)
            {
                DnsServerResponse resopnse = DnsCache.GetFromCache(qname, (int) qtype);
                if (resopnse != null)
                {
                    return resopnse;
                }
            }

            int queryID = ID;
            byte[] query = CreateQuery(queryID, qname, qtype, qclass);

            // Create sending UDP socket.
            Socket udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpClient.SendTimeout = 500;

            // Send parallel query to all dns servers and get first answer.
            DateTime startTime = DateTime.Now;
                List<DnsServerResponse> responses = new List<DnsServerResponse>();
            while (startTime.AddMilliseconds(timeout) > DateTime.Now)
            {
                foreach (IPAddress dnsServer in m_DnsServers)
                {
                    try
                    {
                        udpClient.SendTo(query, new IPEndPoint(dnsServer, 53));
                    }
                    catch {}
                }

                // Wait 10 ms response to arrive, if no response, retransmit query.
                if (udpClient.Poll(10, SelectMode.SelectRead))
                {
                    try
                    {
                        byte[] retVal = new byte[1024];
                        int countRecieved = udpClient.Receive(retVal);

                        // If reply is ok, return it
                        DnsServerResponse serverResponse = ParseQuery(retVal, queryID);

                        // Cache query
                        if (m_UseDnsCache && serverResponse.ResponseCode == RCODE.NO_ERROR)
                        {
                            DnsCache.AddToCache(qname, (int) qtype, serverResponse);
                        }
                        responses.Add(serverResponse);
                    }
                    catch {}
                }
            }

            udpClient.Close();

            // If we reach so far, we probably won't get connection to dsn server
            return responses.Count>0?responses[0]:new DnsServerResponse(false,
                                         RCODE.SERVER_FAILURE,
                                         new List<DNS_rr_base>(),
                                         new List<DNS_rr_base>(),
                                         new List<DNS_rr_base>());
        }

        /// <summary>
        /// Creates new query.
        /// </summary>
        /// <param name="ID">Query ID.</param>
        /// <param name="qname">Query text.</param>
        /// <param name="qtype">Query type.</param>
        /// <param name="qclass">Query class.</param>
        /// <returns></returns>
        private byte[] CreateQuery(int ID, string qname, QTYPE qtype, int qclass)
        {
            byte[] query = new byte[512];

            //---- Create header --------------------------------------------//
            // Header is first 12 bytes of query

            /* 4.1.1. Header section format
										  1  1  1  1  1  1
			0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      ID                       |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    QDCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    ANCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    NSCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                    ARCOUNT                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
			QR  A one bit field that specifies whether this message is a
                query (0), or a response (1).
				
			OPCODE          A four bit field that specifies kind of query in this
                message.  This value is set by the originator of a query
                and copied into the response.  The values are:

                0               a standard query (QUERY)

                1               an inverse query (IQUERY)

                2               a server status request (STATUS)
				
			*/

            //--------- Header part -----------------------------------//
            query[0] = (byte) (ID >> 8);
            query[1] = (byte) (ID & 0xFF);
            query[2] = 1;
            query[3] = 0;
            query[4] = 0;
            query[5] = 1;
            query[6] = 0;
            query[7] = 0;
            query[8] = 0;
            query[9] = 0;
            query[10] = 0;
            query[11] = 0;
            //---------------------------------------------------------//

            //---- End of header --------------------------------------------//

            //----Create query ------------------------------------//

            /* 	Rfc 1035 4.1.2. Question section format
											  1  1  1  1  1  1
			0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                                               |
			/                     QNAME                     /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     QTYPE                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     QCLASS                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			
			QNAME
				a domain name represented as a sequence of labels, where
				each label consists of a length octet followed by that
				number of octets.  The domain name terminates with the
				zero length octet for the null label of the root.  Note
				that this field may be an odd number of octets; no
				padding is used.
			*/
            string[] labels = qname.Split(new[] {'.'});
            int position = 12;

            // Copy all domain parts(labels) to query
            // eg. lumisoft.ee = 2 labels, lumisoft and ee.
            // format = label.length + label(bytes)
            foreach (string label in labels)
            {
                // add label lenght to query
                query[position++] = (byte) (label.Length);

                // convert label string to byte array
                byte[] b = Encoding.ASCII.GetBytes(label);
                b.CopyTo(query, position);

                // Move position by label length
                position += b.Length;
            }

            // Terminate domain (see note above)
            query[position++] = 0;

            // Set QTYPE 
            query[position++] = 0;
            query[position++] = (byte) qtype;

            // Set QCLASS
            query[position++] = 0;
            query[position++] = (byte) qclass;
            //-------------------------------------------------------//
            string queryStr = Encoding.ASCII.GetString(query);
            return query;
        }

        /// <summary>
        /// Parses query.
        /// </summary>
        /// <param name="reply">Dns server reply.</param>
        /// <param name="queryID">Query id of sent query.</param>
        /// <returns></returns>
        private DnsServerResponse ParseQuery(byte[] reply, int queryID)
        {
            //--- Parse headers ------------------------------------//

            /* RFC 1035 4.1.1. Header section format
			 
											1  1  1  1  1  1
			  0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                      ID                       |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |QR|   Opcode  |AA|TC|RD|RA|   Z    |   RCODE   |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    QDCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    ANCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    NSCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 |                    ARCOUNT                    |
			 +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			 
			QDCOUNT
				an unsigned 16 bit integer specifying the number of
				entries in the question section.

			ANCOUNT
				an unsigned 16 bit integer specifying the number of
				resource records in the answer section.
				
			NSCOUNT
			    an unsigned 16 bit integer specifying the number of name
                server resource records in the authority records section.

			ARCOUNT
			    an unsigned 16 bit integer specifying the number of
                resource records in the additional records section.
				
			*/

            // Get reply code
            int id = (reply[0] << 8 | reply[1]);
            OPCODE opcode = (OPCODE) ((reply[2] >> 3) & 15);
            RCODE replyCode = (RCODE) (reply[3] & 15);
            int queryCount = (reply[4] << 8 | reply[5]);
            int answerCount = (reply[6] << 8 | reply[7]);
            int authoritiveAnswerCount = (reply[8] << 8 | reply[9]);
            int additionalAnswerCount = (reply[10] << 8 | reply[11]);
            //---- End of headers ---------------------------------//

            // Check that it's query what we want
            if (queryID != id)
            {
                throw new Exception("This isn't query with ID what we expected");
            }

            int pos = 12;

            //----- Parse question part ------------//
            for (int q = 0; q < queryCount; q++)
            {
                string dummy = "";
                GetQName(reply, ref pos, ref dummy);
                //qtype + qclass
                pos += 4;
            }
            //--------------------------------------//

            // 1) parse answers
            // 2) parse authoritive answers
            // 3) parse additional answers
            List<DNS_rr_base> answers = ParseAnswers(reply, answerCount, ref pos);
            List<DNS_rr_base> authoritiveAnswers = ParseAnswers(reply, authoritiveAnswerCount, ref pos);
            List<DNS_rr_base> additionalAnswers = ParseAnswers(reply, additionalAnswerCount, ref pos);

            return new DnsServerResponse(true, replyCode, answers, authoritiveAnswers, additionalAnswers);
        }

        /// <summary>
        /// Parses specified count of answers from query.
        /// </summary>
        /// <param name="reply">Server returned query.</param>
        /// <param name="answerCount">Number of answers to parse.</param>
        /// <param name="offset">Position from where to start parsing answers.</param>
        /// <returns></returns>
        private List<DNS_rr_base> ParseAnswers(byte[] reply, int answerCount, ref int offset)
        {
            /* RFC 1035 4.1.3. Resource record format
			 
										   1  1  1  1  1  1
			 0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                                               |
			/                                               /
			/                      NAME                     /
			|                                               |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      TYPE                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                     CLASS                     |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                      TTL                      |
			|                                               |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			|                   RDLENGTH                    |
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
			/                     RDATA                     /
			/                                               /
			+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
			*/

            List<DNS_rr_base> answers = new List<DNS_rr_base>();
            //---- Start parsing answers ------------------------------------------------------------------//
            for (int i = 0; i < answerCount; i++)
            {
                string name = "";
                if (!GetQName(reply, ref offset, ref name))
                {
                    throw new Exception("Error parsing anser");
                }

                int type = reply[offset++] << 8 | reply[offset++];
                int rdClass = reply[offset++] << 8 | reply[offset++];
                int ttl = reply[offset++] << 24 | reply[offset++] << 16 | reply[offset++] << 8 |
                          reply[offset++];
                int rdLength = reply[offset++] << 8 | reply[offset++];

                if ((QTYPE) type == QTYPE.A)
                {
                    answers.Add(DNS_rr_A.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.NS)
                {
                    answers.Add(DNS_rr_NS.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.CNAME)
                {
                    answers.Add(DNS_rr_CNAME.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.SOA)
                {
                    answers.Add(DNS_rr_SOA.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.PTR)
                {
                    answers.Add(DNS_rr_PTR.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.HINFO)
                {
                    answers.Add(DNS_rr_HINFO.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.MX)
                {
                    answers.Add(DNS_rr_MX.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.TXT)
                {
                    answers.Add(DNS_rr_TXT.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.AAAA)
                {
                    answers.Add(DNS_rr_AAAA.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.SRV)
                {
                    answers.Add(DNS_rr_SRV.Parse(reply, ref offset, rdLength, ttl));
                }
                else if ((QTYPE) type == QTYPE.NAPTR)
                {
                    answers.Add(DNS_rr_NAPTR.Parse(reply, ref offset, rdLength, ttl));
                }
                else
                {
                    // Unknown record, skip it.
                    offset += rdLength;
                }
            }

            return answers;
        }

        #endregion
    }
}