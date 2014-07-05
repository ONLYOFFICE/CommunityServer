/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using ASC.ActiveDirectory.Expressions;
using ASC.Security.Cryptography;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Hosting;
using LDAPProtocols = System.DirectoryServices.Protocols;

namespace ASC.ActiveDirectory
{
	/// <summary>
	
	/// </summary>
	public sealed class ADDomain 
    {
		private static ILog _log = LogManager.GetLogger(typeof(ADDomain));
        public const byte OPERATION_OK = 0;
        public const byte WRONG_SERVER_OR_PORT = 1;
        public const byte WRONG_USER_DN = 2;
        public const byte INCORRECT_LDAP_FILTER = 3;
        public const byte USERS_NOT_FOUND = 4;
        public const byte WRONG_LOGIN_ATTRIBUTE = 5;
        public const byte WRONG_GROUP_DN_OR_GROUP_NAME = 6;
        public const byte GROUPS_NOT_FOUND = 7;
        public const byte WRONG_GROUP_ATTRIBUTE = 8;
        public const byte WRONG_USER_ATTRIBUTE = 9;
        public const byte CREDENTIALS_NOT_VALID = 10;
        
		/// <summary>
		
		/// </summary>
        public static LDAPDomain GetDomain(LDAPSupportSettings settings)
        {
            try
            {
                string password;
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
                try
                {
                    password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
                }
                catch (Exception)
                {
                    password = string.Empty;
                }
                if (settings.PortNumber == Constants.SSL_LDAP_PORT)
                {
                    type |= AuthenticationTypes.SecureSocketsLayer;
                }
                var entry = settings.Authentication ?
                    new DirectoryEntry(settings.Server + ":" + settings.PortNumber, settings.Login, password, type) :
                    new DirectoryEntry(settings.Server + ":" + settings.PortNumber);

                return (new LDAPObjectFactory()).CreateObject(entry) as LDAPDomain;
            }
            catch (Exception e)
            {
                _log.WarnFormat("Can't get current domain. May be current user has not needed permissions. {0}", e);
                return null;
            }
		}


		#region общие методы

        internal static string GetDefaultDistinguishedName(string server, int portNumber)
        {
            try
            {
                using (HostingEnvironment.Impersonate())
                {
                    return new DirectoryEntry(server + ":" + portNumber).InvokeGet(Constants.ADSchemaAttributes.DistinguishedName).ToString();
                }
            }
            catch (Exception e)
            {
                _log.WarnFormat("Can't get Domain DistinguishedName. May be current user has not needed permissions. {0}", e);
                return string.Empty;
            }
        }

		internal static List<LDAPObject> SearchInternal(string root, string criteria, SearchScope scope, LDAPSupportSettings settings)
        {
			_log.InfoFormat("ADDomain.Search(root: \"{0}\", criteria: \"{1}\", scope: {2})",
                root, criteria, scope);

			List<DirectoryEntry> entries = ADHelper.Search(root, criteria, scope, settings);
            if (entries == null)
            {
                entries = new List<DirectoryEntry>(0);
            }
			return new LDAPObjectFactory().CreateObjects(entries);
		}

        public static List<LDAPObject> Search(string root, Criteria criteria, LDAPSupportSettings settings)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            var entry = settings.Authentication ? new DirectoryEntry(root, settings.Login, password, type) : new DirectoryEntry(root);
            try
            {
                object nativeObject = entry.NativeObject;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
                return null;
            }
            return SearchInternal(root, criteria != null ? criteria.ToString() : null, SearchScope.Subtree, settings);
        }

        public static List<LDAPObject> Search(string root, Criteria criteria, string userFilter, LDAPSupportSettings settings)
        {
            var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch (Exception)
            {
                password = string.Empty;
            }
            if (settings.PortNumber == Constants.SSL_LDAP_PORT)
            {
                type |= AuthenticationTypes.SecureSocketsLayer;
            }
            var entry = settings.Authentication ? new DirectoryEntry(root, settings.Login, password, type) : new DirectoryEntry(root);
            try
            {
                object nativeObject = entry.NativeObject;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Error authenticating user. Current user has not access to read this directory: {0}. {1}", root, e);
                return new List<LDAPObject>(0);
            }

            if (!string.IsNullOrEmpty(userFilter) && !userFilter.StartsWith("(") && !userFilter.EndsWith(")"))
            {
                userFilter = "(" + userFilter + ")";
            }

            return SearchInternal(root, criteria != null ? "(&" + criteria.ToString() + userFilter + ")" : userFilter, SearchScope.Subtree, settings);
        }

