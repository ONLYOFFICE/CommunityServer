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
using ASC.Mail.Core.Entities;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;

namespace ASC.Mail.Core.Engine
{
    public class SignatureEngine
    {
        public int Tenant { get; private set; }
        public string User { get; private set; }
        public ILog Log { get; private set; }

        public SignatureEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;
            Log = log ?? LogManager.GetLogger("ASC.Mail.SignatureEngine");
        }

        public MailSignatureData SaveSignature(int mailboxId, string html, bool isActive)
        {
            if (!string.IsNullOrEmpty(html))
            {
                var imagesReplacer = new StorageManager(Tenant, User, Log);
                html = imagesReplacer.ChangeEditorImagesLinks(html, mailboxId);
            }

            CacheEngine.Clear(User);

            var signature = new MailboxSignature
            {
                MailboxId = mailboxId,
                Tenant = Tenant,
                Html = html,
                IsActive = isActive
            };

            using (var daoFactory = new DaoFactory())
            {
                var result = daoFactory
                    .CreateMailboxSignatureDao(Tenant, User)
                    .SaveSignature(signature);

                if (result <= 0)
                    throw new Exception("Save failed");
            }

            return ToMailMailSignature(signature);
        }

        protected MailSignatureData ToMailMailSignature(MailboxSignature signature)
        {
            return new MailSignatureData(signature.MailboxId, signature.Tenant, signature.Html, signature.IsActive);
        }
    }
}
