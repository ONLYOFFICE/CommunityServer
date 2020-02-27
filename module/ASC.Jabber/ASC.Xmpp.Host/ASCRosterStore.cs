/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Core.Users;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Configuration;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ASC.Xmpp.Host
{
    class ASCRosterStore : DbRosterStore, IRosterStore
    {
        #region IRosterStore Members

        public override List<UserRosterItem> GetRosterItems(Jid rosterJid)
        {
            try
            {
                ASCContext.SetCurrentTenant(rosterJid.Server);
                var items = GetASCRosterItems(rosterJid);
                items.AddRange(base.GetRosterItems(rosterJid));
                SortRoster(items);
                return items;
            }
            catch (Exception e)
            {
                throw new JabberException("Could not get roster items.", e);
            }
        }

        public override UserRosterItem GetRosterItem(Jid rosterJid, Jid itemJid)
        {
            try
            {
                ASCContext.SetCurrentTenant(rosterJid.Server);
                var u = ASCContext.UserManager.GetUserByUserName(itemJid.User);
                return !string.IsNullOrEmpty(u.UserName) ?
                    ToUserRosterItem(u, itemJid.Server) :
                    base.GetRosterItem(rosterJid, itemJid);
            }
            catch (Exception e)
            {
                throw new JabberException("Could not get roster item.", e);
            }
        }

        public override UserRosterItem SaveRosterItem(Jid rosterJid, UserRosterItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            ASCContext.SetCurrentTenant(rosterJid.Server);
            if (IsASCRosterItem(rosterJid, item.Jid)) throw new JabberException(ErrorCode.Forbidden);

            return base.SaveRosterItem(rosterJid, item);
        }

        public override void RemoveRosterItem(Jid rosterJid, Jid itemJid)
        {
            ASCContext.SetCurrentTenant(rosterJid.Server);
            if (IsASCRosterItem(rosterJid, itemJid)) throw new JabberException(ErrorCode.Forbidden);

            base.RemoveRosterItem(rosterJid, itemJid);
        }

        #endregion

        private List<UserRosterItem> GetASCRosterItems(Jid jid)
        {
            var items = new List<UserRosterItem>();
            foreach (var u in ASCContext.UserManager.GetUsers())
            {
                if (string.IsNullOrEmpty(u.UserName) || string.Compare(jid.User, u.UserName, true) == 0) continue;
                items.Add(ToUserRosterItem(u, jid.Server));
            }
            // for migration from teamlab.com to onlyoffice.com
            string domain = jid.Server;
            if (JabberConfiguration.ReplaceDomain && domain.EndsWith(JabberConfiguration.ReplaceFromDomain))
            {
                int place = domain.LastIndexOf(JabberConfiguration.ReplaceFromDomain);
                if (place >= 0)
                {
                    domain = domain.Remove(place, JabberConfiguration.ReplaceFromDomain.Length).Insert(place, JabberConfiguration.ReplaceToDomain);
                }
            }
            //add server
            items.Add(new UserRosterItem(new Jid(jid.Server)) { Name = domain, Subscribtion = SubscriptionType.both, Ask = AskType.NONE });
            return items;
        }

        private bool IsASCRosterItem(Jid rosterJid, Jid itemJid)
        {
            return ASCContext.UserManager.IsUserNameExists(itemJid.User);
        }

        private void SortRoster(List<UserRosterItem> roster)
        {
            roster.Sort((x, y) => string.Compare(!string.IsNullOrEmpty(x.Name) ? x.Name : x.Jid.ToString(), !string.IsNullOrEmpty(y.Name) ? y.Name : y.Jid.ToString(), true));
        }

        private UserRosterItem ToUserRosterItem(UserInfo u, string server)
        {
            var item = new UserRosterItem(new Jid(u.UserName + "@" + server))
            {
                Name = UserFormatter.GetUserName(u),
                Subscribtion = SubscriptionType.both,
                Ask = AskType.NONE,
            };
            foreach (var g in ASCContext.UserManager.GetUserGroups(u.ID))
            {
                item.Groups.Add(g.Name);
            }
            return item;
        }
    }
}