		#endregion

		#region пользователи

        public static List<LDAPUser> GetUsersByAttributes(LDAPSupportSettings settings)
        {
            var dn = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            try
            {
                return Search(dn, Criteria.All(Expression.Exists(settings.BindAttribute),
                    Expression.Exists(settings.LoginAttribute)), settings.UserFilter, settings).
                    ConvertAll<LDAPUser>((LDAPObject obj) => obj as LDAPUser).Where(u => u != null).ToList();
            }
            catch (ArgumentException)
            {
                throw new ArgumentException();
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not access to directory: {0}. {1}", dn, e);
            }
            return null;
        }

        public static List<LDAPUser> GetUsersByAttributesAndFilter(LDAPSupportSettings settings, string filter)
        {
            var dn = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            if (!string.IsNullOrEmpty(settings.UserFilter) && !settings.UserFilter.StartsWith("(") && !settings.UserFilter.EndsWith(")"))
            {
                settings.UserFilter = "(" + settings.UserFilter + ")";
            }
            filter = "(&" + settings.UserFilter + filter + ")";
            try
            {
                return Search(dn, Criteria.All(Expression.Exists(settings.BindAttribute)), filter, settings).
                    ConvertAll<LDAPUser>((LDAPObject obj) => obj as LDAPUser).Where(u => u != null).ToList();
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not access to directory: {0}. {1}", dn, e);
            }
            return null;
        }

        public static List<LDAPUser> GetUsersFromPrimaryGroup(LDAPSupportSettings settings, string primaryGroupID)
        {
            var dn = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            try
            {
                return Search(dn, Criteria.All(Expression.Exists(settings.BindAttribute)).Add(Criteria.All(
                    Expression.Equal(Constants.ADSchemaAttributes.PrimaryGroupID, primaryGroupID))), settings.UserFilter, settings).
                    ConvertAll<LDAPUser>((LDAPObject obj) => obj as LDAPUser).Where(g => g != null).ToList();
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not access to directory: {0}. {1}", dn, e);
            }
            return null;
        }

        public static LDAPUser GetUserBySid(LDAPSupportSettings settings, string sid)
        {
            var dn = settings.Server + ":" + settings.PortNumber + "/" + settings.UserDN;
            try
            {
                var list = Search(dn, Criteria.All(Expression.Exists(settings.BindAttribute)).Add(Criteria.All(
                    Expression.Equal(Constants.ADSchemaAttributes.ObjectSid, sid))), settings.UserFilter, settings).
                    ConvertAll<LDAPUser>((LDAPObject obj) => obj as LDAPUser).Where(u => u != null).ToList();
                if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    return list[0];
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not access to directory: {0}. {1}", dn, e);
            }
            return null;
        }

		#endregion

		#region группы

        readonly static Criteria _AllGroupCriteria = Criteria.All(LDAPExpr.ObjClass(Constants.ObjectClassKnowedValues.Group));

        public static List<LDAPGroup> GetGroupsByParameter(LDAPSupportSettings settings)
        {
            try
            {
                var groupDNs = new List<string>();
                var groups = new List<LDAPGroup>();

                if (settings.GroupName == string.Empty)
                {
                    groupDNs.Add(settings.GroupDN);
                }
                else
                {
                    var names = settings.GroupName.Split(';');
                    for (int i = 0; i < names.Length; i++)
                    {
                        groupDNs.Add(names[i] + "," + settings.GroupDN);
                    }
                }
                for (int i = 0; i < groupDNs.Count; i++)
                {
                    groups.AddRange(Search(settings.Server + ":" + settings.PortNumber + "/" + groupDNs[i],
                        _AllGroupCriteria, settings).ConvertAll<LDAPGroup>((LDAPObject obj) => obj as LDAPGroup).Where(g => g != null).ToList());
                }
                return groups;
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Bad GroupDN or GroupName parameter. {0}", e);
            }
            return null;
        }


		#endregion

        public static void LogError(string message)
        {
            _log.Error(message);
        }

