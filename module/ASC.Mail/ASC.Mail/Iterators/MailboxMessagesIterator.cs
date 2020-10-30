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


using ASC.Mail.Core;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Iterators
{
    /// <summary>
    /// The 'Iterator' interface
    /// </summary>
    internal interface IMailboxMessagesIterator
    {
        MailMessageData First(bool unremoved = false);
        MailMessageData Next(bool unremoved = false);
        bool IsDone { get; }
        MailMessageData Current { get; }
    }

    /// <summary>
    /// The 'ConcreteIterator' class
    /// </summary>
    public class MailboxMessagesIterator : IMailboxMessagesIterator
    {
        private EngineFactory MailEngine { get; set; }
        private readonly int _minMessageId;
        private readonly int _maxMessageId;

        // Constructor
        public MailboxMessagesIterator(MailBoxData mailBoxData)
        {
            MailEngine = new EngineFactory(mailBoxData.TenantId, mailBoxData.UserId);
            var range = MailEngine
                .MessageEngine
                .GetRangeMessages(
                    SimpleMessagesExp.CreateBuilder(mailBoxData.TenantId)
                        .SetMailboxId(mailBoxData.MailBoxId)
                        .Build());

            _minMessageId = range.Item1;
            _maxMessageId = range.Item2;
        }

        // Gets first item
        public MailMessageData First(bool onlyUnremoved = false)
        {
            Current = MailEngine.MessageEngine.GetMessage(_minMessageId,
                new MailMessageData.Options
                {
                    LoadImages = false,
                    LoadBody = false,
                    NeedProxyHttp = false,
                    OnlyUnremoved = onlyUnremoved
                });

            return Current;
        }

        // Gets next item
        public MailMessageData Next(bool onlyUnremoved = false)
        {
            if (IsDone) 
                return null;

            Current = MailEngine.MessageEngine.GetNextMessage(Current.Id,
                new MailMessageData.Options
                {
                    LoadImages = false,
                    LoadBody = false,
                    NeedProxyHttp = false,
                    OnlyUnremoved = onlyUnremoved
                });

            return Current;
        }

        // Gets current iterator item
        public MailMessageData Current { get; private set; }

        // Gets whether iteration is complete
        public bool IsDone
        {
            get { return _minMessageId == 0 || _maxMessageId < _minMessageId || Current == null || Current.Id > _maxMessageId; }
        }

    }
}
