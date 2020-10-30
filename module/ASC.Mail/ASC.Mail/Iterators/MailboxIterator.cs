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
