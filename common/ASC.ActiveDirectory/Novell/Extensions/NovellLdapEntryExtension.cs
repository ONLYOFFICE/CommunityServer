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
using System.Globalization;
using System.Linq;
using System.Text;
using ASC.ActiveDirectory.Base;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Novell.Data;
using Novell.Directory.Ldap;

namespace ASC.ActiveDirectory.Novell.Extensions
{
    public static class NovellLdapEntryExtension
    {
        public static object GetAttributeValue(this LdapEntry ldapEntry, string attributeName, bool getBytes = false)
        {
            var attribute = ldapEntry.getAttribute(attributeName);

            if (attribute == null)
                return null;

            if (!(string.Equals(attributeName, LdapConstants.ADSchemaAttributes.OBJECT_SID,
                StringComparison.OrdinalIgnoreCase) || getBytes))
            {
                return attribute.StringValue;
            }

            if (attribute.ByteValue == null)
                return null;

            var value = new byte[attribute.ByteValue.Length];

            Buffer.BlockCopy(attribute.ByteValue, 0, value, 0, attribute.ByteValue.Length);

            if (getBytes)
            {
                return value;
            }

            return DecodeSid(value);
        }

        public static string[] GetAttributeArrayValue(this LdapEntry ldapEntry, string attributeName)
        {
            var attribute = ldapEntry.getAttribute(attributeName);
            return attribute == null ? null : attribute.StringValueArray;
        }

        private static string DecodeSid(byte[] sid)
        {
            var strSid = new StringBuilder("S-");

            // get version
            int revision = sid[0];
            strSid.Append(revision.ToString(CultureInfo.InvariantCulture));

            //next byte is the count of sub-authorities
            var countSubAuths = sid[1] & 0xFF;

            //get the authority
            long authority = 0;

            //String rid = "";
            for (var i = 2; i <= 7; i++)
            {
                authority |= ((long)sid[i]) << (8 * (5 - (i - 2)));
            }

            strSid.Append("-");
            strSid.Append(authority);

            //iterate all the sub-auths
            var offset = 8;
            const int size = 4; //4 bytes for each sub auth

            for (var j = 0; j < countSubAuths; j++)
            {
                long subAuthority = 0;
                for (var k = 0; k < size; k++)
                {
                    subAuthority |= (long)(sid[offset + k] & 0xFF) << (8 * k);
                }

                strSid.Append("-");
                strSid.Append(subAuthority);

                offset += size;
            }

            return strSid.ToString();
        }

        /// <summary>
        /// Create LDAPObject by LdapEntry
        /// </summary>
        /// <param name="ldapEntry">init ldapEntry</param>
        /// <param name="ldapUniqueIdAttribute"></param>
        /// <returns>LDAPObject</returns>
        public static LdapObject ToLdapObject(this LdapEntry ldapEntry, string ldapUniqueIdAttribute = null)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            return new NovellLdapObject(ldapEntry, ldapUniqueIdAttribute);
        }

        /// <summary>
        /// Create lis of LDAPObject by LdapEntry list
        /// </summary>
        /// <param name="entries">list of LdapEntry</param>
        /// <param name="ldapUniqueIdAttribute"></param>
        /// <returns>list of LDAPObjects</returns>
        public static List<LdapObject> ToLdapObjects(this IEnumerable<LdapEntry> entries, string ldapUniqueIdAttribute = null)
        {
            return entries.Select(e => ToLdapObject(e, ldapUniqueIdAttribute)).ToList();
        }
    }
}
