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
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Novell.Extensions;
using log4net;
using Novell.Directory.Ldap;

namespace ASC.ActiveDirectory.Novell.Data
{
    /// <summary>
    /// Novell LDAP object class
    /// </summary>
    public class NovellLdapObject: LdapObject
    {
        private readonly LdapEntry _ldapEntry;
        private readonly ILog _log;
        private readonly string _sid;
        private readonly string _sidAttribute;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ldapEntry">init ldap entry</param>
        /// <param name="ldapUniqueIdAttribute"></param>
        public NovellLdapObject(LdapEntry ldapEntry, string ldapUniqueIdAttribute = null)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            _ldapEntry = ldapEntry;

            if (string.IsNullOrEmpty(ldapUniqueIdAttribute))
                return;

            _log = LogManager.GetLogger(typeof(LdapObject));

            try
            {
                _sid = GetValue(ldapUniqueIdAttribute) as string;
                _sidAttribute = ldapUniqueIdAttribute;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can't get LDAPObject Sid property. {0}", e);
            }
        }

        #region .Public

        public override string DistinguishedName
        {
            get { return _ldapEntry.DN; }
        }

        public override string Sid
        {
            get { return _sid; }
        }

        public override string SidAttribute
        {
            get { return _sidAttribute; }
        }

        public override bool IsDisabled
        {
            get
            {               
                var userAccauntControl = LdapConstants.UserAccountControl.EMPTY;
                try
                {
                    var uac = Convert.ToInt32(GetValue(LdapConstants.ADSchemaAttributes.USER_ACCOUNT_CONTROL));
                    userAccauntControl = (LdapConstants.UserAccountControl) uac;
                }
                catch (Exception e)
                {
                    _log.ErrorFormat("Can't get LDAPUser UserAccauntControl property. {0}", e);
                }

                return (userAccauntControl & LdapConstants.UserAccountControl.ADS_UF_ACCOUNTDISABLE) > 0;
            }
        }

        #endregion

        /// <summary>
        /// Get property object
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>value object</returns>
        public sealed override object GetValue(string propertyName)
        {
            return _ldapEntry.GetAttributeValue(propertyName);
        }

        /// <summary>
        /// Get property values
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>list of values</returns>
        public override List<string> GetValues(string propertyName)
        {
            var propertyValueArray = _ldapEntry.GetAttributeArrayValue(propertyName);
            if (propertyValueArray == null)
            {
                return new List<string>();
            }

            var properties = propertyValueArray.ToList();
            return properties;
        }
    }
}
