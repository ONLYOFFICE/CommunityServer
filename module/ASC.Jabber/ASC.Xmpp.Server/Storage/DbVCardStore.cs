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
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Server.Storage.Interface;
using System;
using System.Collections.Generic;
using System.Data;

namespace ASC.Xmpp.Server.Storage
{
    public class DbVCardStore : DbStoreBase, IVCardStore
    {
        private IDictionary<string, Vcard> vcardsCache = new Dictionary<string, Vcard>();


        protected override SqlCreate[] GetCreateSchemaScript()
        {
            var t1 = new SqlCreate.Table("jabber_vcard", true)
                .AddColumn("jid", DbType.String, 255, true)
                .AddColumn("vcard", DbType.String, UInt16.MaxValue, true)
                .PrimaryKey("jid");
            return new[] { t1 };
        }


        public virtual void SetVCard(Jid jid, Vcard vcard)
        {
            if (jid == null) throw new ArgumentNullException("jid");
            if (vcard == null) throw new ArgumentNullException("vcard");

            try
            {
                lock (vcardsCache)
                {
                    ExecuteNonQuery(
                        new SqlInsert("jabber_vcard", true)
                        .InColumnValue("jid", jid.Bare.ToLowerInvariant())
                        .InColumnValue("vcard", ElementSerializer.SerializeElement(vcard)));
                    vcardsCache[jid.Bare.ToLowerInvariant()] = vcard;
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not save VCard.", e);
            }
        }

        public virtual Vcard GetVCard(Jid jid, string id = "")
        {
            if (jid == null) throw new ArgumentNullException("jid");

            try
            {
                lock (vcardsCache)
                {
                    var bareJid = jid.Bare.ToLowerInvariant();
                    if (!vcardsCache.ContainsKey(bareJid))
                    {
                        var vcardStr = ExecuteScalar<string>(new SqlQuery("jabber_vcard").Select("vcard").Where("jid", bareJid));
                        vcardsCache[bareJid] = !string.IsNullOrEmpty(vcardStr) ? ElementSerializer.DeSerializeElement<Vcard>(vcardStr) : null;
                    }
                    return vcardsCache[bareJid];
                }
            }
            catch (Exception e)
            {
                throw new JabberException("Could not get VCard.", e);
            }
        }

        public virtual ICollection<Vcard> Search(Vcard pattern)
        {
            return new Vcard[0];
        }
    }
}
