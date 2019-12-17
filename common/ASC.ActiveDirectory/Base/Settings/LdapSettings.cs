/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Core;
using ASC.Core.Common.Settings;
using ASC.Web.Core;

namespace ASC.ActiveDirectory.Base.Settings
{
    [Serializable]
    [DataContract]
    public class LdapSettings : BaseSettings<LdapSettings>, ICloneable
    {
        public override Guid ID
        {
            get { return new Guid("{197149b3-fbc9-44c2-b42a-232f7e729c16}"); }
        }

        public LdapSettings()
        {
            LdapMapping = new Dictionary<MappingFields, string>();
            AccessRights = new Dictionary<AccessRight, string>();
        }

        public enum MappingFields
        {
            FirstNameAttribute,
            SecondNameAttribute,
            BirthDayAttribute,
            GenderAttribute,
            MobilePhoneAttribute,
            MailAttribute,
            TitleAttribute,
            LocationAttribute,
            AvatarAttribute,

            AdditionalPhone,
            AdditionalMobilePhone,
            AdditionalMail,
            Skype
        }

        public enum AccessRight
        {
            FullAccess,
            Documents,
            Projects,
            CRM,
            Community,
            People,
            Mail
        }

        public static readonly Dictionary<AccessRight, Guid> AccessRightsGuids = new Dictionary<AccessRight, Guid>()
        {
            { AccessRight.FullAccess, Guid.Empty },
            { AccessRight.Documents, WebItemManager.DocumentsProductID },
            { AccessRight.Projects, WebItemManager.ProjectsProductID },
            { AccessRight.CRM, WebItemManager.CRMProductID },
            { AccessRight.Community, WebItemManager.CommunityProductID },
            { AccessRight.People, WebItemManager.PeopleProductID },
            { AccessRight.Mail, WebItemManager.MailProductID }
        };

        public override ISettings GetDefault()
        {
            var isMono = WorkContext.IsMono;

            var settings = new LdapSettings
            {
                Server = "",
                UserDN = "",
                PortNumber = LdapConstants.STANDART_LDAP_PORT,
                UserFilter = string.Format("({0}=*)",
                    isMono
                        ? LdapConstants.RfcLDAPAttributes.UID
                        : LdapConstants.ADSchemaAttributes.USER_PRINCIPAL_NAME),
                LoginAttribute = isMono
                    ? LdapConstants.RfcLDAPAttributes.UID
                    : LdapConstants.ADSchemaAttributes.ACCOUNT_NAME,
                FirstNameAttribute = LdapConstants.ADSchemaAttributes.FIRST_NAME,
                SecondNameAttribute = LdapConstants.ADSchemaAttributes.SURNAME,
                MailAttribute = LdapConstants.ADSchemaAttributes.MAIL,
                TitleAttribute = LdapConstants.ADSchemaAttributes.TITLE,
                MobilePhoneAttribute = LdapConstants.ADSchemaAttributes.MOBILE,
                LocationAttribute = LdapConstants.ADSchemaAttributes.STREET,
                GroupDN = "",
                GroupFilter = string.Format("({0}={1})", LdapConstants.ADSchemaAttributes.OBJECT_CLASS,
                    isMono
                        ? LdapConstants.ObjectClassKnowedValues.POSIX_GROUP
                        : LdapConstants.ObjectClassKnowedValues.GROUP),
                UserAttribute =
                    isMono
                        ? LdapConstants.RfcLDAPAttributes.UID
                        : LdapConstants.ADSchemaAttributes.DISTINGUISHED_NAME,
                GroupAttribute = isMono ? LdapConstants.RfcLDAPAttributes.MEMBER_UID : LdapConstants.ADSchemaAttributes.MEMBER,
                GroupNameAttribute = LdapConstants.ADSchemaAttributes.COMMON_NAME,
                Authentication = true,
                AcceptCertificate = false,
                AcceptCertificateHash = null,
                StartTls = false,
                Ssl = false,
                SendWelcomeEmail = false
            };

            return settings;
        }

        public static List<MappingFields> GetImportedFields { get { return Load().LdapMapping.Keys.ToList(); } }

