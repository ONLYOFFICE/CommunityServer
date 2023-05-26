/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Creates a sample message with the parameters specified in the request. [Tests]
        /// </summary>
        /// <short>Create a sample message</short>
        /// <param type="System.Nullable{System.Int32}, System" name="folderId">Folder ID</param>
        /// <param type="System.Nullable{System.Int32}, System" name="mailboxId">Mailbox ID</param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="to">List of mail addresses to which a letter will be sent. <![CDATA[Format: Name<name@domain>]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="cc">List of Cc (carbon copy) mail addresses. <![CDATA[Format: Name<name@domain>]]></param>
        /// <param type="System.Collections.Generic.List{System.String}, System.Collections.Generic" name="bcc">List of Bcc (blind carbon copy) mail addresses. <![CDATA[Format: Name<name@domain>]]></param>
        /// <param type="System.Boolean, System" name="importance">Specifies if this message is important or not: true - important, false - not important</param>
        /// <param type="System.Boolean, System" name="unread">Message status: unread (true), read (false), or all (null) messages</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <param type="System.String, System" name="body">Message body as the HTML string</param>
        /// <returns>Message ID</returns>
        /// <category>Tests</category>
        /// <path>api/2.0/mail/messages/sample/create</path>
        /// <httpMethod>POST</httpMethod>
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
        /// Appends an attachment to the sample message with the ID specified in the request. [Tests]
        /// </summary>
        /// <short>Append an attachment to a sample message</short>
        /// <param type="System.Nullable{System.Int32}, System" name="messageId">Message ID</param>
        /// <param type="System.String, System" name="filename">File name</param>
        /// <param type="System.IO.Stream, System.IO" name="stream">File stream</param>
        /// <param type="System.String, System" name="contentType">File content type</param>
        /// <returns>Message attachment</returns>
        /// <category>Tests</category>
        /// <path>api/2.0/mail/messages/sample/attachments/append</path>
        /// <httpMethod>POST</httpMethod>
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
        /// Loads a sample message with the parameters specified in the request as EML. [Tests]
        /// </summary>
        /// <short>Load a sample message</short>
        /// <param type="System.Nullable{System.Int32}, System" name="folderId">Folder ID</param>
        /// <param type="System.Nullable{System.UInt32}, System" name="userFolderId">User folder ID</param>
        /// <param type="System.Nullable{System.Int32}, System" name="mailboxId">Mailbox ID</param>
        /// <param type="System.Boolean, System" name="unread">Message status: unread (true), read (false), or all (null) messages</param>
        /// <param type="System.IO.Stream, System.IO" name="emlStream">EML stream</param>
        /// <returns>Message ID</returns>
        /// <category>Tests</category>
        /// <path>api/2.0/mail/messages/sample/eml/load</path>
        /// <httpMethod>POST</httpMethod>
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
