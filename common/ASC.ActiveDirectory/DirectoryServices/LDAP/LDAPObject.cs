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


using ASC.ActiveDirectory.Novell;
using log4net;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Web.Hosting;

namespace ASC.ActiveDirectory
{
    /// <summary>
    /// Класс для объекта из службы каталогов при работе через LDAP
    /// </summary>
    /// <remarks>Предназначен для выполнения общей работы.
    public class LDAPObject
    {
        private readonly DirectoryEntry directoryEntry = null;
        private readonly LdapEntry ldapEntry = null;
        protected readonly ILog log = LogManager.GetLogger(typeof(LDAPObject));

        /// <summary>
        /// Внутренний конструктор
        /// </summary>
        /// <param name="directoryEntry">инициализурующая запись из службы сервисов</param>
        public LDAPObject(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException("directoryEntry");

            this.directoryEntry = directoryEntry;
        }

        /// <summary>
        /// Внутренний конструктор
        /// </summary>
        /// <param name="ldapEntry">инициализурующая запись из службы сервисов</param>
        public LDAPObject(LdapEntry ldapEntry)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            this.ldapEntry = ldapEntry;
        }

        #region публичные свойства

        public string DistinguishedName
        {
            get
            {
                if (directoryEntry != null)
                {
                    return InvokeGet(Constants.ADSchemaAttributes.DistinguishedName) as string;
                }
                else
                {
                    return ldapEntry.DN;
                }
            }
        }

        public string Sid
        {
            get
            {
                if (directoryEntry != null)
                {
                    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
                    try
                    {
                        byte[] binaryForm = InvokeGet(Constants.ADSchemaAttributes.ObjectSid) as byte[];
                        if (binaryForm != null)
                        {
                            sid = new SecurityIdentifier(binaryForm, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Can't get LDAPObject Sid property. {0}", e);
                    }
                    return sid.Value;
                }
                else
                {
                    try
                    {
                        string sid;
                        string ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];
                        if (ldapUniqueIdAttribute == null)
                        {
                            sid = InvokeGet(Constants.RFCLDAPAttributes.EntryUUID) as string;
                            if (sid == null)
                            {
                                sid = InvokeGet(Constants.RFCLDAPAttributes.NSUniqueId) as string;
                                if (sid == null)
                                {
                                    sid = InvokeGet(Constants.RFCLDAPAttributes.GUID) as string;
                                    if (sid == null)
                                    {
                                        sid = InvokeGet(Constants.ADSchemaAttributes.ObjectSid) as string;
                                    }
                                }
                            }
                        }
                        else
                        {
                            sid = InvokeGet(ldapUniqueIdAttribute) as string;
                        }
                        return sid;
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Can't get LDAPObject Sid property. {0}", e);
                    }
                    return null;
                }
            }
        }

        public bool IsDisabled
        {
            get
            {
                if (directoryEntry != null)
                {
                    Constants.UserAccauntControl userAccauntControl = Constants.UserAccauntControl.Empty;
                    try
                    {
                        int uac = Convert.ToInt32(InvokeGet(Constants.ADSchemaAttributes.UserAccountControl));
                        userAccauntControl = (Constants.UserAccauntControl)uac;
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat("Can't get LDAPUser UserAccauntControl property. {0}", e);
                    }
                    return (userAccauntControl & Constants.UserAccauntControl.ADS_UF_ACCOUNTDISABLE) > 0;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Получение значения указанного свойства
        /// </summary>
        /// <param name="propertyName">название свойства</param>
        /// <returns>его значение</returns>
        public object InvokeGet(string propertyName)
        {
            if (directoryEntry != null)
            {
                using (HostingEnvironment.Impersonate())
                {
                    return directoryEntry.InvokeGet(propertyName);
                }
            }
            else
            {
                return ldapEntry.GetAttributeValue(propertyName);
            }
        }

        /// <summary>
        /// Получение значений свойсва
        /// </summary>
        /// <param name="propertyName">название свойства</param>
        /// <returns>значения</returns>
        public List<string> GetValues(string propertyName)
        {
            var properties = new List<string>();
            if (directoryEntry != null)
            {
                PropertyValueCollection propertyValueCollection = directoryEntry.Properties[propertyName];
                if (propertyValueCollection == null || propertyValueCollection.Value == null)
                {
                    return null;
                }

                foreach (var val in propertyValueCollection)
                {
                    properties.Add(val.ToString());
                }
            }
            else
            {
                var propertyValueArray = ldapEntry.GetAttributeArrayValue(propertyName);
                if (propertyValueArray == null)
                {
                    return null;
                }
                properties = propertyValueArray.ToList();
            }
            return properties;
        }
    }
}