        public static byte CheckSettings(LDAPSupportSettings settings, LDAPUserImporter importer)
        {
            if (!settings.EnableLdapAuthentication)
            {
                return OPERATION_OK;
            }
            string password;
            try
            {
                password = new UnicodeEncoding().GetString(InstanceCrypto.Decrypt(settings.PasswordBytes));
            }
            catch
            {
                password = string.Empty;
            }
            try
            {
                if (settings.Authentication)
                {
                    CheckCredentials(settings.Login, password, settings.Server, settings.PortNumber);
                }
                if (!CheckServerAndPort(settings.Server, settings.PortNumber,
                        settings.Authentication, settings.Login, password))
                {
                    return WRONG_SERVER_OR_PORT;
                }
            }
            catch (DirectoryServicesCOMException)
            {
                return CREDENTIALS_NOT_VALID;
            }
            catch (COMException)
            {
                return WRONG_SERVER_OR_PORT;
            }
            
            if (!CheckUserDN(settings.UserDN, settings.Server, settings.PortNumber,
                    settings.Authentication, settings.Login, password))
            {
                return WRONG_USER_DN;
            }
            try
            {
                importer.AllDomainUsers = GetUsersByAttributes(settings);
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("Incorrect filter. userFilter = {0}", settings.UserFilter);
                return INCORRECT_LDAP_FILTER;
            }
            if (importer.AllDomainUsers == null || importer.AllDomainUsers.Count == 0)
            {
                _log.ErrorFormat("Any user is not found. userDN = {0}", settings.UserDN);
                return USERS_NOT_FOUND;
            }
            if (!CheckLoginAttribute(importer.AllDomainUsers[0], settings.LoginAttribute))
            {
                return WRONG_LOGIN_ATTRIBUTE;
            }
            if (settings.GroupMembership)
            {
                if (!CheckGroupDNAndGroupName(settings.GroupDN, settings.GroupName, settings.Server,
                    settings.PortNumber, settings.Authentication, settings.Login, password))
                {
                    return WRONG_GROUP_DN_OR_GROUP_NAME;
                }

                importer.DomainGroups = GetGroupsByParameter(settings);
                if (importer.DomainGroups == null || importer.DomainGroups.Count == 0)
                {
                    return GROUPS_NOT_FOUND;
                }

                if (!CheckGroupAttribute(importer.DomainGroups[0], settings.GroupAttribute))
                {
                    return WRONG_GROUP_ATTRIBUTE;
                }
                if (!CheckUserAttribute(importer.AllDomainUsers[0], settings.UserAttribute))
                {
                    return WRONG_USER_ATTRIBUTE;
                }
            }
            return OPERATION_OK;
        }

