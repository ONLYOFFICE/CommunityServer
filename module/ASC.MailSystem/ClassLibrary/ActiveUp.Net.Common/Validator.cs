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

//using System.Management;
using System;
using System.Collections;
using Microsoft.Win32;
using ActiveUp.Net.Mail;
#if !PocketPC
using System.Net.NetworkInformation;
#endif
using System.Net;
using ActiveUp.Net.Dns;
using System.Collections.Generic;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// 
    /// </summary>
#if !PocketPC
    [System.Serializable]
#endif
    public class Validator
    {
        ActiveUp.Net.Mail.ServerCollection _dnsServers = new ActiveUp.Net.Mail.ServerCollection();
        /// <summary>
        /// Constructor.
        /// </summary>
        public Validator()
        {
             
        }
        
        /// <summary>
        /// Validates the address' syntax.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <returns>True if syntax is valid, otherwise false.</returns>
        public static bool ValidateSyntax(string address)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(address,"\\w+([-+.]\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*");
        }
        /// <summary>
        /// Validates the address' syntax.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <returns>True if syntax is valid, otherwise false.</returns>
        public static bool ValidateSyntax(ActiveUp.Net.Mail.Address address)
        {
            System.Int32.Parse("20",System.Globalization.NumberStyles.HexNumber);
            return ActiveUp.Net.Mail.Validator.ValidateSyntax(address.Email);
        }
        /// <summary>
        /// Validates the addresses' syntax.
        /// </summary>
        /// <param name="addresses">The addresses to be validated.</param>
        /// <returns>True if syntax is valid, otherwise false.</returns>
        public static ActiveUp.Net.Mail.AddressCollection ValidateSyntax(ActiveUp.Net.Mail.AddressCollection addresses)
        {
            ActiveUp.Net.Mail.AddressCollection invalids = new ActiveUp.Net.Mail.AddressCollection();
            foreach(ActiveUp.Net.Mail.Address address in addresses) if(!ActiveUp.Net.Mail.Validator.ValidateSyntax(address.Email)) invalids.Add(address);
            return invalids;
        }

        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <returns>A collection of Mx Records.</returns>
        public static MxRecordCollection GetMxRecords(string address)
        {
            return GetMxRecords(address, 5000);
        }

        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <param name="timeout">The timeout in miliseconds.</param>
        /// <returns>A collection of Mx Records.</returns>
        public static MxRecordCollection GetMxRecords(string address, int timeout)
        {
            ArrayList nameServers = GetListNameServers();

            if (nameServers.Count > 0)
            {
                ActiveUp.Net.Mail.Logger.AddEntry("Name servers found : " + nameServers.Count.ToString(), 0);

                foreach(string server in nameServers)
                {
                    if (server.Length > 3)
                    {
                        try
                        {
                            ActiveUp.Net.Mail.Logger.AddEntry("Ask " + server + ":53 for MX records.", 0); 
                            return GetMxRecords(address, server, 53, timeout);
                        }
                        catch
                        {
                            ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to " + server + ":53", 0);
                        }
                    }
                }
            }

            ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to any of the specified DNS servers.", 0);

            return null;
        }

        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <returns>The byte array.</returns>
        public static byte[] GetTxtRecords(string address)
        {
            return GetTxtRecords(address, 5000);
        }

        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <param name="timeout">The timeout in miliseconds.</param>
        /// <returns>The byte array.</returns>
        public static byte[] GetTxtRecords(string address, int timeout)
        {
            ArrayList nameServers = GetListNameServers();
            if (nameServers.Count > 0)
            {
                ActiveUp.Net.Mail.Logger.AddEntry("Name servers found : " + nameServers.Count.ToString(), 0);

                foreach (string server in nameServers)
                {
                    if (server.Length > 3)
                    {
                        try
                        {
                            ActiveUp.Net.Mail.Logger.AddEntry("Ask " + server + ":53 for TXT records.", 0);
                            return GetTxtRecords(address, server, 53);
                        }
                        catch
                        {
                            ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to " + server + ":53", 0);
                        }
                    }
                }
            }

            ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to any of the specified DNS servers.", 0);

            return null;
        }

        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <param name="dnsServers">Servers to be used for MX records search.</param>
        /// <returns>A collection of Mx Records.</returns>
        public static ActiveUp.Net.Mail.MxRecordCollection GetMxRecords(string address, ActiveUp.Net.Mail.ServerCollection dnsServers)
        {
            return GetMxRecords(address, dnsServers, 5000);
        }
            
        /// <summary>
        /// Get the MX records for the specified domain name using the system configuration.
        /// </summary>
        /// <param name="address">The domain name.</param>
        /// <param name="dnsServers">Servers to be used for MX records search.</param>
        /// <param name="timeout">The timeout in miliseconds.</param>
        /// <returns>A collection of Mx Records.</returns>
        public static ActiveUp.Net.Mail.MxRecordCollection GetMxRecords(string address, 
            ActiveUp.Net.Mail.ServerCollection dnsServers, int timeout)
        {
            if (dnsServers == null)
                dnsServers = new ServerCollection();

            if (dnsServers.Count == 0)
            {
#if !PocketPC
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    if (properties.DnsAddresses.Count > 0)
                    {
                        foreach (IPAddress ipAddress in properties.DnsAddresses)
                            dnsServers.Add(ipAddress.ToString(), 53);
                    }
                }
#endif
            }

            foreach(ActiveUp.Net.Mail.Server server in dnsServers)
            {
                try
                {
                    return GetMxRecords(address, server.Host, server.Port, timeout);
                }
                catch
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to " + server.Host + ":" + server.Port, 0);
                }
            }

        return GetMxRecords(address);

        //ActiveUp.Net.Mail.Logger.AddEntry("Can't connect to any of the specified DNS servers.", 0);
    }

    /// <summary>
    /// Get the MX records for the specified domain name using the specified DNS server.
    /// </summary>
    /// <param name="address">The domain name.</param>
    /// <param name="host">The host name of the DNS server to use.</param>
    /// <param name="port">The port number of the DNS server to use.</param>
    /// <returns>A collection of Mx Records.</returns>
    public static ActiveUp.Net.Mail.MxRecordCollection GetMxRecords(string address, string host, int port)
    {
        return GetMxRecords(address, host, port, 5000);
    }

    /// <summary>
    /// Get the MX records for the specified domain name using the specified DNS server.
    /// </summary>
    /// <param name="address">The domain name.</param>
    /// <param name="host">The host name of the DNS server to use.</param>
    /// <param name="port">The port number of the DNS server to use.</param>
    /// <param name="timeout">The timeout in miliseconds.</param>
    /// <returns>A collection of Mx Records.</returns>
    public static ActiveUp.Net.Mail.MxRecordCollection GetMxRecords(string address, string host, int port, int timeout)
    {
        MxRecordCollection mxRecords = new MxRecordCollection();

        DnsQuery query = new DnsQuery(IPAddress.Parse(host));

        query.RecursiveQuery = true;
        query.DnsServer.Port = port;
        query.Domain = address;
  
        DnsAnswer answer = query.QueryServer(RecordType.MX, timeout);

        foreach (DnsEntry entry in answer.Answers)
        {
            MXRecord mxRecord = (MXRecord)entry.Data;

            mxRecords.Add(mxRecord.Domain, mxRecord.Preference);
        }
                
        return mxRecords;
    }

        public static byte[] GetTxtRecords(string address, string host, int port)
        {
            byte[] header = { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
            string[] addressParts = address.Split('.');
            byte[] label = new byte[address.Length + 2];
            int pos = 0;
            foreach (string part in addressParts)
            {
                label[pos++] = System.Convert.ToByte(part.Length);
                foreach (char character in part)
                    label[pos++] = System.Convert.ToByte(character);
                label[pos] = 0;
            }
            byte[] footer = { 0, 16, 0, 1 };

            // Build the query
            byte[] query = new byte[header.Length + label.Length + footer.Length];

            header.CopyTo(query, 0);
            label.CopyTo(query, header.Length);
            footer.CopyTo(query, header.Length + label.Length);

            // Send the query
            //System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(GetIpAddress(dnsHost), 53);
            System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), 53);
            //System.Net.Sockets.UdpClient udpClient = new System.Net.Sockets.UdpClient();
            ActiveUp.Net.Mail.TimedUdpClient udpClient = new ActiveUp.Net.Mail.TimedUdpClient();
            udpClient.Connect(endPoint);
            udpClient.Send(query, query.Length);
            byte[] response2;
            try
            {
                response2 = udpClient.Receive(ref endPoint);
            }
            catch(Exception)
            {
                udpClient.Close();
                throw new System.Exception("Can't connect to DNS server.");
            }

            //System.IO.FileStream fs = new System.IO.FileStream("c:\\Inetpub\\wwwroot\\brol.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            //fs.Write(response2, 0, response2.Length);
            //fs.Close();

            pos = query.Length;

            //ActiveUp.Net.Mail.MxRecordCollection mxRecords = new ActiveUp.Net.Mail.MxRecordCollection();

            //for (index = 1; index <= answersCount; index++)
            //{
                //ActiveUp.Net.Mail.MxRecord newMxRecord = new ActiveUp.Net.Mail.MxRecord();

            GetLabelsByPos(response2, ref pos);
            pos += 7;
            int length = response2[pos];
            byte[] data = new byte[response2.Length-pos-4];
            Array.Copy(response2, pos + 4, data, 0, response2.Length - pos - 4);
            
            return data;
        }
        /// <summary>
        /// Get the label at the specified position in the DNS data.
        /// </summary>
        /// <param name="streamData">The DNS data.</param>
        /// <param name="pos">The start position.</param>
        /// <returns>The label.</returns>
        private static string GetLabelsByPos(byte[] streamData, ref int pos)
        {
            int currentPos = pos;
            byte[] buffer = streamData;
            byte labelLength;
            bool pointerFound = false;
            string temp = string.Empty, stringData = System.Text.Encoding.ASCII.GetString(streamData,0,streamData.Length);

            labelLength = buffer[currentPos];

            while (labelLength != 0 && !pointerFound)
            {
                // Pointer found
                if ((labelLength & 192) == 192)
                {
                    int newPointer;

                    if (buffer[currentPos] == 192)
                        newPointer = (buffer[currentPos]-192)*256 + buffer[currentPos+1]; 
                    else
                        newPointer = buffer[currentPos+1];

                    temp += GetLabelsByPos(streamData, ref newPointer);
                    temp += ".";

                    currentPos += 2;
                    
                    pointerFound = true;
                }
                else
                {
                    temp += stringData.Substring(currentPos+1, labelLength) + ".";
                    currentPos = currentPos + labelLength + 1;
                    labelLength = buffer[currentPos];
                }
            }

            if (pointerFound)
                pos = currentPos;
            else
                pos = currentPos+1;

            if (temp.Length > 0)
            {
                return temp.TrimEnd('.');
            }

            return temp;
        }

        public static ArrayList GetListNameServers()
        {
            ArrayList nameServers = new ArrayList();
            
            IList<IPAddress> machineDnsServers = DnsQuery.GetMachineDnsServers();
            foreach (IPAddress ipAddress in machineDnsServers)
                nameServers.Add(ipAddress.ToString());

            return nameServers;
        }

        private static bool IsPresent(ArrayList list, string valueToTest)
        {
            foreach(string valueList in list)
            {
                if (valueToTest == valueList)
                {
                    return true;
                }
            }

            return false;
        }

    }
}