        public override bool Equals(object obj)
        {
            var settings = obj as LdapSettings;

            return settings != null
                   && EnableLdapAuthentication == settings.EnableLdapAuthentication
                   && StartTls == settings.StartTls
                   && Ssl == settings.Ssl
                   && SendWelcomeEmail == settings.SendWelcomeEmail
                   && (string.IsNullOrEmpty(Server)
                       && string.IsNullOrEmpty(settings.Server)
                       || Server == settings.Server)
                   && (string.IsNullOrEmpty(UserDN)
                       && string.IsNullOrEmpty(settings.UserDN)
                       || UserDN == settings.UserDN)
                   && PortNumber == settings.PortNumber
                   && UserFilter == settings.UserFilter
                   && LoginAttribute == settings.LoginAttribute
                   && LdapMapping.Count == settings.LdapMapping.Count
                   && LdapMapping.All(pair => settings.LdapMapping.ContainsKey(pair.Key)
                       && pair.Value == settings.LdapMapping[pair.Key])
                   && AccessRights.Count == settings.AccessRights.Count
                   && AccessRights.All(pair => settings.AccessRights.ContainsKey(pair.Key)
                       && pair.Value == settings.AccessRights[pair.Key])
                   && GroupMembership == settings.GroupMembership
                   && (string.IsNullOrEmpty(GroupDN)
                       && string.IsNullOrEmpty(settings.GroupDN)
                       || GroupDN == settings.GroupDN)
                   && GroupFilter == settings.GroupFilter
                   && UserAttribute == settings.UserAttribute
                   && GroupAttribute == settings.GroupAttribute
                   && (string.IsNullOrEmpty(Login)
                       && string.IsNullOrEmpty(settings.Login)
                       || Login == settings.Login)
                   && Authentication == settings.Authentication;
        }

