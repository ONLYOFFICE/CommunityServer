/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using log4net;
using System;
using System.DirectoryServices;

namespace ASC.ActiveDirectory
{
    /// <summary>
    
    /// </summary>
    public class LDAPComputer : LDAPAccount
    {
        /// <summary>
        
        /// </summary>
        
        internal LDAPComputer(DirectoryEntry directoryEntry) : base(directoryEntry)
        { 
        }

        /// <summary>
        
        /// </summary>
        public string HostName
        {
            get
            {
                string hostName = InvokeGet(Constants.ADSchemaAttributes.DNSHostName) as string;
                return hostName;
            }
        }
        /// <summary>
        
        /// </summary>
        public string OperatingSystem
        {
            get
            {
                string value = InvokeGet(Constants.ADSchemaAttributes.OperatingSystem) as string;
                return value;
            }
        }
        /// <summary>
        
        /// </summary>
        public string OperatingSystemVersion
        {
            get
            {
                string value = InvokeGet(Constants.ADSchemaAttributes.OperatingSystemVersion) as string;
                return value;
            }
        }
        /// <summary>
        
        /// </summary>
        public string OperatingSystemServicePack
        {
            get
            {
                string value = InvokeGet(Constants.ADSchemaAttributes.OperatingSystemServicePack) as string;
                return value;
            }
        }
        /// <summary>
        
        /// </summary>
        public string OperatingSystemHotfix
        {
            get
            {
                string value = InvokeGet(Constants.ADSchemaAttributes.OperatingSystemHotfix) as string;
                return value;
            }
        }

        /// <summary>
        
        /// </summary>
        public bool IsOnline
        {
            get
            {
                try
                {
                    System.Net.IPAddress[] adress = System.Net.Dns.GetHostAddresses(Name);
                    System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                    System.Net.NetworkInformation.PingReply pr = ping.Send(adress[0]);
                    Console.WriteLine(pr.Status);
                    if (pr.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(GetType()).Error(e);
                    return false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} - [{1} {2} {3} {4}]", HostName, OperatingSystem, 
                OperatingSystemVersion, OperatingSystemServicePack, OperatingSystemHotfix);
        }
    }
}
