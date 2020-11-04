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
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Data.Storage
{
    public static class AttachmentManager
    {
        public static AttachmentStream ToAttachmentStream(this MailAttachmentData mailAttachmentData, int offset = 0)
        {
            if (mailAttachmentData == null) 
                throw new InvalidOperationException("Attachment not found");

            var storage = MailDataStore.GetDataStore(mailAttachmentData.tenant);
            var attachmentPath = MailStoragePathCombiner.GerStoredFilePath(mailAttachmentData);
            var result = new AttachmentStream
            {
                FileStream = storage.GetReadStream("", attachmentPath, offset),
                FileName = mailAttachmentData.fileName,
                FileSize = mailAttachmentData.size
            };
            return result;
        }
    }
}
