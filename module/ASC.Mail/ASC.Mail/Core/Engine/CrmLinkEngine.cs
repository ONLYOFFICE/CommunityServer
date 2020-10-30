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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ASC.Common.Logging;
using ASC.CRM.Core;
using ASC.Mail.Core.Dao.Expressions.Message;
using ASC.Mail.Data.Contracts;
using ASC.Mail.Data.Storage;
using ASC.Mail.Exceptions;
using ASC.Mail.Utils;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Mail.Core.Engine
{
    public class CrmLinkEngine
    {
        public int Tenant { get; private set; }

        public string User { get; private set; }

        public ILog Log { get; private set; }

        public CrmLinkEngine(int tenant, string user, ILog log = null)
        {
            Tenant = tenant;
            User = user;

            Log = log ?? LogManager.GetLogger("ASC.Mail.CrmLinkEngine");
        }

        public List<CrmContactData> GetLinkedCrmEntitiesId(int messageId)
        {
            using (var daoFactory = new DaoFactory())
            {
                var daoCrmLink = daoFactory.CreateCrmLinkDao(Tenant, User);

                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User));

                return daoCrmLink.GetLinkedCrmContactEntities(mail.ChainId, mail.MailboxId);
            }
        }

        public void LinkChainToCrm(int messageId, List<CrmContactData> contactIds, string httpContextScheme)
        {
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<CRM.Core.Dao.DaoFactory>();
                foreach (var crmContactEntity in contactIds)
                {
                    switch (crmContactEntity.Type)
                    {
                        case CrmContactData.EntityTypes.Contact:
                            var crmContact = factory.ContactDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmContact);
                            break;
                        case CrmContactData.EntityTypes.Case:
                            var crmCase = factory.CasesDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmCase);
                            break;
                        case CrmContactData.EntityTypes.Opportunity:
                            var crmOpportunity = factory.DealDao.GetByID(crmContactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmOpportunity);
                            break;
                    }
                }
            }

            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var daoMail = daoFactory.CreateMailDao(Tenant, User);

                var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User));

                var daoMailInfo = daoFactory.CreateMailInfoDao(Tenant, User);

                var chainedMessages = daoMailInfo.GetMailInfoList(
                    SimpleMessagesExp.CreateBuilder(Tenant, User)
                        .SetChainId(mail.ChainId)
                        .Build());

                    if (!chainedMessages.Any())
                        return;

                var linkingMessages = new List<MailMessageData>();

                var engine = new EngineFactory(Tenant, User);

                foreach (var chainedMessage in chainedMessages)
                {
                    var message = engine.MessageEngine.GetMessage(chainedMessage.Id,
                        new MailMessageData.Options
                        {
                            LoadImages = true,
                            LoadBody = true,
                            NeedProxyHttp = false
                        });

                    message.LinkedCrmEntityIds = contactIds;

                    linkingMessages.Add(message);

                }

                var daoCrmLink = daoFactory.CreateCrmLinkDao(Tenant, User);

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    daoCrmLink.SaveCrmLinks(mail.ChainId, mail.MailboxId, contactIds);

                    foreach (var message in linkingMessages)
                    {
                        try
                        {
                            AddRelationshipEvents(message, httpContextScheme);
                        }
                        catch (ApiHelperException ex)
                        {
                            if (!ex.Message.Equals("Already exists"))
                                throw;
                        }
                    }

                    tx.Commit();
                }
            }
        }

        public void MarkChainAsCrmLinked(int messageId, List<CrmContactData> contactIds)
        {
            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var daoCrmLink = daoFactory.CreateCrmLinkDao(Tenant, User);

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoMail = daoFactory.CreateMailDao(Tenant, User);

                    var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User));

                    daoCrmLink.SaveCrmLinks(mail.ChainId, mail.MailboxId, contactIds);

                    tx.Commit();
                }
            }
        }

        public void UnmarkChainAsCrmLinked(int messageId, IEnumerable<CrmContactData> contactIds)
        {
            using (var daoFactory = new DaoFactory())
            {
                var db = daoFactory.DbManager;

                var daoCrmLink = daoFactory.CreateCrmLinkDao(Tenant, User);

                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var daoMail = daoFactory.CreateMailDao(Tenant, User);

                    var mail = daoMail.GetMail(new ConcreteUserMessageExp(messageId, Tenant, User));

                    daoCrmLink.RemoveCrmLinks(mail.ChainId, mail.MailboxId, contactIds);

                    tx.Commit();
                }
            }
        }

        public void ExportMessageToCrm(int messageId, IEnumerable<CrmContactData> crmContactIds)
        {
            if (messageId < 0)
                throw new ArgumentException(@"Invalid message id", "messageId");
            if (crmContactIds == null)
                throw new ArgumentException(@"Invalid contact ids list", "crmContactIds");

            var engine = new EngineFactory(Tenant, User);

            var messageItem = engine.MessageEngine.GetMessage(messageId, new MailMessageData.Options
            {
                LoadImages = true,
                LoadBody = true,
                NeedProxyHttp = false
            });

            messageItem.LinkedCrmEntityIds = crmContactIds.ToList();

            AddRelationshipEvents(messageItem);
        }

        public void AddRelationshipEventForLinkedAccounts(MailBoxData mailbox, MailMessageData messageItem, string httpContextScheme)
        {
            try
            {
                using (var daoFactory = new DaoFactory())
                {
                    var dao = daoFactory.CreateCrmLinkDao(mailbox.TenantId, mailbox.UserId);

                    messageItem.LinkedCrmEntityIds = dao.GetLinkedCrmContactEntities(messageItem.ChainId, mailbox.MailBoxId);
                }

                if (!messageItem.LinkedCrmEntityIds.Any()) return;

                AddRelationshipEvents(messageItem, httpContextScheme);
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("Problem with adding history event to CRM. mailId={0}", messageItem.Id), ex);
            }
        }

        public void AddRelationshipEvents(MailMessageData message, string httpContextScheme = null)
        {
            using (var scope = DIHelper.Resolve())
            {
                var factory = scope.Resolve<CRM.Core.Dao.DaoFactory>();
                foreach (var contactEntity in message.LinkedCrmEntityIds)
                {
                    switch (contactEntity.Type)
                    {
                        case CrmContactData.EntityTypes.Contact:
                            var crmContact = factory.ContactDao.GetByID(contactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmContact);
                            break;
                        case CrmContactData.EntityTypes.Case:
                            var crmCase = factory.CasesDao.GetByID(contactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmCase);
                            break;
                        case CrmContactData.EntityTypes.Opportunity:
                            var crmOpportunity = factory.DealDao.GetByID(contactEntity.Id);
                            CRMSecurity.DemandAccessTo(crmOpportunity);
                            break;
                    }

                    var fileIds = new List<object>();

                    var apiHelper = new ApiHelper(httpContextScheme ?? Defines.DefaultApiSchema, Log);

                    foreach (var attachment in message.Attachments.FindAll(attach => !attach.isEmbedded))
                    {
                        if (attachment.dataStream != null)
                        {
                            attachment.dataStream.Seek(0, SeekOrigin.Begin);

                            var uploadedFileId = apiHelper.UploadToCrm(attachment.dataStream, attachment.fileName,
                                attachment.contentType, contactEntity);

                            if (uploadedFileId != null)
                            {
                                fileIds.Add(uploadedFileId);
                            }
                        }
                        else
                        {
                            using (var file = attachment.ToAttachmentStream())
                            {
                                var uploadedFileId = apiHelper.UploadToCrm(file.FileStream, file.FileName,
                                    attachment.contentType, contactEntity);
                                
                                if (uploadedFileId != null)
                                {
                                    fileIds.Add(uploadedFileId);
                                }
                            }
                        }
                    }

                    apiHelper.AddToCrmHistory(message, contactEntity, fileIds);

                    Log.InfoFormat(
                        "CrmLinkEngine->AddRelationshipEvents(): message with id = {0} has been linked successfully to contact with id = {1}",
                        message.Id, contactEntity.Id);
                }
            }
        }
    }
}
