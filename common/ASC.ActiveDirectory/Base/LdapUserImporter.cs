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
using System.Configuration;
using System.Linq;
using ASC.ActiveDirectory.Base.Data;
using ASC.ActiveDirectory.Base.Expressions;
using ASC.ActiveDirectory.Base.Settings;
using ASC.ActiveDirectory.ComplexOperations;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Users;
// ReSharper disable RedundantToStringCall

namespace ASC.ActiveDirectory.Base
{
    public class LdapUserImporter: IDisposable
    {
        public List<LdapObject> AllDomainUsers { get; private set; }
        public List<LdapObject> AllDomainGroups { get; private set; }

        public Dictionary<LdapObject, LdapSettingsStatus> AllSkipedDomainUsers { get; private set; }
        public Dictionary<LdapObject, LdapSettingsStatus> AllSkipedDomainGroups { get; private set; }

        private string _ldapDomain;
        private static readonly string UnknownDomain = ConfigurationManager.AppSettings["ldap.domain"] ?? "unknown";

        public string LDAPDomain {
            get
            {
                if (!string.IsNullOrEmpty(_ldapDomain)) 
                    return _ldapDomain;

                _ldapDomain = LoadLDAPDomain();

                if (string.IsNullOrEmpty(_ldapDomain))
                {
                    _ldapDomain = UnknownDomain;
                }

                return _ldapDomain;
            }
        }
        public string PrimaryGroupId { get; set; }

        public LdapSettings Settings
        {
            get { return LdapHelper.Settings; }
        }

        public LdapHelper LdapHelper { get; private set; }
        public LdapLocalization Resource { get; private set; }

        private List<string> _watchedNestedGroups;

        private readonly ILog _log;

        public LdapUserImporter(LdapHelper ldapHelper, LdapLocalization resource)
        {
            LdapHelper = ldapHelper;

            AllDomainUsers = new List<LdapObject>();
            AllDomainGroups = new List<LdapObject>();
            AllSkipedDomainUsers = new Dictionary<LdapObject, LdapSettingsStatus>();
            AllSkipedDomainGroups = new Dictionary<LdapObject, LdapSettingsStatus>();

            Resource = resource;

            _log = LogManager.GetLogger("ASC");

            _watchedNestedGroups = new List<string>();
        }

        public List<UserInfo> GetDiscoveredUsersByAttributes()
        {
            var users = new List<UserInfo>();

            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return users;

            var usersToAdd = AllDomainUsers.Select(ldapObject => ldapObject.ToUserInfo(this, _log));

            users.AddRange(usersToAdd);

            return users;
        }

        public List<GroupInfo> GetDiscoveredGroupsByAttributes()
        {
            if (!Settings.GroupMembership)
                return new List<GroupInfo>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return new List<GroupInfo>();

            var groups = new List<GroupInfo>();

            var groupsToAdd = AllDomainGroups.ConvertAll(g => g.ToGroupInfo(Settings));

            groups.AddRange(groupsToAdd);

            return groups;
        }

        public List<UserInfo> GetGroupUsers(GroupInfo groupInfo)
        {
            return GetGroupUsers(groupInfo, true);
        }

