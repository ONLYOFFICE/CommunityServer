/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Common.Caching;
using ASC.Common.Security;
using ASC.Common.Security.Authorizing;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAction = ASC.Common.Security.Authorizing.Action;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Core
{
    public static class WebItemSecurity
    {
        private static readonly SecurityAction Read = new SecurityAction(new Guid("77777777-32ae-425f-99b5-83176061d1ae"), "ReadWebItem", false, true);
        private static readonly ICache cache = AscCache.Memory;

                
        public static bool IsAvailableForUser(string id, Guid @for)
        {
            var result = false;

            var key = GetCacheKey();
            var dic = cache.Get<Dictionary<string, bool>>(key);
            if (dic == null)
            {
                cache.Insert(key, dic = new Dictionary<string, bool>(), DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
            }
            else
            {
                lock (dic)
                {
                    if (dic.ContainsKey(id + @for))
                    {
                        return dic[id + @for];
                    }
                }
            }

            // can read or administrator
            var securityObj = WebItemSecurityObject.Create(id);

            if (CoreContext.Configuration.Personal
                && securityObj.WebItemId != WebItemManager.DocumentsProductID)
            {
                // only files visible in your-docs portal
                result = false;
            }
            else
            {
                var webitem = WebItemManager.Instance[securityObj.WebItemId];
                if (webitem != null)
                {
                    if ((webitem.ID == WebItemManager.CRMProductID ||
                        webitem.ID == WebItemManager.PeopleProductID ||
                        webitem.ID == WebItemManager.BirthdaysProductID ||
                        webitem.ID == WebItemManager.MailProductID) &&
                        CoreContext.UserManager.GetUsers(@for).IsVisitor())
                    {
                        // hack: crm, people, birtthday and mail products not visible for collaborators
                        result = false;
                    }
                    else if ((webitem.ID == WebItemManager.CalendarProductID ||
                              webitem.ID == WebItemManager.TalkProductID) &&
                             CoreContext.UserManager.GetUsers(@for).IsOutsider())
                    {
                        // hack: calendar and talk products not visible for outsider
                        result = false;
                    }
                    else if (webitem is IModule)
                    {
                        result = SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(@for), securityObj, null, Read) &&
                            IsAvailableForUser(WebItemManager.Instance.GetParentItemID(webitem.ID).ToString(), @for);
                    }
                    else
                    {
                        var hasUsers = CoreContext.AuthorizationManager.GetAces(Guid.Empty, Read.ID, securityObj).Any(a => a.SubjectId != ASC.Core.Users.Constants.GroupEveryone.ID);
                        result = SecurityContext.PermissionResolver.Check(CoreContext.Authentication.GetAccountByID(@for), securityObj, null, Read) ||
                                 (hasUsers && IsProductAdministrator(securityObj.WebItemId, @for));
                    }
                }
                else
                {
                    result = false;
                }
            }

            dic = cache.Get<Dictionary<string, bool>>(key);
            if (dic != null)
            {
                lock (dic)
                {
                    dic[id + @for] = result;
                }
            }
            return result;
        }


        public static bool IsLicensed(IWebItem item)
        {
            return true;
        }

        public static void SetSecurity(string id, bool enabled, params Guid[] subjects)
        {
            if(SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone)
                throw new SecurityException("Security settings are disabled for an open portal");
            
            var securityObj = WebItemSecurityObject.Create(id);

            // remove old aces
            CoreContext.AuthorizationManager.RemoveAllAces(securityObj);
            var allowToAll = new AzRecord(ASC.Core.Users.Constants.GroupEveryone.ID, Read.ID, AceType.Allow, securityObj);
            CoreContext.AuthorizationManager.RemoveAce(allowToAll);

            // set new aces
            if (subjects == null || subjects.Length == 0 || subjects.Contains(ASC.Core.Users.Constants.GroupEveryone.ID))
            {
                if (!enabled && subjects != null && subjects.Length == 0)
                {
                    // users from list with no users equals allow to all users
                    enabled = true;
                }
                subjects = new[] { ASC.Core.Users.Constants.GroupEveryone.ID };
            }
            foreach (var s in subjects)
            {
                var a = new AzRecord(s, Read.ID, enabled ? AceType.Allow : AceType.Deny, securityObj);
                CoreContext.AuthorizationManager.AddAce(a);
            }

            ClearCache();
        }

        public static WebItemSecurityInfo GetSecurityInfo(string id)
        {
            var info = GetSecurity(id).ToList();
            var module = WebItemManager.Instance.GetParentItemID(new Guid(id)) != Guid.Empty;
            return new WebItemSecurityInfo
                       {
                           WebItemId = id,

                           Enabled = !info.Any() || (!module && info.Any(i => i.Item2)) || (module && info.All(i => i.Item2)),

                           Users = info
                               .Select(i => CoreContext.UserManager.GetUsers(i.Item1))
                               .Where(u => u.ID != ASC.Core.Users.Constants.LostUser.ID),

                           Groups = info
                               .Select(i => CoreContext.UserManager.GetGroupInfo(i.Item1))
                               .Where(g => g.ID != ASC.Core.Users.Constants.LostGroupInfo.ID && g.CategoryID != ASC.Core.Users.Constants.SysGroupCategoryId)
                       };
        }

        private static IEnumerable<Tuple<Guid, bool>> GetSecurity(string id)
        {
            var securityObj = WebItemSecurityObject.Create(id);
            var result = CoreContext.AuthorizationManager
                .GetAcesWithInherits(Guid.Empty, Read.ID, securityObj, null)
                .GroupBy(a => a.SubjectId)
                .Select(a => Tuple.Create(a.Key, a.First().Reaction == AceType.Allow))
                .ToList();
            if (!result.Any())
            {
                result.Add(Tuple.Create(ASC.Core.Users.Constants.GroupEveryone.ID, false));
            }
            return result;
        }

        public static void SetProductAdministrator(Guid productid, Guid userid, bool administrator)
        {
            if (productid == Guid.Empty)
            {
                productid = ASC.Core.Users.Constants.GroupAdmin.ID;
            }
            if (administrator)
            {
                if (CoreContext.UserManager.IsUserInGroup(userid, ASC.Core.Users.Constants.GroupVisitor.ID))
                {
                    throw new System.Security.SecurityException("Collaborator can not be an administrator");
                }
                CoreContext.UserManager.AddUserIntoGroup(userid, productid);
            }
            else
            {
                if (productid == ASC.Core.Users.Constants.GroupAdmin.ID)
                {
                    foreach (var id in WebItemManager.Instance.GetItemsAll().OfType<IProduct>().Select(p => p.ID))
                    {
                        CoreContext.UserManager.RemoveUserFromGroup(userid, id);
                    }
                }
                CoreContext.UserManager.RemoveUserFromGroup(userid, productid);
            }

            ClearCache();
        }

        public static bool IsProductAdministrator(Guid productid, Guid userid)
        {
            if (CoreContext.UserManager.IsUserInGroup(userid, ASC.Core.Users.Constants.GroupAdmin.ID))
            {
                return true;
            }
            else
            {
                return CoreContext.UserManager.IsUserInGroup(userid, productid);
            }
        }

        public static IEnumerable<UserInfo> GetProductAdministrators(Guid productid)
        {
            var groups = new List<Guid>();
            if (productid == Guid.Empty)
            {
                groups.Add(ASC.Core.Users.Constants.GroupAdmin.ID);
                groups.AddRange(WebItemManager.Instance.GetItemsAll().OfType<IProduct>().Select(p => p.ID));
            }
            else
            {
                groups.Add(productid);
            }

            var users = Enumerable.Empty<UserInfo>();
            foreach (var id in groups)
            {
                users = users.Union(CoreContext.UserManager.GetUsersByGroup(id));
            }
            return users.ToList();
        }

        public static void ClearCache()
        {
            cache.Remove(GetCacheKey());
        }

        private static string GetCacheKey()
        {
            return string.Format("{0}:{1}", TenantProvider.CurrentTenantID, "webitemsecurity");
        }

        private class WebItemSecurityObject : ISecurityObject
        {
            public Guid WebItemId { get; private set; }


            public Type ObjectType
            {
                get { return GetType(); }
            }

            public object SecurityId
            {
                get { return WebItemId.ToString("N"); }
            }

            public bool InheritSupported
            {
                get { return true; }
            }

            public bool ObjectRolesSupported
            {
                get { return false; }
            }


            public static WebItemSecurityObject Create(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id");
                }

                var itemId = Guid.Empty;
                if (32 <= id.Length)
                {
                    itemId = new Guid(id);
                }
                else
                {
                    var w = WebItemManager.Instance
                        .GetItemsAll()
                        .FirstOrDefault(i => id.Equals(i.GetSysName(), StringComparison.InvariantCultureIgnoreCase));
                    if (w != null) itemId = w.ID;
                }
                return new WebItemSecurityObject(itemId);
            }


            private WebItemSecurityObject(Guid itemId)
            {
                WebItemId = itemId;
            }

            public ISecurityObjectId InheritFrom(ISecurityObjectId objectId)
            {
                var s = objectId as WebItemSecurityObject;
                if (s != null)
                {
                    var parent = WebItemSecurityObject.Create(WebItemManager.Instance.GetParentItemID(s.WebItemId).ToString("N")) as WebItemSecurityObject;
                    return parent != null && parent.WebItemId != s.WebItemId && parent.WebItemId != Guid.Empty ? parent : null;
                }
                return null;
            }

            public IEnumerable<IRole> GetObjectRoles(ISubject account, ISecurityObjectId objectId, SecurityCallContext callContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}