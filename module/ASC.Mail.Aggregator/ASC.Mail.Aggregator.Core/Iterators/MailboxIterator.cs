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
    interface IMailboxIterator
    {
        MailBox First();
        MailBox Next();
        bool IsDone { get; }
        MailBox Current { get; }
    }

    /// <summary>
    /// The 'ConcreteIterator' class
    /// </summary>
    public class MailboxIterator : IMailboxIterator
    {
        private readonly MailBoxManager _mailBoxManager;
        private readonly int _minMailboxId;
        private readonly int _maxMailboxId;

        // Constructor
        public MailboxIterator(MailBoxManager mailBoxManager)
        {
            _mailBoxManager = mailBoxManager;
            _mailBoxManager.GetMailboxesRange(out _minMailboxId, out _maxMailboxId);
        }

        // Gets first item
        public MailBox First()
        {
            Current = _mailBoxManager.GetMailBox(_minMailboxId);
            return Current;
        }

        // Gets next item
        public MailBox Next()
        {
            if (!IsDone)
            {
                Current = _mailBoxManager.GetNextMailBox(Current.MailBoxId);
                return Current;
            }

            return null;
        }

        // Gets current iterator item
        public MailBox Current { get; private set; }

        // Gets whether iteration is complete
        public bool IsDone
        {
            get { return _minMailboxId == 0 || _maxMailboxId < _minMailboxId || Current == null || Current.MailBoxId > _maxMailboxId; }
        }

    }
}