        private List<UserInfo> GetGroupUsers(GroupInfo groupInfo, bool clearCache)
        {
            if (!LdapHelper.IsConnected)
                LdapHelper.Connect();

            _log.DebugFormat("LdapUserImporter.GetGroupUsers(Group name: {0})", groupInfo.Name);

            var users = new List<UserInfo>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return users;

            var domainGroup = AllDomainGroups.FirstOrDefault(lg => lg.Sid.Equals(groupInfo.Sid));

            if (domainGroup == null)
                return users;

            if (!string.IsNullOrEmpty(PrimaryGroupId) && domainGroup.Sid.EndsWith("-" + PrimaryGroupId))
            {
                // Domain Users found

                var ldapUsers = FindUsersByPrimaryGroup();

               if (!ldapUsers.Any())
                   return users;

                foreach (var ldapUser in ldapUsers)
                {
                        var userInfo = ldapUser.ToUserInfo(this, _log);

                        if (!users.Exists(u => u.Sid == userInfo.Sid))
                            users.Add(userInfo);
                    }
            }
            else
            {
                var members = domainGroup.GetAttributes(Settings.GroupAttribute, _log);

                if (!members.Any())
                    return users;

                foreach (var member in members)
                {
                    var ldapUser = FindUserByMember(member);

                    if (ldapUser == null)
                    {
                        var nestedLdapGroup = FindGroupByMember(member);

                        if (nestedLdapGroup != null)
                        {
                            _log.DebugFormat("Found nested LDAP Group: {0}", nestedLdapGroup.DistinguishedName);

                            if (clearCache)
                                _watchedNestedGroups = new List<string>();

                            if (_watchedNestedGroups.Contains(nestedLdapGroup.DistinguishedName))
                            {
                                _log.DebugFormat("Skip already watched nested LDAP Group: {0}", nestedLdapGroup.DistinguishedName);
                                continue;
                            }

                            _watchedNestedGroups.Add(nestedLdapGroup.DistinguishedName);

                            var nestedGroupInfo = nestedLdapGroup.ToGroupInfo(Settings, _log);

                            var nestedGroupUsers = GetGroupUsers(nestedGroupInfo, false);

                            foreach (var groupUser in nestedGroupUsers)
                            {
                                if (!users.Exists(u => u.Sid == groupUser.Sid))
                                    users.Add(groupUser);
                            }
                        }

                        continue;
                    }

                        var userInfo = ldapUser.ToUserInfo(this, _log);

                        if (!users.Exists(u => u.Sid == userInfo.Sid))
                            users.Add(userInfo);
                    }
            }

            return users;
        }

        const string GROUP_MEMBERSHIP = "groupMembership";

        private bool TryGetLdapUserGroups(LdapObject ldapUser, out List<LdapObject> ldapUserGroups)
        {
            ldapUserGroups = new List<LdapObject>();
            try
            {
                if (!Settings.GroupMembership)
                {
                    return false;
                }

                if (ldapUser == null ||
                    string.IsNullOrEmpty(ldapUser.Sid))
                {
                    return false;
                }

                if (!LdapHelper.IsConnected)
                    LdapHelper.Connect();

                var userGroups = ldapUser.GetAttributes(LdapConstants.ADSchemaAttributes.MEMBER_OF, _log)
                    .Select(s => s.Replace("\\", string.Empty))
                    .ToList();

                if (!userGroups.Any())
                {
                    userGroups = ldapUser.GetAttributes(GROUP_MEMBERSHIP, _log);
                }

                var searchExpressions = new List<Expression>();

                PrimaryGroupId = PrimaryGroupId ??
                                ldapUser.GetValue(LdapConstants.ADSchemaAttributes.PRIMARY_GROUP_ID) as string;

                if (!string.IsNullOrEmpty(PrimaryGroupId))
                {
                    var userSid = ldapUser.Sid;
                    var index = userSid.LastIndexOf("-", StringComparison.InvariantCultureIgnoreCase);

                    if (index > -1)
                    {
                        var primaryGroupSid = userSid.Substring(0, index + 1) + PrimaryGroupId;
                        searchExpressions.Add(Expression.Equal(ldapUser.SidAttribute, primaryGroupSid));
                    }
                }

                if (userGroups.Any())
                {
                    searchExpressions.AddRange(userGroups
                        .Select(g => g.Substring(0, g.IndexOf(",", StringComparison.InvariantCultureIgnoreCase)))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(Expression.Parse)
                        .Where(e => e != null));

                    var criteria = Criteria.Any(searchExpressions.ToArray());

                    var foundList = LdapHelper.GetGroups(criteria);

                    if (foundList.Any())
                    {
                        ldapUserGroups.AddRange(foundList);
                        return true;
                    }
                }
                else
                {
                    var ldapGroups = LdapHelper.GetGroups();

                    ldapUserGroups.AddRange(
                        ldapGroups.Where(
                            ldapGroup =>
                                LdapHelper.UserExistsInGroup(ldapGroup, ldapUser, Settings)));

                    return ldapUserGroups.Any();
                }
            }
            catch (Exception ex)
            {
                if (ldapUser != null)
                    _log.ErrorFormat("IsUserExistInGroups(login: '{0}' sid: '{1}') error {2}",
                        ldapUser.DistinguishedName, ldapUser.Sid, ex);
            }

            return false;
        }

