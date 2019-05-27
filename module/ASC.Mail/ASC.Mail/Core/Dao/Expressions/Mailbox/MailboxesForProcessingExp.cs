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