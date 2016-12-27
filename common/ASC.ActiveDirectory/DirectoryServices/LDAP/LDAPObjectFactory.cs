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


using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.DirectoryServices;

namespace ASC.ActiveDirectory
{
    /// <summary>
    /// Фабрика для создания объектов LDAP
    /// </summary>
    public class LDAPObjectFactory
    {
        /// <summary>
        /// Создание конкретного объекта по DirectoryEntry
        /// </summary>
        /// <param name="directoryEntry"></param>
        /// <returns>конкретный объект</returns>
        public LDAPObject CreateObject(DirectoryEntry directoryEntry)
        {
            if (directoryEntry == null)
                throw new ArgumentNullException("directoryEntry");

            return new LDAPObject(directoryEntry);
       }

        /// <summary>
        /// Создание конкретного объекта по LdapEntry
        /// </summary>
        /// <param name="ldapEntry"></param>
        /// <returns>конкретный объект</returns>
        public LDAPObject CreateObject(LdapEntry ldapEntry)
        {
            if (ldapEntry == null)
                throw new ArgumentNullException("ldapEntry");

            return new LDAPObject(ldapEntry);
        }

        /// <summary>
        /// Создание списка конкретных объектов по списку DirectoryEntry
        /// </summary>
        /// <param name="entries">список DirectoryEntry</param>
        /// <returns>список конкретных объектов</returns>
        public List<LDAPObject> CreateObjects(IEnumerable<DirectoryEntry> entries)
        {
            List<LDAPObject> list = new List<LDAPObject>();
            foreach (var item in entries)
            {
                list.Add(CreateObject(item));
            }

            return list;
        }

        /// <summary>
        /// Создание списка конкретных объектов по списку DirectoryEntry
        /// </summary>
        /// <param name="entries">список LdapEntry</param>
        /// <returns>список конкретных объектов</returns>
        public List<LDAPObject> CreateObjects(IEnumerable<LdapEntry> entries)
        {
            List<LDAPObject> list = new List<LDAPObject>();
            foreach (var item in entries)
            {
                list.Add(CreateObject(item));
            }

            return list;
        }

        /// <summary>
        /// Создание списка конкретных объектов по DirectoryEntries
        /// </summary>
        /// <param name="entries">коллекция DirectoryEntries</param>
        /// <returns>список конкретных объектов</returns>
        public IList<LDAPObject> CreateObjects(DirectoryEntries entries)
        {
            List<LDAPObject> list = new List<LDAPObject>();
            foreach (DirectoryEntry item in entries)
            {
                list.Add(CreateObject(item));
            }
            return list;
        }
    }
}
