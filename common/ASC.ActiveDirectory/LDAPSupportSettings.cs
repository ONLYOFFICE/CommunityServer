/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


﻿using ASC.ActiveDirectory.BuiltIn;
using ASC.ActiveDirectory.DirectoryServices;
using ASC.ActiveDirectory.Novell;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Serialization;

namespace ASC.ActiveDirectory
{
    [Serializable]
    [DataContract]
    public class LDAPSupportSettings : ISettings
    {
        [NonSerialized]
        private readonly LdapHelper ldapHelper;

        public LDAPSupportSettings()
        {
            ldapHelper = !WorkContext.IsMono ? (LdapHelper)new SystemLdapHelper() : new NovellLdapHelper();
        }

        public Guid ID
        {
            get
            {
                return new Guid("{197149b3-fbc9-44c2-b42a-232f7e729c16}");
            }
        }

        public ISettings GetDefault()
        {
            string domainName = null;
            string userFilter;
            string loginAttribute;
            string userAttribute;
            string groupFilter;
            string groupAttribute;

            string firstNameAttribute = Constants.ADSchemaAttributes.FirstName;
            string secondNameAttribute = Constants.ADSchemaAttributes.Surname;
            string mailAttribute = Constants.ADSchemaAttributes.Mail;
            string titleAttribute = Constants.ADSchemaAttributes.Title;
            string mobilePhoneAttribute = Constants.ADSchemaAttributes.Mobile;
            string locationAttribute = Constants.ADSchemaAttributes.Street;
            string groupNameAttribute = Constants.ADSchemaAttributes.CommonName;

            if (!WorkContext.IsMono)
            {
                try
                {
                    var domain = Domain.GetCurrentDomain();
                    domainName = String.Format(@"LDAP://{0}", domain.Name);
                }
                catch
                {
                    domainName = null;
                }
                userFilter = "(" + Constants.ADSchemaAttributes.UserPrincipalName + "=*)";
                loginAttribute = Constants.ADSchemaAttributes.AccountName;
                groupFilter = "(" + Constants.ADSchemaAttributes.ObjectClass + "=" + Constants.ObjectClassKnowedValues.Group + ")";
                userAttribute = Constants.ADSchemaAttributes.DistinguishedName;
                groupAttribute = Constants.ADSchemaAttributes.Member;
            }
            else
            {
                userFilter = "(" + Constants.RFCLDAPAttributes.Uid + "=*)";
                loginAttribute = Constants.RFCLDAPAttributes.Uid;
                groupFilter = "(" + Constants.ADSchemaAttributes.ObjectClass + "=" + Constants.ObjectClassKnowedValues.PosixGroup + ")";
                userAttribute = Constants.RFCLDAPAttributes.Uid;
                groupAttribute = Constants.RFCLDAPAttributes.MemberUId;
            }

            string distinguishedName = ldapHelper.GetDefaultDistinguishedName(domainName, Constants.STANDART_LDAP_PORT);

            return new LDAPSupportSettings
            {
                Server = domainName,
                UserDN = distinguishedName,
                PortNumber = Constants.STANDART_LDAP_PORT,
                UserFilter = userFilter,
                LoginAttribute = loginAttribute,
                FirstNameAttribute = firstNameAttribute,
                SecondNameAttribute = secondNameAttribute,
                MailAttribute = mailAttribute,
                TitleAttribute = titleAttribute,
                MobilePhoneAttribute = mobilePhoneAttribute,
                LocationAttribute = locationAttribute,
                GroupDN = distinguishedName,
                GroupFilter = groupFilter,
                UserAttribute = userAttribute,
                GroupAttribute = groupAttribute,
                GroupNameAttribute = groupNameAttribute,
                Authentication = distinguishedName == null,
            };
        }

        public override bool Equals(object obj)
        {
            LDAPSupportSettings settings = obj as LDAPSupportSettings;
            if (settings == null ||
                EnableLdapAuthentication != settings.EnableLdapAuthentication ||
                StartTls != settings.StartTls ||
                Server != settings.Server ||
                UserDN != settings.UserDN ||
                PortNumber != settings.PortNumber ||
                UserFilter != settings.UserFilter ||
                LoginAttribute != settings.LoginAttribute ||
                FirstNameAttribute != settings.FirstNameAttribute ||
                SecondNameAttribute != settings.SecondNameAttribute ||
                MailAttribute != settings.MailAttribute ||
                TitleAttribute != settings.TitleAttribute ||
                MobilePhoneAttribute != settings.MobilePhoneAttribute ||
                LocationAttribute != settings.LocationAttribute ||
                GroupMembership != settings.GroupMembership ||
                GroupDN != settings.GroupDN ||
                GroupNameAttribute != settings.GroupNameAttribute ||
                GroupFilter != settings.GroupFilter ||
                UserAttribute != settings.UserAttribute ||
                GroupAttribute != settings.GroupAttribute ||
                Login != settings.Login)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 3;
            hash = (hash * 2) + EnableLdapAuthentication.GetHashCode();
            hash = (hash * 2) + StartTls.GetHashCode();
            hash = (hash * 2) + Server.GetHashCode();
            hash = (hash * 2) + UserDN.GetHashCode();
            hash = (hash * 2) + PortNumber.GetHashCode();
            hash = (hash * 2) + UserFilter.GetHashCode();
            hash = (hash * 2) + LoginAttribute.GetHashCode();
            hash = (hash * 2) + FirstNameAttribute.GetHashCode();
            hash = (hash * 2) + SecondNameAttribute.GetHashCode();
            hash = (hash * 2) + MailAttribute.GetHashCode();
            hash = (hash * 2) + TitleAttribute.GetHashCode();
            hash = (hash * 2) + MobilePhoneAttribute.GetHashCode();
            hash = (hash * 2) + LocationAttribute.GetHashCode();
            hash = (hash * 2) + GroupMembership.GetHashCode();
            hash = (hash * 2) + GroupDN.GetHashCode();
            hash = (hash * 2) + GroupNameAttribute.GetHashCode();
            hash = (hash * 2) + GroupFilter.GetHashCode();
            hash = (hash * 2) + UserAttribute.GetHashCode();
            hash = (hash * 2) + GroupAttribute.GetHashCode();
            hash = (hash * 2) + Authentication.GetHashCode();
            hash = (hash * 2) + Login.GetHashCode();
            return hash;
        }

        [DataMember]
        public bool EnableLdapAuthentication
        {
            get;
            set;
        }

        [DataMember]
        public bool StartTls
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
        public string LoginAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string FirstNameAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string SecondNameAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string MailAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string TitleAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string MobilePhoneAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string LocationAttribute
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
        public string GroupNameAttribute
        {
            get;
            set;
        }

        [DataMember]
        public string GroupFilter
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

        [DataMember]
        public bool IsDefault
        {
            get;
            set;
        }
    }
}
