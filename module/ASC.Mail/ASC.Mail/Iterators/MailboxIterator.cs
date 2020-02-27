/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Logging;
using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Core.Engine;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Iterators
{
    /// <summary>
    /// The 'ConcreteIterator' class
    /// </summary>
    public class MailboxIterator : IMailboxIterator
    {
        private int Tenant { get; set; }
        private string UserId { get; set; }
        public bool? IsRemoved { get; private set; }
        public ILog Log { get; set; }

        public int MinMailboxId { get; private set; }
        public int MaxMailboxId { get; private set; }

        private MailboxEngine MailboxEngine { get; set; }

        // Constructor
        public MailboxIterator(int tenant = -1, string userId = null, bool? isRemoved = false, ILog log = null)
        {
            if (!string.IsNullOrEmpty(userId) && tenant < 0)
                throw new ArgumentException("Tenant must be initialized if user not empty");

            var engine = new EngineFactory(tenant, userId, log);

            MailboxEngine = engine.MailboxEngine;

            Tenant = tenant;
            UserId = userId;
            IsRemoved = isRemoved;

            Log = log ?? new NullLog();

            var result = MailboxEngine.GetRangeMailboxes(GetMailboxExp(Tenant, UserId, IsRemoved));

            if (result == null)
                return;

            MinMailboxId = result.Item1;
            MaxMailboxId = result.Item2;

            Current = null;
        }

        // Gets first item
        public MailBoxData First()
        {
            if (MinMailboxId == 0 && MinMailboxId == MaxMailboxId)
            {
                return null;
            }

            var exp = GetMailboxExp(MinMailboxId, Tenant, UserId, IsRemoved);
            var mailbox = MailboxEngine.GetMailboxData(exp);

            Current = mailbox == null && MinMailboxId < MaxMailboxId
                ? GetNextMailbox(MinMailboxId)
                : mailbox;

            return Current;
        }

        // Gets next item
        public MailBoxData Next()
        {
            if (IsDone)
                return null;

            Current = GetNextMailbox(Current.MailBoxId);

            return Current;
        }

        // Gets current iterator item
        public MailBoxData Current { get; private set; }

        // Gets whether iteration is complete
        public bool IsDone
        {
            get
            {
                return MinMailboxId == 0
                       || MinMailboxId > MaxMailboxId
                       || Current == null;
            }
        }

        private MailBoxData GetNextMailbox(int id)
        {
            do
            {
                if (id < MinMailboxId || id >= MaxMailboxId)
                    return null;

                MailBoxData mailbox;

                var exp = GetNextMailboxExp(id, Tenant, UserId, IsRemoved);

                int failedId;

                if (!MailboxEngine.TryGetNextMailboxData(exp, out mailbox, out failedId))
                {
                    if (failedId > 0)
                    {
                        id = failedId;

                        Log.ErrorFormat("MailboxEngine.GetNextMailboxData(Mailbox id = {0}) failed. Skip it.", id);

                        id++;
                    }
                    else
                    {
                        Log.ErrorFormat("MailboxEngine.GetNextMailboxData(Mailbox id = {0}) failed. End seek next.", id);
                        return null;
                    }
                }
                else
                {
                    return mailbox;
                }

            } while (id <= MaxMailboxId);

            return null;
        }

        private static IMailboxExp GetMailboxExp(int tenant = -1, string user = null, bool? isRemoved = false)
        {
            IMailboxExp mailboxExp;

            if (!string.IsNullOrEmpty(user) && tenant > -1)
            {
                mailboxExp = new UserMailboxExp(tenant, user, isRemoved);
            }
            else if (tenant > -1)
            {
                mailboxExp = new TenantMailboxExp(tenant, isRemoved);
            }
            else if (!string.IsNullOrEmpty(user))
            {
                throw new ArgumentException("Tenant must be initialized if user not empty");
            }
            else
            {
                mailboxExp = new SimpleMailboxExp(isRemoved);
            }

            return mailboxExp;
        }

        private static IMailboxExp GetMailboxExp(int id, int tenant, string user = null, bool? isRemoved = false)
        {
            IMailboxExp mailboxExp;

            if (!string.IsNullOrEmpty(user) && tenant > -1)
            {
                mailboxExp = new СoncreteUserMailboxExp(id, tenant, user, isRemoved);
            }
            else if (tenant > -1)
            {
                mailboxExp = new ConcreteTenantMailboxExp(id, tenant, isRemoved);
            }
            else if (!string.IsNullOrEmpty(user))
            {
                throw new ArgumentException("Tenant must be initialized if user not empty");
            }
            else
            {
                mailboxExp = new ConcreteSimpleMailboxExp(id, isRemoved);
            }

            return mailboxExp;
        }

        private static IMailboxExp GetNextMailboxExp(int id, int tenant, string user = null, bool? isRemoved = false)
        {
            IMailboxExp mailboxExp;

            if (!string.IsNullOrEmpty(user) && tenant > -1)
            {
                mailboxExp = new СoncreteUserNextMailboxExp(id, tenant, user, isRemoved);
            }
            else if (tenant > -1)
            {
                mailboxExp = new ConcreteTenantNextMailboxExp(id, tenant, isRemoved);
            }
            else if (!string.IsNullOrEmpty(user))
            {
                throw new ArgumentException("Tenant must be initialized if user not empty");
            }
            else
            {
                mailboxExp = new ConcreteSimpleNextMailboxExp(id, isRemoved);
            }

            return mailboxExp;
        }
    }
}
