/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

                    var apiHelper = new ApiHelper(httpContextScheme ?? Defines.DefaultApiSchema);

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