        public bool TrySyncUserGroupMembership(Tuple<UserInfo, LdapObject> ldapUserInfo)
        {
            if (ldapUserInfo == null ||
                !Settings.GroupMembership)
            {
                return false;
            }

            var userInfo = ldapUserInfo.Item1;
            var ldapUser = ldapUserInfo.Item2;

            List<LdapObject> ldapUserGroupList;

            if (!TryGetLdapUserGroups(ldapUser, out ldapUserGroupList))
                return false;

            if (!LdapHelper.IsConnected)
                LdapHelper.Connect();

            var portalUserLdapGroups =
                CoreContext.UserManager.GetUserGroups(userInfo.ID, IncludeType.All)
                    .Where(g => !string.IsNullOrEmpty(g.Sid))
                    .ToList();

            var actualPortalLdapGroups = new List<GroupInfo>();

            foreach (var ldapUserGroup in ldapUserGroupList)
            {
                var groupInfo = CoreContext.UserManager.GetGroupInfoBySid(ldapUserGroup.Sid);

                if (Equals(groupInfo, Constants.LostGroupInfo))
                {
                    groupInfo = CoreContext.UserManager.SaveGroupInfo(ldapUserGroup.ToGroupInfo(Settings, _log));

                    CoreContext.UserManager.AddUserIntoGroup(userInfo.ID, groupInfo.ID);

                    actualPortalLdapGroups.Add(groupInfo);
                }
                else if (!portalUserLdapGroups.Contains(groupInfo))
                {
                    CoreContext.UserManager.AddUserIntoGroup(userInfo.ID, groupInfo.ID);

                    actualPortalLdapGroups.Add(groupInfo);
                }
            }

            if (!actualPortalLdapGroups.Any())
                return true;

            foreach (var portalUserLdapGroup in portalUserLdapGroups)
            {
                if (!actualPortalLdapGroups.Contains(portalUserLdapGroup))
                {
                    CoreContext.UserManager.RemoveUserFromGroup(userInfo.ID, portalUserLdapGroup.ID);
                }
            }

            return true;
        }

        public bool TryLoadLDAPUsers()
        {
            try
            {
                if (!Settings.EnableLdapAuthentication)
                    return false;

                if (!LdapHelper.IsConnected)
                    LdapHelper.Connect();

                var users = LdapHelper.GetUsers();

                foreach (var user in users)
                {
                    if (string.IsNullOrEmpty(user.Sid))
                    {
                        AllSkipedDomainUsers.Add(user, LdapSettingsStatus.WrongSidAttribute);
                        continue;
                    }

                    if (!CheckLoginAttribute(user, Settings.LoginAttribute))
                    {
                        AllSkipedDomainUsers.Add(user, LdapSettingsStatus.WrongLoginAttribute);
                        continue;
                    }

                    if (!Settings.GroupMembership)
                    {
                        AllDomainUsers.Add(user);
                        continue;
                    }

                    if (!Settings.UserAttribute.Equals(LdapConstants.RfcLDAPAttributes.DN,
                        StringComparison.InvariantCultureIgnoreCase) && !CheckUserAttribute(user, Settings.UserAttribute))
                    {
                        AllSkipedDomainUsers.Add(user, LdapSettingsStatus.WrongUserAttribute);
                        continue;
                    }

                    AllDomainUsers.Add(user);
                }

                if (AllDomainUsers.Any())
                {
                    PrimaryGroupId = AllDomainUsers.First().GetValue(LdapConstants.ADSchemaAttributes.PRIMARY_GROUP_ID) as string;
                }

                return AllDomainUsers.Any() || !users.Any();
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("TryLoadLDAPUsers(): Incorrect filter. userFilter = {0}", Settings.UserFilter);
            }

            return false;
        }