        public override int GetHashCode()
        {
            var hash = 3;
            hash = (hash * 2) + EnableLdapAuthentication.GetHashCode();
            hash = (hash * 2) + StartTls.GetHashCode();
            hash = (hash * 2) + Ssl.GetHashCode();
            hash = (hash * 2) + SendWelcomeEmail.GetHashCode();
            hash = (hash * 2) + Server.GetHashCode();
            hash = (hash * 2) + UserDN.GetHashCode();
            hash = (hash * 2) + PortNumber.GetHashCode();
            hash = (hash * 2) + UserFilter.GetHashCode();
            hash = (hash * 2) + LoginAttribute.GetHashCode();
            hash = (hash * 2) + GroupMembership.GetHashCode();
            hash = (hash * 2) + GroupDN.GetHashCode();
            hash = (hash * 2) + GroupNameAttribute.GetHashCode();
            hash = (hash * 2) + GroupFilter.GetHashCode();
            hash = (hash * 2) + UserAttribute.GetHashCode();
            hash = (hash * 2) + GroupAttribute.GetHashCode();
            hash = (hash * 2) + Authentication.GetHashCode();
            hash = (hash * 2) + Login.GetHashCode();

            foreach (var pair in LdapMapping)
                hash = (hash * 2) + pair.Value.GetHashCode();

            foreach (var pair in AccessRights)
                hash = (hash * 2) + pair.Value.GetHashCode();

            return hash;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        [DataMember]
        public bool EnableLdapAuthentication { get; set; }

        [DataMember]
        public bool StartTls { get; set; }

        [DataMember]
        public bool Ssl { get; set; }

        [DataMember]
        public bool SendWelcomeEmail { get; set; }

        [DataMember]
        public string Server { get; set; }

        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string UserDN { get; set; }

        [DataMember]
        public int PortNumber { get; set; }

        [DataMember]
        public string UserFilter { get; set; }

        [DataMember]
        public string LoginAttribute { get; set; }

        [DataMember]
        public Dictionary<MappingFields, string> LdapMapping { get; set; }
        
        //ToDo: use SId instead of group name
        [DataMember]
        public Dictionary<AccessRight, string> AccessRights { get; set; }

        [DataMember]
        public string FirstNameAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.FirstNameAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.FirstNameAttribute, value);
            }
        }

        [DataMember]
        public string SecondNameAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.SecondNameAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.SecondNameAttribute, value);
            }
        }

        [DataMember]
        public string MailAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.MailAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.MailAttribute, value);
            }
        }

        [DataMember]
        public string TitleAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.TitleAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.TitleAttribute, value);
            }
        }

        [DataMember]
        public string MobilePhoneAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.MobilePhoneAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.MobilePhoneAttribute, value);
            }
        }

        [DataMember]
        public string LocationAttribute
        {
            get
            {
                return GetOldSetting(MappingFields.LocationAttribute);
            }

            set
            {
                SetOldSetting(MappingFields.LocationAttribute, value);
            }
        }

        [DataMember]
        public bool GroupMembership { get; set; }

        [DataMember]
        // ReSharper disable once InconsistentNaming
        public string GroupDN { get; set; }

        [DataMember]
        public string GroupNameAttribute { get; set; }

        [DataMember]
        public string GroupFilter { get; set; }

        [DataMember]
        public string UserAttribute { get; set; }

        [DataMember]
        public string GroupAttribute { get; set; }

        [DataMember]
        public bool Authentication { get; set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public byte[] PasswordBytes { get; set; }

        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public bool AcceptCertificate { get; set; }

        [DataMember]
        public string AcceptCertificateHash { get; set; }

        private string GetOldSetting(MappingFields field)
        {
            if (LdapMapping == null)
                LdapMapping = new Dictionary<MappingFields, string>();

            if (LdapMapping.ContainsKey(field))
                return LdapMapping[field];
            else
                return "";
        }
        private void SetOldSetting(MappingFields field, string value)
        {
            if (LdapMapping == null)
                LdapMapping = new Dictionary<MappingFields, string>();

            if (string.IsNullOrEmpty(value))
            {
                if (LdapMapping.ContainsKey(field))
                {
                    LdapMapping.Remove(field);
                }
                return;
            }

            if (LdapMapping.ContainsKey(field))
            {
                LdapMapping[field] = value;
            }
            else
            {
                LdapMapping.Add(field, value);
            }
        }
    }

    [Serializable]
    [DataContract]
    public class LdapCronSettings : BaseSettings<LdapCronSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{58C42C54-56CD-4BEF-A3ED-C60ACCF6E975}"); }
        }

        public override ISettings GetDefault()
        {
            return new LdapCronSettings()
            {
                Cron = null
            };
        }

        [DataMember]
        public string Cron { get; set; }
    }

    [Serializable]
    [DataContract]
    public class LdapCurrentAcccessSettings : BaseSettings<LdapCurrentAcccessSettings>
    {
        public override Guid ID
        {
            get { return new Guid("{134B5EAA-F612-4834-AEAB-34C90515EA4E}"); }
        }

        public override ISettings GetDefault()
        {
            return new LdapCurrentAcccessSettings() { CurrentAccessRights = null };
        }

        public LdapCurrentAcccessSettings()
        {
            CurrentAccessRights = new Dictionary<LdapSettings.AccessRight, List<string>>();
        }

        [DataMember]
        public Dictionary<LdapSettings.AccessRight, List<string>> CurrentAccessRights { get; set; }
    }

    [Serializable]
    [DataContract]
    public class LdapCurrentUserPhotos : BaseSettings<LdapCurrentUserPhotos>
    {
        public override Guid ID
        {
            get { return new Guid("{50AE3C2B-0783-480F-AF30-679D0F0A2D3E}"); }
        }

        public override ISettings GetDefault()
        {
            return new LdapCurrentUserPhotos() { CurrentPhotos = null };
        }

        public LdapCurrentUserPhotos()
        {
            CurrentPhotos = new Dictionary<Guid, string>();
        }

        [DataMember]
        public Dictionary<Guid, string> CurrentPhotos { get; set; }
    }

    [Serializable]
    [DataContract]
    public class LdapCurrentDomain : BaseSettings<LdapCurrentDomain>
    {
        public override Guid ID
        {
            get { return new Guid("{75A5F745-F697-4418-B38D-0FE0D277E258}"); }
        }

        public override ISettings GetDefault()
        {
            return new LdapCurrentDomain() { CurrentDomain = null };
        }

        [DataMember]
        public string CurrentDomain { get; set; }
    }
}
