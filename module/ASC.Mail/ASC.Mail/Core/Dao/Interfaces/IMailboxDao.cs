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


using System;
using System.Collections.Generic;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IMailboxDao
    {
        Mailbox GetMailBox(IMailboxExp exp);

        List<Mailbox> GetMailBoxes(IMailboxesExp exp);

        Mailbox GetNextMailBox(IMailboxExp exp);

        Tuple<int, int> GetRangeMailboxes(IMailboxExp exp);

        List<Tuple<int, string>> GetMailUsers(IMailboxExp exp);

        int SaveMailBox(Mailbox mailbox);

        bool SetMailboxRemoved(Mailbox mailbox);

        bool RemoveMailbox(Mailbox mailbox);

        bool Enable(IMailboxExp exp, bool enabled);

        bool SetNextLoginDelay(IMailboxExp exp, TimeSpan delay);

        bool SetMailboxEmailIn(Mailbox mailbox, string emailInFolder);

        bool SetMailboxesActivity(int tenant, string user, bool userOnline = true);

        bool SetMailboxInProcess(int id);

        bool SetMailboxProcessed(Mailbox mailbox, int nextLoginDelay, bool? enabled = null, int? messageCount = null,
            long? size = null, bool? quotaError = null, string oAuthToken = null, string imapIntervalsJson = null, bool? resetImapIntervals = false);

        bool SetMailboxAuthError(int id, DateTime? authErroDate);

        List<int> SetMailboxesProcessed(int timeoutInMinutes);

        bool CanAccessTo(IMailboxExp exp);

        MailboxStatus GetMailBoxStatus(IMailboxExp exp);
    }
}