        private static bool CheckUserAttribute(LDAPUser user, string userAttr)
        {
            try
            {
                var userAttribute = user.InvokeGet(userAttr);
                if (userAttribute == null || string.IsNullOrWhiteSpace(userAttribute.ToString()))
                {
                    _log.ErrorFormat("Wrong Group Attribute parameter: {0}", userAttr);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong Group Attribute parameter: {0}. {1}", userAttr, e);
                return false;
            }
            return true;
        }

        private static bool CheckGroupAttribute(LDAPGroup group, string groupAttr)
        {
            try
            {
                var groupAttribute = group.GetValues(groupAttr);
                if (groupAttribute == null)
                {
                    _log.ErrorFormat("Wrong Group Attribute parameter: {0}", groupAttr);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong Group Attribute parameter: {0}. {1}", groupAttr, e);
                return false;
            }
            return true;
        }

        private static bool CheckGroupDNAndGroupName(string groupDN, string groupName, 
            string server, int portNumber, bool authentication, string login, string password)
        {
            try
            {
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
                if (portNumber == Constants.SSL_LDAP_PORT)
                {
                    type |= AuthenticationTypes.SecureSocketsLayer;
                }
                var groupDNs = new List<string>();
                var groups = new List<LDAPGroup>();

                if (groupName == string.Empty)
                {
                    groupDNs.Add(groupDN);
                }
                else
                {
                    var names = groupName.Split(';');
                    for (int i = 0; i < names.Length; i++)
                    {
                        groupDNs.Add(names[i] + "," + groupDN);
                    }
                }
                for (int i = 0; i < groupDNs.Count; i++)
                {
                    var groupEntry = authentication ?
                    new DirectoryEntry(server + ":" + portNumber + "/" + groupDNs[i], login, password, type) :
                    new DirectoryEntry(server + ":" + portNumber + "/" + groupDNs[i]);
                    if (string.IsNullOrEmpty(groupEntry.SchemaClassName))
                    {
                        _log.ErrorFormat("Wrong Group DN parameter: {0}", groupDNs[i]);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong Group DN or Group Names parameters: {0}. {1}. {2}", groupDN, groupName, e);
                return false;
            }
            return true;
        }

        private static bool CheckUserDN(string userDN, string server, int portNumber,
            bool authentication, string login, string password)
        {
            try
            {
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
                if (portNumber == Constants.SSL_LDAP_PORT)
                {
                    type |= AuthenticationTypes.SecureSocketsLayer;
                }
                var userEntry = authentication ?
                    new DirectoryEntry(server + ":" + portNumber + "/" + userDN, login, password, type) :
                    new DirectoryEntry(server + ":" + portNumber + "/" + userDN);
                if (userEntry.SchemaClassName == string.Empty)
                {
                    _log.ErrorFormat("Wrong User DN parameter: {0}", userDN);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong User DN parameter: {0}. {1}", userDN, e);
                return false;
            }
            return true;
        }

        private static bool CheckServerAndPort(string server, int portNumber,
            bool authentication, string login, string password)
        {
            try
            {
                var type = AuthenticationTypes.ReadonlyServer | AuthenticationTypes.Secure;
                if (portNumber == Constants.SSL_LDAP_PORT)
                {
                    type |= AuthenticationTypes.SecureSocketsLayer;
                }
                var rootEntry = authentication ?
                    new DirectoryEntry(server + ":" + portNumber, login, password, type) :
                    new DirectoryEntry(server + ":" + portNumber);
                if (rootEntry.SchemaClassName != Constants.ObjectClassKnowedValues.DomainDNS)
                {
                    _log.ErrorFormat("Wrong Server Address or Port: {0}:{1}", server, portNumber);
                    return false;
                }
            }
            catch (DirectoryServicesCOMException e)
            {
                _log.ErrorFormat("Wrong login or password: {0}:{1}. {2}", login, password, e);
                throw new DirectoryServicesCOMException(e.Message);
            }
            catch (COMException e)
            {
                _log.ErrorFormat("Wrong Server Address or Port: {0}:{1}. {2}", server, portNumber, e);
                throw new COMException(e.Message);
            }
            return true;
        }

        public static bool CheckLoginAttribute(LDAPUser user, string loginAttribute)
        {
            string memberUser;
            try
            {
                var member = user.InvokeGet(loginAttribute);
                memberUser = member != null ? member.ToString() : null;
                if (string.IsNullOrWhiteSpace(memberUser))
                {
                    _log.ErrorFormat("Wrong Login Attribute parameter: {0}", memberUser);
                    return false;
                }
            }
            catch (Exception e)
            {
                memberUser = null;
                _log.ErrorFormat("Wrong Login Attribute parameter: {0}. {1}", memberUser, e);
                return false;
            }
            return true;
        }

        public static bool UserExistsInGroup(LDAPGroup domainGroup, string memberString, string groupAttribute)
        {
            try
            {
                var members = domainGroup.GetValues(groupAttribute);
                if (memberString != null)
                {
                    foreach (var member in members)
                    {
                        if (memberString.ToString().Equals(member.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong Group Attribute parameters: {0}. {1}", groupAttribute, e);
            }
            return false;
        }

        public static PropertyValueCollection GetGroupAttribute(LDAPGroup domainGroup, string groupAttribute)
        {
            try
            {
                return domainGroup.GetValues(groupAttribute);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Wrong Group Attribute parameters: {0}. {1}", groupAttribute, e);
            }
            return null;
        }

        public static string GetUserAttribute(LDAPUser user, string userAttribute)
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
                _log.ErrorFormat("Wrong  User Attribute parameters: {0}. {1}", userAttribute, e);
            }
            return null;
        }

        public static void CheckCredentials(string login, string password, string server, int portNumber)
        {
            try
            {
                var domainName = server.Split('/').Last() + ":" + portNumber;
                // if login with domain
                login = login.Split('@')[0];
                using (var ldap = new LDAPProtocols.LdapConnection(domainName))
                {
                    var networkCredential = new NetworkCredential(login, password, domainName);
                    ldap.SessionOptions.VerifyServerCertificate = new LDAPProtocols.VerifyServerCertificateCallback((con, cer) => true);
                    ldap.SessionOptions.SecureSocketLayer = (portNumber == Constants.SSL_LDAP_PORT);
                    ldap.SessionOptions.ProtocolVersion = 3;
                    ldap.AuthType = LDAPProtocols.AuthType.Negotiate;
                    ldap.Bind(networkCredential);
                }
            }
            catch (LDAPProtocols.LdapException e)
            {
                if (!e.ErrorCode.Equals(Constants.LDAP_ERROR_INVALID_CREDENTIALS))
                {
                    _log.ErrorFormat("Internal LDAP authentication error: {0}.", e);
                    throw new COMException();
                }
                throw new DirectoryServicesCOMException();
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Internal AD authentication error: {0}.", e);
                throw new COMException();
            }
        }
    }
}
