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
    public class SmtpValidator : Validator
    {
        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(string address)
        {
            if (!ActiveUp.Net.Mail.Validator.ValidateSyntax(address)) return false;
            else
            {
                try
                {
                    string domain = address.Split('@')[1];
                    bool result;
                    ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
                    smtp.SendTimeout = 0;
                    smtp.ReceiveTimeout = 0;
                    MxRecordCollection mxRecords = new MxRecordCollection();
                    try
                    {
                        mxRecords = ActiveUp.Net.Mail.Validator.GetMxRecords(domain);
                    }
                    catch
                    {
                        new System.Exception("Can't connect to DNS server.");
                    }
                    //Console.WriteLine(mxRecords.GetPrefered().Exchange);
                    if (mxRecords.Count > 0) smtp.Connect(mxRecords.GetPrefered().Exchange);
                    else return false;
                    try
                    {
                        smtp.Ehlo(System.Net.Dns.GetHostName());
                    }
                    catch
                    {
                        smtp.Helo(System.Net.Dns.GetHostName());
                    }
                    if (smtp.Verify(address)) result = true;
                    else
                    {
                        try
                        {
                            //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                            //smtp.MailFrom("postmaster@evolution-internet.com");
                            smtp.MailFrom("postmaster@" + domain);
                            smtp.RcptTo(address);
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex.ToString());
#if !PocketPC
                            System.Web.HttpContext.Current.Trace.Write("ActiveMail", ex.ToString());
#endif
                            result = false;
                        }
                    }
                    smtp.Disconnect();
                    return result;
                }
                catch
                {
                    return false;
                }
            }
        }

        private delegate bool DelegateValidate(string address);
        private static DelegateValidate _delegateValidate;

        public static IAsyncResult BeginValidate(string address, AsyncCallback callback)
        {
            SmtpValidator._delegateValidate = SmtpValidator.Validate;
            return SmtpValidator._delegateValidate.BeginInvoke(address, callback, SmtpValidator._delegateValidate);
        }


        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <param name="dnsServerHost">Name Server to be used for MX records search.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(string address, string dnsServerHost)
        {
            ActiveUp.Net.Mail.ServerCollection servers = new ActiveUp.Net.Mail.ServerCollection();
            servers.Add(dnsServerHost, 53);
            return ActiveUp.Net.Mail.SmtpValidator.Validate(address, servers);
        }

        private delegate bool DelegateValidateString(string address, string dnsServerHost);
        private static DelegateValidateString _delegateValidateString;

        public static IAsyncResult BeginValidate(string address, string dnsServerHost, AsyncCallback callback)
        {
            SmtpValidator._delegateValidateString = SmtpValidator.Validate;
            return SmtpValidator._delegateValidateString.BeginInvoke(address, dnsServerHost, callback, SmtpValidator._delegateValidateString);
        }

        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <param name="dnsServers">Name Servers to be used for MX records search.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(string address, ServerCollection dnsServers)
        {
            if (!ActiveUp.Net.Mail.Validator.ValidateSyntax(address)) return false;
            else
            {
                string domain = address.Split('@')[1];
                bool result;
                ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
                smtp.SendTimeout = 15;
                smtp.ReceiveTimeout = 15;

                MxRecordCollection mxRecords = new MxRecordCollection();
                try
                {
#if !PocketPC
                    mxRecords = ActiveUp.Net.Mail.Validator.GetMxRecords(domain, dnsServers);
#else
                    mxRecords = ActiveUp.Net.Mail.Validator.GetMxRecords(domain);
#endif
                }
                catch
                {
                    new System.Exception("Can't connect to DNS server.");
                }
                smtp.Connect(mxRecords.GetPrefered().Exchange);
                try
                {
                    smtp.Ehlo(System.Net.Dns.GetHostName());
                }
                catch
                {
                    smtp.Helo(System.Net.Dns.GetHostName());
                }
                if (smtp.Verify(address)) result = true;
                else
                {
                    try
                    {
                        //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                        //smtp.MailFrom("postmaster@evolution-internet.com");
                        smtp.MailFrom("postmaster@" + domain);
                        smtp.RcptTo(address);
                        result = true;
                    }
                    catch
                    {
                        result = false;
                    }
                }
                smtp.Disconnect();
                return result;
            }
        }

        private delegate bool DelegateValidateStringServers(string address, ServerCollection servers);
        private static DelegateValidateStringServers _delegateValidateStringServers;

        public static IAsyncResult BeginValidate(string address, ServerCollection servers, AsyncCallback callback)
        {
            SmtpValidator._delegateValidateStringServers = SmtpValidator.Validate;
            return SmtpValidator._delegateValidateStringServers.BeginInvoke(address, servers, callback, SmtpValidator._delegateValidateStringServers);
        }

        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(Address address)
        {
            return SmtpValidator.Validate(address.Email);
        }

        private delegate bool DelegateValidateAddress(Address address);
        private static DelegateValidateAddress _delegateValidateAddress;

        public static IAsyncResult BeginValidate(Address address, AsyncCallback callback)
        {
            SmtpValidator._delegateValidateAddress = SmtpValidator.Validate;
            return SmtpValidator._delegateValidateAddress.BeginInvoke(address, callback, SmtpValidator._delegateValidateAddress);
        }

        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <param name="dnsServers">Name Servers to be used for MX records search.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(Address address, ServerCollection dnsServers)
        {
            return SmtpValidator.Validate(address.Email, dnsServers);
        }

        private delegate bool DelegateValidateAddressServers(Address address, ServerCollection dnsServers);
        private static DelegateValidateAddressServers _delegateValidateAddressServers;

        public static IAsyncResult BeginValidate(Address address, ServerCollection dnsServers, AsyncCallback callback)
        {
            SmtpValidator._delegateValidateAddressServers = SmtpValidator.Validate;
            return SmtpValidator._delegateValidateAddressServers.BeginInvoke(address, dnsServers, callback, SmtpValidator._delegateValidateAddressServers);
        }

        /// <summary>
        /// Validates syntax and existence of the given address.
        /// </summary>
        /// <param name="address">The address to be validated.</param>
        /// <param name="dnsServerHost">Name Server to be used for MX records search.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public static bool Validate(Address address, string dnsServerHost)
        {
            ActiveUp.Net.Mail.ServerCollection servers = new ActiveUp.Net.Mail.ServerCollection();
            servers.Add(dnsServerHost, 53);
            return ActiveUp.Net.Mail.SmtpValidator.Validate(address.Email, servers);
        }

        private delegate bool DelegateValidateAddressString(Address address, string dnsServerHost);
        private static DelegateValidateAddressString _delegateValidateAddressString;

        public static IAsyncResult BeginValidate(Address address, string dnsServerHost, AsyncCallback callback)
        {
            SmtpValidator._delegateValidateAddressString = SmtpValidator.Validate;
            return SmtpValidator._delegateValidateAddressString.BeginInvoke(address, dnsServerHost, callback, SmtpValidator._delegateValidateAddressString);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return (bool)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Validates syntax and existence of the given addresses and returns a collection of invalid or inexistent addresses.
        /// </summary>
        /// <param name="addresses">The addresses to be examined.</param>
        /// <returns>A collection containing the invalid addresses.</returns>
        public static AddressCollection GetInvalidAddresses(AddressCollection addresses)
        {
            ActiveUp.Net.Mail.AddressCollection invalids = new ActiveUp.Net.Mail.AddressCollection();
            ActiveUp.Net.Mail.AddressCollection valids = new ActiveUp.Net.Mail.AddressCollection();
            System.Collections.Specialized.HybridDictionary ads = new System.Collections.Specialized.HybridDictionary();
            for (int i = 0; i < addresses.Count; i++)
                if (!ActiveUp.Net.Mail.Validator.ValidateSyntax(addresses[i].Email)) invalids.Add(addresses[i]);
                else valids.Add(addresses[i]);
#if !PocketPC
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count }, new int[] { 0 });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count }, new int[] { 0 });
#else
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count });
#endif

            for (int i = 0; i < valids.Count; i++)
            {
                domains.SetValue(valids[i].Email.Split('@')[1], i);
                adds.SetValue(valids[i], i);
            }
            System.Array.Sort(domains, adds, null);
            string currentDomain = "";
            string address = "";
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            bool isConnected = false;
            for (int i = 0; i < adds.Length; i++)
            {
                address = ((ActiveUp.Net.Mail.Address)adds.GetValue(i)).Email;
                if (((string)domains.GetValue(i)) == currentDomain)
                {
                    if (!smtp.Verify(address))
                    {
                        try
                        {
                            //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                            smtp.RcptTo(address);
                        }
                        catch
                        {
                            invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                        }
                    }
                }
                else
                {
                    currentDomain = (string)domains.GetValue(i);
                    try
                    {
                        if (isConnected == true)
                        {
                            isConnected = false;
                            smtp.Disconnect();
                            smtp = new ActiveUp.Net.Mail.SmtpClient();
                        }

                        smtp.Connect(ActiveUp.Net.Mail.Validator.GetMxRecords(currentDomain).GetPrefered().Exchange);
                        isConnected = true;
                        try
                        {
                            smtp.Ehlo(System.Net.Dns.GetHostName());
                        }
                        catch
                        {
                            smtp.Helo(System.Net.Dns.GetHostName());
                        }
                        if (!smtp.Verify(address))
                        {
                            try
                            {
                                //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                                //smtp.MailFrom("postmaster@evolution-internet.com");
                                smtp.MailFrom("postmaster@" + currentDomain);
                                smtp.RcptTo(address);
                            }
                            catch
                            {
                                invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                            }
                        }
                    }
                    catch
                    {
                        invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                    }
                }
            }
            if (isConnected == true)
                smtp.Disconnect();
            return invalids;
        }

        private delegate AddressCollection DelegateGetInvalidAddresses(AddressCollection addresses);
        private static DelegateGetInvalidAddresses _delegateGetInvalidAddresses;

        public static IAsyncResult BeginGetInvalidAddresses(AddressCollection addresses, AsyncCallback callback)
        {
            SmtpValidator._delegateGetInvalidAddresses = SmtpValidator.GetInvalidAddresses;
            return SmtpValidator._delegateGetInvalidAddresses.BeginInvoke(addresses, callback, SmtpValidator._delegateGetInvalidAddresses);
        }

        /// <summary>
        /// Validates syntax and existence of the given address and returns valid addresses.
        /// </summary>
        /// <param name="addresses">The collection to be filtered.</param>
        /// <returns>A collection containing the valid addresses.</returns>
        public static AddressCollection Filter(AddressCollection addresses)
        {
            ActiveUp.Net.Mail.AddressCollection valids = new ActiveUp.Net.Mail.AddressCollection();
            ActiveUp.Net.Mail.AddressCollection valids1 = new ActiveUp.Net.Mail.AddressCollection();
            System.Collections.Specialized.HybridDictionary ads = new System.Collections.Specialized.HybridDictionary();
            for (int i = 0; i < addresses.Count; i++)
                if (ActiveUp.Net.Mail.Validator.ValidateSyntax(addresses[i].Email)) valids.Add(addresses[i]);
#if !PocketPC
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count }, new int[] { 0 });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count }, new int[] { 0 });
#else
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count });
#endif
            for (int i = 0; i < valids.Count; i++)
            {
                domains.SetValue(valids[i].Email.Split('@')[1], i);
                adds.SetValue(valids[i], i);
            }
            System.Array.Sort(domains, adds, null);
            string currentDomain = "";
            string address = "";
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            bool isConnected = false;
            for (int i = 0; i < adds.Length; i++)
            {
                address = ((ActiveUp.Net.Mail.Address)adds.GetValue(i)).Email;
                if (((string)domains.GetValue(i)) == currentDomain)
                {
                    if (!smtp.Verify(address))
                    {
                        try
                        {
                            //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                            //smtp.MailFrom("postmaster@"+currentDomain);
                            smtp.RcptTo(address);
                            valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                        }
                        catch
                        {

                        }
                    }
                    else valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                }
                else
                {
                    currentDomain = (string)domains.GetValue(i);
                    try
                    {
                        if (isConnected == true)
                        {
                            isConnected = false;
                            smtp.Disconnect();
                            smtp = new ActiveUp.Net.Mail.SmtpClient();
                        }

                        smtp.Connect(ActiveUp.Net.Mail.Validator.GetMxRecords(currentDomain).GetPrefered().Exchange);
                        isConnected = true;
                        try
                        {
                            smtp.Ehlo(System.Net.Dns.GetHostName());
                        }
                        catch
                        {
                            smtp.Helo(System.Net.Dns.GetHostName());
                        }
                        if (!smtp.Verify(address))
                        {
                            try
                            {

                                //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                                //smtp.MailFrom("postmaster@evolution-internet.com");
                                smtp.MailFrom("postmaster@" + currentDomain);
                                smtp.RcptTo(address);
                                valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                            }
                            catch
                            {

                            }
                        }
                        else valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                    }
                    catch
                    {

                    }
                }
            }
            if (isConnected == true)
                smtp.Disconnect();
            return valids1;
        }

        private delegate AddressCollection DelegateFilter(AddressCollection addresses);
        private static DelegateFilter _delegateFilter;

        public static IAsyncResult BeginFilter(AddressCollection addresses, AsyncCallback callback)
        {
            SmtpValidator._delegateFilter = SmtpValidator.Filter;
            return SmtpValidator._delegateFilter.BeginInvoke(addresses, callback, SmtpValidator._delegateFilter);
        }

        /// <summary>
        /// Validates syntax and existence of the given addresses and returns a collection of invalid or inexistent addresses.
        /// </summary>
        /// <param name="addresses">The addresses to be examined.</param>
        /// <param name="dnsServers">Name Servers to be used for MX records search.</param>
        /// <returns>A collection containing the invalid addresses.</returns>
        public static AddressCollection GetInvalidAddresses(AddressCollection addresses, ServerCollection dnsServers)
        {
            ActiveUp.Net.Mail.AddressCollection invalids = new ActiveUp.Net.Mail.AddressCollection();
            ActiveUp.Net.Mail.AddressCollection valids = new ActiveUp.Net.Mail.AddressCollection();
            System.Collections.Specialized.HybridDictionary ads = new System.Collections.Specialized.HybridDictionary();
            for (int i = 0; i < addresses.Count; i++)
                if (!ActiveUp.Net.Mail.Validator.ValidateSyntax(addresses[i].Email)) invalids.Add(addresses[i]);
                else valids.Add(addresses[i]);
#if !PocketPC
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count }, new int[] { 0 });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count }, new int[] { 0 });
#else
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count });
#endif
            for (int i = 0; i < valids.Count; i++)
            {
                domains.SetValue(valids[i].Email.Split('@')[1], i);
                adds.SetValue(valids[i], i);
            }
            System.Array.Sort(domains, adds, null);
            string currentDomain = "";
            string address = "";
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            bool isConnected = false;
            for (int i = 0; i < adds.Length; i++)
            {
                address = ((ActiveUp.Net.Mail.Address)adds.GetValue(i)).Email;
                if (((string)domains.GetValue(i)) == currentDomain)
                {
                    if (!smtp.Verify(address))
                    {
                        try
                        {
                            //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                            //smtp.MailFrom("postmaster@"+currentDomain);
                            smtp.RcptTo(address);
                        }
                        catch
                        {
                            invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                        }
                    }
                }
                else
                {
                    currentDomain = (string)domains.GetValue(i);
                    try
                    {
                        if (isConnected == true)
                        {
                            isConnected = false;
                            smtp.Disconnect();
                            smtp = new ActiveUp.Net.Mail.SmtpClient();
                        }

                        smtp.Connect(ActiveUp.Net.Mail.Validator.GetMxRecords(currentDomain, dnsServers).GetPrefered().Exchange);
                        isConnected = true;
                        try
                        {
                            smtp.Ehlo(System.Net.Dns.GetHostName());
                        }
                        catch
                        {
                            smtp.Helo(System.Net.Dns.GetHostName());
                        }
                        if (!smtp.Verify(address))
                        {
                            try
                            {
                                //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                                //smtp.MailFrom("postmaster@evolution-internet.com");
                                smtp.MailFrom("postmaster@" + currentDomain);
                                smtp.RcptTo(address);
                            }
                            catch
                            {
                                invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                            }
                        }
                    }
                    catch
                    {
                        invalids.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                    }
                }
            }
            if (isConnected == true)
                smtp.Disconnect();
            return invalids;
        }

        private delegate AddressCollection DelegateGetInvalidAddressesServers(AddressCollection addresses, ServerCollection dnsServers);
        private static DelegateGetInvalidAddressesServers _delegateGetInvalidAddressesServers;

        public static IAsyncResult BeginGetInvalidAddresses(AddressCollection addresses, ServerCollection dnsServers, AsyncCallback callback)
        {
            SmtpValidator._delegateGetInvalidAddressesServers = SmtpValidator.GetInvalidAddresses;
            return SmtpValidator._delegateGetInvalidAddressesServers.BeginInvoke(addresses, dnsServers, callback, SmtpValidator._delegateGetInvalidAddressesServers);
        }

        public AddressCollection EndGetInvalidAddresses(IAsyncResult result)
        {
            return (AddressCollection)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

        /// <summary>
        /// Validates syntax and existence of the given address and returns valid addresses.
        /// </summary>
        /// <param name="addresses">The collection to be filtered.</param>
        /// <param name="dnsServers">Name Servers to be used for MX records search.</param>
        /// <returns>A collection containing the valid addresses.</returns>
        public static AddressCollection Filter(AddressCollection addresses, ServerCollection dnsServers)
        {
            ActiveUp.Net.Mail.AddressCollection valids = new ActiveUp.Net.Mail.AddressCollection();
            ActiveUp.Net.Mail.AddressCollection valids1 = new ActiveUp.Net.Mail.AddressCollection();
            System.Collections.Specialized.HybridDictionary ads = new System.Collections.Specialized.HybridDictionary();
            for (int i = 0; i < addresses.Count; i++)
                if (ActiveUp.Net.Mail.Validator.ValidateSyntax(addresses[i].Email)) valids.Add(addresses[i]);
#if !PocketPC
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count }, new int[] { 0 });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count }, new int[] { 0 });
#else
            System.Array domains = System.Array.CreateInstance(typeof(string), new int[] { valids.Count });
            System.Array adds = System.Array.CreateInstance(typeof(ActiveUp.Net.Mail.Address), new int[] { valids.Count });
