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

using System;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ActiveUp.Net.Mail;

namespace ASC.Mail.Aggregator.Contacts
{
    internal static class MailContactsManager
    {
        public const int fulltextsearch_ids_count = 200;

        public static int MailContactExists(IDbManager db, int tenant, string user_id, string name, string address)
        {
            return db.ExecuteScalar<int>(
                new SqlQuery(ContactsTable.name)
                    .Select(ContactsTable.Columns.id)
                    .Where(ContactsTable.Columns.id_user, user_id)
                    .Where(ContactsTable.Columns.id_tenant, tenant)
                    .Where(ContactsTable.Columns.name, name)
                    .Where(ContactsTable.Columns.address, address));
        }

        public static void SaveMailContacts(IDbManager db, int tenant, string user, Message message, ILogger log)
        {
            try
            {
                var contacts = new AddressCollection {message.From};
                contacts.AddRange(message.To);
                contacts.AddRange(message.Cc);
                contacts.AddRange(message.Bcc);

                var valid_contacts = (from contact in contacts
                                      where MailContactExists(db, tenant, user, contact.Name, contact.Email) < 1
                                      select contact).ToList();

                if (!valid_contacts.Any()) return;

                var last_modified = DateTime.Now;

                var insert_query = new SqlInsert(ContactsTable.name)
                    .InColumns(ContactsTable.Columns.id_user,
                               ContactsTable.Columns.id_tenant,
                               ContactsTable.Columns.name,
                               ContactsTable.Columns.address,
                               ContactsTable.Columns.last_modified);

                valid_contacts
                    .ForEach(contact =>
                             insert_query
                                 .Values(user, tenant, contact.Name, contact.Email, last_modified));

                db.ExecuteNonQuery(insert_query);
            }
            catch (Exception e)
            {
                log.Warn("SaveMailContacts(tenant={0}, userId='{1}', mail_id={2}) Exception:\r\n{3}\r\n",
                          tenant, user, message.Id, e.ToString());
            }
        }
    }
}
