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

using ASC.Web.Core.Utility.Settings;
using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Serialization;

namespace ASC.ActiveDirectory
{
    [Serializable]
    [DataContract]
    public class LDAPSupportSettings : ISettings
    {
        public Guid ID
        {
            get
            {
                return new Guid("{197149b3-fbc9-44c2-b42a-232f7e729c16}");
            }
        }

        public ISettings GetDefault()
        {
            string domainName;
            try
            {
                var domain = Domain.GetCurrentDomain();
                domainName = String.Format(@"LDAP://{0}", domain.Name);
            }
            catch
            {
                domainName = string.Empty;
            }
            var dn = ADDomain.GetDefaultDistinguishedName(domainName, Constants.STANDART_LDAP_PORT);
            return new LDAPSupportSettings
            {
                EnableLdapAuthentication = false,
                Server = domainName,
                UserDN = dn,
                PortNumber = Constants.STANDART_LDAP_PORT,
                UserFilter = string.Empty,
                BindAttribute = Constants.ADSchemaAttributes.UserPrincipalName,
                LoginAttribute = Constants.ADSchemaAttributes.AccountName,
                GroupMembership = false,
                GroupDN = dn,
                GroupName = string.Empty,
                UserAttribute = Constants.ADSchemaAttributes.DistinguishedName,
                GroupAttribute = Constants.ADSchemaAttributes.Member,
                Authentication = dn == string.Empty,
                Login = string.Empty,
                Password = string.Empty
            };
        }

        [DataMember]
        public bool EnableLdapAuthentication
        {
            get;
            set;
        }

        [DataMember]
        public string Server
        {
            get;
            set;
        }

        [DataMember]
        public string UserDN
        {
            get;
            set;
        }

        [DataMember]
        public int PortNumber
        {
            get;
            set;
        }

        [DataMember]
        public string UserFilter
        {
            get;
            set;
        }

        [DataMember]
        public string BindAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string LoginAttribute
        {
            get;
            set;
        }

        [DataMember]
        public bool GroupMembership
        {
            get;
            set;
        }

        [DataMember]
        public string GroupDN
        {
            get;
            set;
        }

        [DataMember]
        public string GroupName
        {
            get;
            set;
        }

        [DataMember]
        public string UserAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string GroupAttribute
        {
            get;
            set;
        }

        [DataMember]
        public bool Authentication
        {
            get;
            set;
        }

        [DataMember]
        public string Login
        {
            get;
            set;
        }

        [DataMember]
        public string Password
        {
            get;
            set;
        }

        [DataMember]
        public byte[] PasswordBytes
        {
            get;
            set;
        }
    }
}
