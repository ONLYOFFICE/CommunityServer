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
    /// LDAP object class
    /// </summary>
    public class LDAPObject
    {
        private readonly DirectoryEntry _directoryEntry;
        private readonly LdapEntry _ldapEntry;
        protected readonly ILog Log = LogManager.GetLogger(typeof(LDAPObject));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directoryEntry">init directory entry</param>
        public LDAPObject(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException("directoryEntry");

            _directoryEntry = directoryEntry;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ldapEntry">init ldap entry</param>
        public LDAPObject(LdapEntry ldapEntry)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            _ldapEntry = ldapEntry;
        }

        #region .Public

        public string DistinguishedName
        {
            get
            {
                if (_directoryEntry != null)
                {
                    return InvokeGet(Constants.ADSchemaAttributes.DISTINGUISHED_NAME) as string;
                }
                return _ldapEntry.DN;
            }
        }

        public string Sid
        {
            get
            {
                if (_directoryEntry != null)
                {
                    var sid = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
                    try
                    {
                        var binaryForm = InvokeGet(Constants.ADSchemaAttributes.OBJECT_SID) as byte[];
                        if (binaryForm != null)
                        {
                            sid = new SecurityIdentifier(binaryForm, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Can't get LDAPObject Sid property. {0}", e);
                    }
                    return sid.Value;
                }

                try
                {
                    string sid;
                    var ldapUniqueIdAttribute = ConfigurationManager.AppSettings["ldap.unique.id"];
                    if (ldapUniqueIdAttribute == null)
                    {
                        sid = InvokeGet(Constants.RfcLDAPAttributes.ENTRY_UUID) as string;
                        if (sid == null)
                        {
                            sid = InvokeGet(Constants.RfcLDAPAttributes.NS_UNIQUE_ID) as string;
                            if (sid == null)
                            {
                                sid = InvokeGet(Constants.RfcLDAPAttributes.GUID) as string;
                                if (sid == null)
                                {
                                    sid = InvokeGet(Constants.ADSchemaAttributes.OBJECT_SID) as string;
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
                    Log.ErrorFormat("Can't get LDAPObject Sid property. {0}", e);
                }
                return null;
            }
        }

        public bool IsDisabled
        {
            get
            {
                if (_directoryEntry == null)
                    return false;
                
                var userAccauntControl = Constants.UserAccountControl.EMPTY;
                try
                {
                    var uac = Convert.ToInt32(InvokeGet(Constants.ADSchemaAttributes.USER_ACCOUNT_CONTROL));
                    userAccauntControl = (Constants.UserAccountControl) uac;
                }
                catch (Exception e)
                {
                    Log.ErrorFormat("Can't get LDAPUser UserAccauntControl property. {0}", e);
                }

                return (userAccauntControl & Constants.UserAccountControl.ADS_UF_ACCOUNTDISABLE) > 0;
            }
        }

        #endregion

        /// <summary>
        /// Get property object
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>value object</returns>
        public object InvokeGet(string propertyName)
        {
            if (_directoryEntry == null)
                return _ldapEntry.GetAttributeValue(propertyName);

            using (HostingEnvironment.Impersonate())
            {
                return _directoryEntry.InvokeGet(propertyName);
            }
        }

        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>list of values</returns>
        public List<string> GetValues(string propertyName)
        {
            var properties = new List<string>();
            if (_directoryEntry != null)
            {
                var propertyValueCollection = _directoryEntry.Properties[propertyName];
                if (propertyValueCollection == null || propertyValueCollection.Value == null)
                {
                    return null;
                }

                properties.AddRange(from object val in propertyValueCollection select val.ToString());
            }
            else
            {
                var propertyValueArray = _ldapEntry.GetAttributeArrayValue(propertyName);
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
