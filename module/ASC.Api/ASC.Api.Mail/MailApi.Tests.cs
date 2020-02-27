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


using System.Collections.Generic;
using System.IO;
using ASC.Api.Attributes;
using ASC.Mail.Data.Contracts;
using MailMessage = ASC.Mail.Data.Contracts.MailMessageData;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Create sample message [Tests]
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="mailboxId"></param>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="importance"></param>
        /// <param name="unread"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/create")]
        public int CreateSampleMessage(
            int? folderId,
            int? mailboxId,
            List<string> to,
            List<string> cc,
            List<string> bcc,
            bool importance,
            bool unread,
            string subject,
            string body)
        {
            var id = MailEngineFactory.TestEngine
                .CreateSampleMessage(folderId, mailboxId, to, cc, bcc, importance, unread, subject, body, add2Index: true);

            return id;
        }

        /// <summary>
        /// Append attachment to sample message [Tests]
        /// </summary>
        /// <param name="messageId">Id of any message</param>
        /// <param name="filename">File name</param>
        /// <param name="stream">File stream</param>
        /// <param name="contentType">File content type</param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/attachments/append")]
        public MailAttachmentData AppendAttachmentsToSampleMessage(
            int? messageId, string filename, Stream stream, string contentType)
        {
            var data = MailEngineFactory.TestEngine
                .AppendAttachmentsToSampleMessage(messageId, filename, stream, contentType);

            return data;
        }

        /// <summary>
        /// Load sample message from EML [Tests]
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="userFolderId"></param>
        /// <param name="mailboxId"></param>
        /// <param name="unread"></param>
        /// <param name="emlStream"></param>
        /// <returns>Id message</returns>
        /// <category>Tests</category>
        /// <visible>false</visible>
        [Create(@"messages/sample/eml/load")]
        public int LoadSampleMessage(
            int? folderId,
            uint? userFolderId,
            int? mailboxId,
            bool unread,
            Stream emlStream)
        {
            var id = MailEngineFactory.TestEngine
                .LoadSampleMessage(folderId, userFolderId, mailboxId, unread, emlStream, true);

            return id;
        }
    }
}
