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


using System;
using System.Collections.Generic;
using System.Linq;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Novell.Extensions;
using ASC.Common.Logging;
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

            _log = LogManager.GetLogger("ASC");

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
        public sealed override object GetValue(string propertyName, bool getBytes = false)
        {
            return _ldapEntry.GetAttributeValue(propertyName, getBytes);
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