        public bool TryLoadLDAPGroups()
        {
            try
            {
                if (!Settings.EnableLdapAuthentication || !Settings.GroupMembership)
                {
                    return false;
                }

                if (!LdapHelper.IsConnected)
                    LdapHelper.Connect();

                var groups = LdapHelper.GetGroups();

                foreach (var group in groups)
                {
                    if (string.IsNullOrEmpty(group.Sid))
                    {
                        AllSkipedDomainGroups.Add(group, LdapSettingsStatus.WrongSidAttribute);
                        continue;
                    }

                    if (!CheckGroupAttribute(group, Settings.GroupAttribute))
                    {
                        AllSkipedDomainGroups.Add(group, LdapSettingsStatus.WrongGroupAttribute);
                        continue;
                    }

                    if (!CheckGroupNameAttribute(group, Settings.GroupNameAttribute))
                    {
                        AllSkipedDomainGroups.Add(group, LdapSettingsStatus.WrongGroupNameAttribute);
                        continue;
                    }

                    AllDomainGroups.Add(group);
                }

                return AllDomainGroups.Any() || !groups.Any();
            }
            catch (ArgumentException)
            {
                _log.ErrorFormat("TryLoadLDAPGroups(): Incorrect group filter. groupFilter = {0}", Settings.GroupFilter);
            }

            return false;
        }

        private string LoadLDAPDomain()
        {
            try
            {
                if (!Settings.EnableLdapAuthentication)
                    return null;

                if (!LdapHelper.IsConnected)
                    LdapHelper.Connect();

                string ldapDomain;

                if (AllDomainUsers.Any())
                {
                    ldapDomain = AllDomainUsers.First().GetDomainFromDn();

                    if (!string.IsNullOrEmpty(ldapDomain))
                        return ldapDomain;
                }

                ldapDomain = LdapHelper.SearchDomain();

                if (!string.IsNullOrEmpty(ldapDomain))
                    return ldapDomain;

                ldapDomain = LdapUtils.DistinguishedNameToDomain(Settings.UserDN);

                if (!string.IsNullOrEmpty(ldapDomain))
                    return ldapDomain;

                ldapDomain = LdapUtils.DistinguishedNameToDomain(Settings.GroupDN);

                if (!string.IsNullOrEmpty(ldapDomain))
                    return ldapDomain;
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("LoadLDAPDomain(): Error: {0}", ex);
            }

            return null;
        }

        protected bool CheckLoginAttribute(LdapObject user, string loginAttribute)
        {
            try
            {
                var member = user.GetValue(loginAttribute);
                if (member == null || string.IsNullOrWhiteSpace(member.ToString()))
                {
                    _log.DebugFormat("Login Attribute parameter ({0}) not found: DN = {1}", Settings.LoginAttribute,
                        user.DistinguishedName);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Login Attribute parameter ({0}) not found: loginAttribute = {1}. {2}", Settings.LoginAttribute,
                    loginAttribute, e);
                return false;
            }
            return true;
        }

        protected bool CheckUserAttribute(LdapObject user, string userAttr)
        {
            try
            {
                var userAttribute = user.GetValue(userAttr);
                if (userAttribute == null || string.IsNullOrWhiteSpace(userAttribute.ToString()))
                {
                    _log.DebugFormat("User Attribute parameter ({0}) not found: DN = {1}", Settings.UserAttribute,
                        user.DistinguishedName);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("User Attribute parameter ({0}) not found: userAttr = {1}. {2}",
                    Settings.UserAttribute, userAttr, e);
                return false;
            }
            return true;
        }

        protected bool CheckGroupAttribute(LdapObject group, string groupAttr)
        {
            try
            {
                group.GetValue(groupAttr); // Group attribute can be empty - example => Domain users
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Group Attribute parameter ({0}) not found: {1}. {2}",
                    Settings.GroupAttribute, groupAttr, e);
                return false;
            }
            return true;
        }

