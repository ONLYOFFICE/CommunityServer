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


using ASC.Api.Attributes;
using ASC.Mail.Aggregator.Common;
using System;
using ASC.Mail.Aggregator.Common.DataStorage;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// This method needed for update or create autoreply.
        /// </summary>
        /// <param name="mailboxId">Id of updated mailbox.</param>
        /// <param name="turnOn">New autoreply status.</param>
        /// <param name="onlyContacts">If true then send autoreply only for contacts.</param>
        /// <param name="turnOnToDate">If true then field To is active.</param>
        /// <param name="fromDate">Start date of autoreply sending.</param>
        /// <param name="toDate">End date of autoreply sending.</param>
        /// <param name="subject">New autoreply subject.</param>
        /// <param name="html">New autoreply value.</param>
        [Create(@"autoreply/update/{mailboxId:[0-9]+}")]
        public MailAutoreply UpdateAutoreply(int mailboxId, bool turnOn, bool onlyContacts,
            bool turnOnToDate, DateTime fromDate, DateTime toDate, string subject, string html)
        {
            if (fromDate == DateTime.MinValue) throw new ArgumentException("Invalid parameter", "from");
            if (turnOnToDate && toDate == DateTime.MinValue) throw new ArgumentException("Invalid parameter", "to");
            if (turnOnToDate && toDate < fromDate) throw new ArgumentException("Wrong date interval, toDate < fromDate", "to, from");
            if (String.IsNullOrEmpty(html)) throw new ArgumentException("Invalid parameter", "html");

            var imagesReplacer = new StorageManager(TenantId, Username);
            html = imagesReplacer.ChangeEditorImagesLinks(html, mailboxId);

            MailBoxManager.CachedAccounts.Clear(Username);

            return MailBoxManager.UpdateOrCreateMailboxAutoreply(mailboxId, Username,
                TenantId, turnOn, onlyContacts, turnOnToDate, fromDate, toDate, subject, html);
        }
    }
}
