/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.Linq;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Core.DbSchema.Tables;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Core.Dao.Expressions.Mailbox
{
    public class MailboxesForProcessingExp : IMailboxesExp
    {
        public string OrderBy { get; private set; }
        public bool? OrderAsc { get; private set; }
        public int? Limit { get; private set; }

        private TasksConfig TasksConfig { get; set; }
        private bool OnlyActive { get; set; }

        public MailboxesForProcessingExp(TasksConfig tasksConfig, int tasksLimit, bool active)
        {
            TasksConfig = tasksConfig;

            Limit = tasksLimit > 0 ? tasksLimit : (int?) null;

            OnlyActive = active;

            OrderBy = MailboxTable.Columns.DateChecked;
            OrderAsc = true;
        }

        private const string WHERE_LOGIN_DELAY_EXPIRED =
            MailboxTable.Columns.DateLoginDelayExpires + " < UTC_TIMESTAMP()";

        private const string DATE_USER_CHECKED_TIMESTAMP =
            "TIMESTAMPDIFF(SECOND, " + MailboxTable.Columns.DateUserChecked + ", UTC_TIMESTAMP())";

        public Exp GetExpression()
        {
            var exp = Exp.Eq(MailboxTable.Columns.IsProcessed, false)
                      & Exp.Sql(WHERE_LOGIN_DELAY_EXPIRED)
                      & Exp.Eq(MailboxTable.Columns.IsRemoved, false)
                      & Exp.Eq(MailboxTable.Columns.Enabled, true);

            if (TasksConfig.AggregateMode != TasksConfig.AggregateModeType.All)
            {
                exp = exp &
                      Exp.Eq(MailboxTable.Columns.IsServerMailbox,
                          TasksConfig.AggregateMode == TasksConfig.AggregateModeType.Internal);
            }

            if (TasksConfig.EnableSignalr)
            {
                exp = exp & Exp.Eq(MailboxTable.Columns.UserOnline, OnlyActive);
            }
            else
            {
                exp = exp & Exp.Or(Exp.Eq(MailboxTable.Columns.DateUserChecked, null), OnlyActive
                    ? Exp.Lt(DATE_USER_CHECKED_TIMESTAMP, TasksConfig.ActiveInterval.Seconds)
                    : Exp.Gt(DATE_USER_CHECKED_TIMESTAMP, TasksConfig.ActiveInterval.Seconds));
            }

            if (TasksConfig.WorkOnUsersOnly.Any())
            {
                exp = exp & Exp.In(MailboxTable.Columns.User, TasksConfig.WorkOnUsersOnly);
            }

            return exp;
        }
    }
}