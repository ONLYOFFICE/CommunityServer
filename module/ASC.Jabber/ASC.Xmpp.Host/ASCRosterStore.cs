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

using System;
using System.Collections.Generic;
using ASC.Core.Users;
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Server;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Storage.Interface;
using System.Configuration;

namespace ASC.Xmpp.Host
{
    class ASCRosterStore : DbRosterStore, IRosterStore
    {
        // for migration from teamlab.com to onlyoffice.com
        private static string fromTeamlabToOnlyOffice = ConfigurationManager.AppSettings["jabber.from-teamlab-to-onlyoffice"] ?? "true";
        private static string fromServerInJid = ConfigurationManager.AppSettings["jabber.from-server-in-jid"] ?? "teamlab.com";
        private static string toServerInJid = ConfigurationManager.AppSettings["jabber.to-server-in-jid"] ?? "onlyoffice.com";

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
            if (fromTeamlabToOnlyOffice == "true" && domain.EndsWith(fromServerInJid))
            {
                int place = domain.LastIndexOf(fromServerInJid);
                if (place >= 0)
                {
                    domain = domain.Remove(place, fromServerInJid.Length).Insert(place, toServerInJid);
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