#endif
            for (int i = 0; i < valids.Count; i++)
            {
                domains.SetValue(valids[i].Email.Split('@')[1], i);
                adds.SetValue(valids[i], i);
            }
            System.Array.Sort(domains, adds, null);
            string currentDomain = "";
            string address = "";
            ActiveUp.Net.Mail.SmtpClient smtp = new ActiveUp.Net.Mail.SmtpClient();
            bool isConnected = false;
            for (int i = 0; i < adds.Length; i++)
            {
                address = ((ActiveUp.Net.Mail.Address)adds.GetValue(i)).Email;
                if (((string)domains.GetValue(i)) == currentDomain)
                {
                    if (!smtp.Verify(address))
                    {
                        try
                        {
                            //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                            //smtp.MailFrom("postmaster@"+currentDomain);
                            smtp.RcptTo(address);
                            valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                        }
                        catch
                        {

                        }
                    }
                    else valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                }
                else
                {
                    currentDomain = (string)domains.GetValue(i);
                    try
                    {
                        if (isConnected == true)
                        {
                            isConnected = false;
                            smtp.Disconnect();
                            smtp = new ActiveUp.Net.Mail.SmtpClient();
                        }

                        smtp.Connect(ActiveUp.Net.Mail.Validator.GetMxRecords(currentDomain, dnsServers).GetPrefered().Exchange);
                        isConnected = true;
                        try
                        {
                            smtp.Ehlo(System.Net.Dns.GetHostName());
                        }
                        catch
                        {
                            smtp.Helo(System.Net.Dns.GetHostName());
                        }
                        if (!smtp.Verify(address))
                        {
                            try
                            {
                                //smtp.MailFrom("postmaster@"+System.Net.Dns.GetHostName());
                                //smtp.MailFrom("postmaster@evolution-internet.com");
                                smtp.MailFrom("postmaster@" + currentDomain);
                                smtp.RcptTo(address);
                                valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                            }
                            catch
                            {

                            }
                        }
                        else valids1.Add((ActiveUp.Net.Mail.Address)adds.GetValue(i));
                    }
                    catch
                    {

                    }
                }
            }
            if (isConnected == true)
                smtp.Disconnect();
            return valids1;
        }

        private delegate AddressCollection DelegateFilterServers(AddressCollection addresses, ServerCollection dnsServers);
        private static DelegateFilterServers _delegateFilterServers;

        public static IAsyncResult BeginFilter(AddressCollection addresses, ServerCollection dnsServers, AsyncCallback callback)
        {
            SmtpValidator._delegateFilterServers = SmtpValidator.Filter;
            return SmtpValidator._delegateFilterServers.BeginInvoke(addresses, dnsServers, callback, SmtpValidator._delegateFilterServers);

        }

        public AddressCollection EndFilter(IAsyncResult result)
        {
            return (AddressCollection)result.AsyncState.GetType().GetMethod("EndInvoke").Invoke(result.AsyncState, new object[] { result });
        }

    }
}
