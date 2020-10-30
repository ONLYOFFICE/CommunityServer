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
