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


using System.Linq;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ASC.ActiveDirectory
{
    /// <summary>
    /// LDAP objects Factory Class
    /// </summary>
    public static class LDAPObjectFactory
    {
        /// <summary>
        /// Create LDAPObject by DirectoryEntry
        /// </summary>
        /// <param name="directoryEntry">init directoryEntry</param>
        /// <returns>LDAPObject</returns>
        public static LDAPObject CreateObject(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException("directoryEntry");

            return new LDAPObject(directoryEntry);
       }

        /// <summary>
        /// Create LDAPObject by LdapEntry
        /// </summary>
        /// <param name="ldapEntry">init ldapEntry</param>
        /// <returns>LDAPObject</returns>
        public static LDAPObject CreateObject(LdapEntry ldapEntry)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            return new LDAPObject(ldapEntry);
        }

        /// <summary>
        /// Create lis of LDAPObject by DirectoryEntry list
        /// </summary>
        /// <param name="entries">list of DirectoryEntry</param>
        /// <returns>list of LDAPObjects</returns>
        public static List<LDAPObject> CreateObjects(IEnumerable<DirectoryEntry> entries)
        {
            return entries.Select(CreateObject).ToList();
        }

        /// <summary>
        /// Create lis of LDAPObject by LdapEntry list
        /// </summary>
        /// <param name="entries">list of LdapEntry</param>
        /// <returns>list of LDAPObjects</returns>
        public static List<LDAPObject> CreateObjects(IEnumerable<LdapEntry> entries)
        {
            return entries.Select(CreateObject).ToList();
        }

        /// <summary>
        /// Create lis of LDAPObject by DirectoryEntries Collection
        /// </summary>
        /// <param name="entries">DirectoryEntries</param>
        /// <returns>list of LDAPObjects</returns>
        public static IList<LDAPObject> CreateObjects(DirectoryEntries entries)
        {
            return (from DirectoryEntry item in entries select CreateObject(item)).ToList();
        }
    }
}
