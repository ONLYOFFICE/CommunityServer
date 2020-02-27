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
