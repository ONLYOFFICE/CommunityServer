/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Common.Data.Sql;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.iq.roster;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using UserRosterItemDic = System.Collections.Generic.Dictionary<ASC.Xmpp.Core.protocol.Jid, ASC.Xmpp.Server.Storage.UserRosterItem>;

namespace ASC.Xmpp.Server.Storage
{
    public class DbRosterStore : DbStoreBase, IRosterStore
    {
        private static readonly string GroupSeparator = "$@$;";

        private IDictionary<Jid, UserRosterItemDic> cache;

        private readonly object syncRoot = new object();


        private IDictionary<Jid, UserRosterItemDic> RosterItems
        {
            get
            {
                if (cache == null)
                {
                    lock (syncRoot)
                    {
                        if (cache == null) cache = LoadRosterItems();
                    }
                }
                return cache;
            }
        }

        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_roster", true)
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("item_jid", DbType.String, 255, true)
                .AddColumn("name", DbType.String, 512)
                .AddColumn(new SqlCreate.Column("subscription", DbType.Int32).NotNull(true).Default(0))
                .AddColumn(new SqlCreate.Column("ask", DbType.Int32).NotNull(true).Default(0))
                .AddColumn("groups", DbType.String, UInt16.MaxValue)
                .PrimaryKey("jid", "item_jid");
            return new[] { t1 };
        }


        #region IRosterStore Members

        public virtual List<UserRosterItem> GetRosterItems(Jid rosterJid)
        {
            try
            {
                lock (syncRoot)
                {
                    var bareJid = new Jid(rosterJid.Bare.ToLowerInvariant());
                    if (RosterItems.ContainsKey(bareJid)) return new List<UserRosterItem>(RosterItems[bareJid].Values);
                    return new List<UserRosterItem>();
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not get roster items.", e);
            }
        }

        public virtual List<UserRosterItem> GetRosterItems(Jid rosterJid, SubscriptionType subscriptionType)
        {
            return GetRosterItems(rosterJid).FindAll(i => { return i.Subscribtion == subscriptionType; });
        }

        public virtual UserRosterItem GetRosterItem(Jid rosterJid, Jid itemJid)
        {
            return GetRosterItems(rosterJid).Find(i => { return string.Compare(i.Jid.Bare, itemJid.Bare, true) == 0; });
        }

        public virtual UserRosterItem SaveRosterItem(Jid rosterJid, UserRosterItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            try
            {
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(item.Name)) item.Name = item.Jid.Bare;
                    rosterJid = new Jid(rosterJid.Bare.ToLowerInvariant());
                    ExecuteNonQuery(new SqlInsert("jabber_roster", true)
                        .InColumnValue("jid", rosterJid.ToString())
                        .InColumnValue("item_jid", item.Jid.ToString())
                        .InColumnValue("name", item.Name)
                        .InColumnValue("subscription", (Int32)item.Subscribtion)
                        .InColumnValue("ask", (Int32)item.Ask)
                        .InColumnValue("groups", string.Join(GroupSeparator, item.Groups.ToArray())));

                    if (!RosterItems.ContainsKey(rosterJid)) RosterItems[rosterJid] = new UserRosterItemDic();
                    RosterItems[rosterJid][item.Jid] = item;

                    return item;
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not save or update roster item.", e);
            }
        }

        public virtual void RemoveRosterItem(Jid rosterJid, Jid itemJid)
        {
            try
            {
                lock (syncRoot)
                {
                    rosterJid = new Jid(rosterJid.Bare.ToLowerInvariant());
                    itemJid = new Jid(itemJid.Bare.ToLowerInvariant());

                    if (RosterItems.ContainsKey(rosterJid) && RosterItems[rosterJid].ContainsKey(itemJid))
                    {
                        ExecuteNonQuery(new SqlDelete("jabber_roster").Where("jid", rosterJid.ToString()).Where("item_jid", itemJid.ToString()));
                        RosterItems[rosterJid].Remove(itemJid);
                    }
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not remove roster item.", e);
            }
        }

        #endregion

        private IDictionary<Jid, UserRosterItemDic> LoadRosterItems()
        {
            var items = new Dictionary<Jid, UserRosterItemDic>();

            ExecuteList(new SqlQuery("jabber_roster").Select("jid", "item_jid", "name", "subscription", "ask", "groups")).ForEach(r =>
                {
                    var item = new UserRosterItem(new Jid((string)r[1]))
                    {
                        Name = r[2] as string,
                        Subscribtion = (SubscriptionType)Convert.ToInt32(r[3]),
                        Ask = (AskType)Convert.ToInt32(r[4]),
                    };
                    if (r[5] != null)
                    {
                        item.Groups.AddRange(((string)r[5]).Split(new[] { GroupSeparator }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    var jid = new Jid((string)r[0]);
                    if (!items.ContainsKey(jid)) items[jid] = new UserRosterItemDic();
                    items[jid][item.Jid] = item;
                });

            return items;
        }
    }
}