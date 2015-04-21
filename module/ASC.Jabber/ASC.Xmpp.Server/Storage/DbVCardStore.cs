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
