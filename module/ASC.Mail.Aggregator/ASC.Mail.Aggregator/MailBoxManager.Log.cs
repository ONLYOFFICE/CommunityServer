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
using ASC.Common.Data.Sql;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        // ReSharper disable InconsistentNaming
        private const string MAIL_AGGREGATORS = "mail_aggregators";
        private const string MAIL_LOG = "mail_log";
        // ReSharper restore InconsistentNaming

        private struct MailAggregatorFields
        {
            public const string id = "id";
            public const string ip = "ip";
            public const string start_work_time = "start_work";
            public const string finish_work_time = "end_work";
        }

        private struct MailLogFields
        {
            public const string id = "id";
            public const string id_aggregator = "id_aggregator";
            public const string id_thread = "id_thread";
            public const string id_mailbox = "id_mailbox";
            public const string proccessing_start_time = "processing_start_time";
            public const string proccessing_end_time = "processing_end_time";
            public const string proccessed_mails_count = "processed_mails_count";
        }

        public int RegisterAggregator(string aggregator_ip)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(new SqlInsert(MAIL_AGGREGATORS)
                                   .InColumnValue(MailAggregatorFields.id, 0)
                                   .InColumnValue(MailAggregatorFields.ip, aggregator_ip)
                                   .InColumnValue(MailAggregatorFields.start_work_time, DateTime.Now)
                                   .Identity(0, 0, true)
                                   );
            }
        }

        public void UnregisterAggregator(int aggregator_id)
        {
            using (var db = GetDb())
            {
                db.ExecuteList(new SqlUpdate(MAIL_AGGREGATORS)
                                   .Set(MailAggregatorFields.finish_work_time, DateTime.Now)
                                   .Where(MailAggregatorFields.id, aggregator_id));
            }
        }

        public long RegisterMailBoxProccessing(int mailbox_id, int thread_id, int aggregator_id)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<long>(new SqlInsert(MAIL_LOG)
                                   .InColumnValue(MailLogFields.id, 0)
                                   .InColumnValue(MailLogFields.id_aggregator, aggregator_id)
                                   .InColumnValue(MailLogFields.id_mailbox, mailbox_id)
                                   .InColumnValue(MailLogFields.id_thread, thread_id)
                                   .InColumnValue(MailLogFields.proccessing_start_time, DateTime.Now)
                                   .Identity(0, 0, true)
                                   );
            }
        }

        public void RegisterFinishMailBoxProccessing(long record_id, int? proccessed_message_count)
        {
            using (var db = GetDb())
            {
                db.ExecuteList(new SqlUpdate(MAIL_LOG)
                                   .Set(MailLogFields.proccessing_end_time, DateTime.Now)
                                   .Set(MailLogFields.proccessed_mails_count, proccessed_message_count)
                                   .Where(MailLogFields.id, record_id));
            }
        }
    }
}
