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
using log4net;
using System;
using System.Collections.Generic;

namespace ASC.ActiveDirectory.DirectoryServices
{
    public abstract class LdapHelper
    {
        protected readonly ILog Log = LogManager.GetLogger(typeof(LdapHelper));

        public abstract  LDAPObject GetDomain(LDAPSupportSettings settings);

        public abstract string GetDefaultDistinguishedName(string server, int portNumber);

        public abstract List<LDAPObject> GetUsersByAttributes(LDAPSupportSettings settings);

        public abstract void CheckCredentials(string login, string password, string server, int portNumber, bool startTls);

        public abstract bool CheckUserDN(string userDN, string server, int portNumber,
            bool authentication, string login, string password, bool startTls);

        public abstract List<LDAPObject> GetUsersByAttributesAndFilter(LDAPSupportSettings settings, string filter);

        public abstract List<LDAPObject> GetUsersFromPrimaryGroup(LDAPSupportSettings settings, string primaryGroupId);

        public abstract LDAPObject GetUserBySid(LDAPSupportSettings settings, string sid);

        public abstract bool CheckGroupDN(string groupDN, string server, int portNumber,
            bool authentication, string login, string password, bool startTls);

        public abstract List<LDAPObject> GetGroupsByAttributes(LDAPSupportSettings settings);

        public bool UserExistsInGroup(LDAPObject domainGroup, string memberString, string groupAttribute)
        {
            try
            {
                if (domainGroup == null ||
                    string.IsNullOrEmpty(memberString) ||
                    string.IsNullOrEmpty(groupAttribute))
                {
                    return false;
                }

                var members = domainGroup.GetValues(groupAttribute);
                if (members == null)
                    return false;

                if (members.Any(member => memberString.Equals(member, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Wrong Group Attribute parameters: {0}. {1}", groupAttribute, e);
            }
            return false;
        }

        public string GetUserAttribute(LDAPObject user, string userAttribute)
        {
            try
            {
                var member = user.InvokeGet(userAttribute);
                if (member != null)
                {
                    return member.ToString();
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Wrong  User Attribute parameters: {0}. {1}", userAttribute, e);
            }
            return null;
        }

        public List<string> GetGroupAttribute(LDAPObject domainGroup, string groupAttribute)
        {
            try
            {
                return domainGroup.GetValues(groupAttribute);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Wrong Group Attribute parameters: {0}. {1}", groupAttribute, e);
            }
            return null;
        }
    }
}
