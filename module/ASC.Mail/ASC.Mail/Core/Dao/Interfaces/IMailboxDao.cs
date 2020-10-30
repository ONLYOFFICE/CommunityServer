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