        protected bool CheckGroupNameAttribute(LdapObject group, string groupAttr)
        {
            try
            {
                var groupNameAttribute = group.GetValues(groupAttr);
                if (!groupNameAttribute.Any())
                {
                    _log.DebugFormat("Group Name Attribute parameter ({0}) not found: {1}", Settings.GroupNameAttribute,
                        groupAttr);
                    return false;
                }
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Group Attribute parameter ({0}) not found: {1}. {2}", Settings.GroupNameAttribute,
                    groupAttr, e);
                return false;
            }
            return true;
        }

        private List<LdapObject> FindUsersByPrimaryGroup()
        {
            _log.Debug("LdapUserImporter.FindUsersByPrimaryGroup()");

            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return null;

            return
                AllDomainUsers.Where(
                    lu =>
                    {
                        var primaryGroupId = lu.GetValue(LdapConstants.ADSchemaAttributes.PRIMARY_GROUP_ID) as string;

                        return !string.IsNullOrEmpty(primaryGroupId) &&
                               primaryGroupId.Equals(PrimaryGroupId, StringComparison.InvariantCultureIgnoreCase);
                    })
                    .ToList();

        }

        private LdapObject FindUserByMember(string userAttributeValue)
        {
            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return null;

            _log.DebugFormat("LdapUserImporter.FindUserByMember(user attr: {0})", userAttributeValue);

            return AllDomainUsers.FirstOrDefault(u =>
                u.DistinguishedName.Equals(userAttributeValue, StringComparison.InvariantCultureIgnoreCase)
                || Convert.ToString(u.GetValue(Settings.UserAttribute)).Equals(userAttributeValue,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        private LdapObject FindGroupByMember(string member)
        {
            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return null;

            _log.DebugFormat("LdapUserImporter.FindGroupByMember(member: {0})", member);

            return AllDomainGroups.FirstOrDefault(g =>
                g.DistinguishedName.Equals(member, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<Tuple<UserInfo, LdapObject>> FindLdapUsers(string login)
        {
            var listResults = new List<Tuple<UserInfo, LdapObject>>();

            var ldapLogin = LdapLogin.ParseLogin(login);

            if (ldapLogin == null)
                return listResults;

            if (!LdapHelper.IsConnected)
                LdapHelper.Connect();

            var exps = new List<Expression> {Expression.Equal(Settings.LoginAttribute, ldapLogin.Username)};

            if (!ldapLogin.Username.Equals(login) && ldapLogin.ToString().Equals(login))
            {
                exps.Add(Expression.Equal(Settings.LoginAttribute, login));
            }

            string email = null;

            if (!string.IsNullOrEmpty(Settings.MailAttribute) && !string.IsNullOrEmpty(ldapLogin.Domain) && login.Contains("@"))
            {
                email = ldapLogin.ToString();
                exps.Add(Expression.Equal(Settings.MailAttribute, email));
            }

            var searchTerm = exps.Count > 1 ? Criteria.Any(exps.ToArray()).ToString() : exps.First().ToString();

            var users = LdapHelper.GetUsers(searchTerm, !string.IsNullOrEmpty(email) ? -1 : 1)
                .Where(user => user != null)
                .ToLookup(lu =>
                {
                    var ui = Constants.LostUser;

                    try
                    {
                        if (string.IsNullOrEmpty(_ldapDomain))
                        {
                            _ldapDomain = LdapUtils.DistinguishedNameToDomain(lu.DistinguishedName);
                        }

                        ui = lu.ToUserInfo(this, _log);
                    }
                    catch(Exception ex)
                    {
                        _log.ErrorFormat("FindLdapUser->ToUserInfo() failed. Error: {0}", ex.ToString());
                    }

                    return Tuple.Create(ui, lu);

                });

            if (!users.Any())
                return listResults;

            foreach (var user in users)
            {
                var ui = user.Key.Item1;

                if (ui.Equals(Constants.LostUser))
                    continue;

                var ul = user.Key.Item2;

                var ldapLoginAttribute = ul.GetValue(Settings.LoginAttribute) as string;

                if (string.IsNullOrEmpty(ldapLoginAttribute))
                {
                    _log.WarnFormat("LDAP: DN: '{0}' Login Attribute '{1}' is empty", ul.DistinguishedName, Settings.LoginAttribute);
                    continue;
                }

                if (ldapLoginAttribute.Equals(login))
                {
                    listResults.Add(user.Key);
                    continue;
                }

                if (!string.IsNullOrEmpty(email))
                {
                    if (ui.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        listResults.Add(user.Key);
                        continue;
                    }
                }

                if (LdapUtils.IsLoginAccepted(ldapLogin, ui, LDAPDomain))
                {
                    listResults.Add(user.Key);
                }
            }

            return listResults;
        }

        public List<LdapObject> FindUsersByAttribute(string key, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var users = new List<LdapObject>();

            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return users;

            return users.Where(us => !us.IsDisabled && string.Equals((string)us.GetValue(key), value, comparison)).ToList();
        }

        public List<LdapObject> FindUsersByAttribute(string key, IEnumerable<string> value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var users = new List<LdapObject>();

            if (!AllDomainUsers.Any() && !TryLoadLDAPUsers())
                return users;

            return AllDomainUsers.Where(us => !us.IsDisabled && value.Any(val => string.Equals(val, (string)us.GetValue(key), comparison))).ToList();
        }

        public List<LdapObject> FindGroupsByAttribute(string key, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var gr = new List<LdapObject>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return gr;

            return gr.Where(g => !g.IsDisabled && string.Equals((string)g.GetValue(key), value, comparison)).ToList();
        }

        public List<LdapObject> FindGroupsByAttribute(string key, IEnumerable<string> value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            var gr = new List<LdapObject>();

            if (!AllDomainGroups.Any() && !TryLoadLDAPGroups())
                return gr;

            return AllDomainGroups.Where(g => !g.IsDisabled && value.Any(val => string.Equals(val, (string)g.GetValue(key), comparison))).ToList();
        }

        public Tuple<UserInfo, LdapObject> Login(string login, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                    return null;

                var ldapUsers = FindLdapUsers(login);

                _log.DebugFormat("FindLdapUsers(login '{0}') found: {1} users", login, ldapUsers.Count);

                foreach (var ldapUser in ldapUsers)
                {
                    string currentLogin = null;
                    try
                    {
                        var ldapUserInfo = ldapUser.Item1;
                        var ldapUserObject = ldapUser.Item2;

                        if (ldapUserInfo.Equals(Constants.LostUser)
                            || ldapUserObject == null
                            || string.IsNullOrEmpty(ldapUserObject.DistinguishedName))
                        {
                            continue;
                        }

                        currentLogin = ldapUserObject.DistinguishedName;

                        _log.DebugFormat("LdapUserImporter.Login('{0}')", currentLogin);

                        LdapHelper.CheckCredentials(currentLogin, password, Settings.Server,
                            Settings.PortNumber, Settings.StartTls, Settings.Ssl, Settings.AcceptCertificate, 
                            Settings.AcceptCertificateHash);

                        return new Tuple<UserInfo, LdapObject>(ldapUserInfo, ldapUserObject);
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("LdapUserImporter->Login(login: '{0}') failed. Error: {1}", currentLogin ?? login, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("LdapUserImporter->Login({0}) failed {1}", login, ex);
            }

            return null;
        }

        public void Dispose()
        {
            if (LdapHelper != null)
                LdapHelper.Dispose();
        }
    }
}
