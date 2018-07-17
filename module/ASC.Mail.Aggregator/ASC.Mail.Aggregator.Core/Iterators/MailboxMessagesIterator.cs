/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Mail.Aggregator.Common;

namespace ASC.Mail.Aggregator.Core.Iterators
{
    /// <summary>
    /// The 'Iterator' interface
    /// </summary>
    interface IMailboxMessagesIterator
    {
        MailMessage First(bool unremoved = false);
        MailMessage Next(bool unremoved = false);
        bool IsDone { get; }
        MailMessage Current { get; }
    }

    /// <summary>
    /// The 'ConcreteIterator' class
    /// </summary>
    public class MailboxMessagesIterator : IMailboxMessagesIterator
    {
        private readonly MailBoxManager _mailBoxManager;
        private readonly MailBox _currentMailBox;
        private readonly int _minMessageId;
        private readonly int _maxMessageId;

        // Constructor
        public MailboxMessagesIterator(MailBox mailBox, MailBoxManager mailBoxManager)
        {
            _currentMailBox = mailBox;
            _mailBoxManager = mailBoxManager;
            _mailBoxManager.GetMailboxMessagesRange(_currentMailBox, out _minMessageId, out _maxMessageId);
        }

        // Gets first item
        public MailMessage First(bool onlyUnremoved = false)
        {
            Current = _mailBoxManager.GetMailInfo(_currentMailBox.TenantId, _currentMailBox.UserId, _minMessageId,
                new MailMessage.Options
                {
                    LoadImages = false,
                    LoadBody = false,
                    NeedProxyHttp = false,
                    OnlyUnremoved = onlyUnremoved
                });
            return Current;
        }

        // Gets next item
        public MailMessage Next(bool onlyUnremoved = false)
        {
            if (!IsDone)
            {
                Current = _mailBoxManager.GetNextMailBoxNessage(_currentMailBox, (int)Current.Id, onlyUnremoved);
                return Current;
            }

            return null;
        }

        // Gets current iterator item
        public MailMessage Current { get; private set; }

        // Gets whether iteration is complete
        public bool IsDone
        {
            get { return _minMessageId == 0 || _maxMessageId < _minMessageId || Current == null || Current.Id > _maxMessageId; }
        }

    }
}
