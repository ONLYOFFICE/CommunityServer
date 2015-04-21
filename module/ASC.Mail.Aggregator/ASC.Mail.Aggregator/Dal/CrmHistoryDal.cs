/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.DataStorage;

namespace ASC.Mail.Aggregator.Dal
{
    public class CrmHistoryDal
    {
        [DataContract]
        internal class CrmHistoryContent
        {
            [DataMember]
            public long message_id;
        }

        private readonly int _tenant;
        private readonly Guid _user;

        private int Tenant { get { return _tenant; } }
        private Guid User { get { return _user; } }

        public CrmHistoryDal(int tenant, string user)
        {
            _tenant = tenant;
            _user = new Guid(user);

            if (SecurityContext.IsAuthenticated) return;

            CoreContext.TenantManager.SetCurrentTenant(Tenant);
            SecurityContext.AuthenticateMe(User);
        }

        public void AddRelationshipEvents(MailMessageItem item)
        {
            var factory = new DaoFactory(CoreContext.TenantManager.GetCurrentTenant().TenantId, CRMConstants.DatabaseId);
            foreach (var contactEntity in item.LinkedCrmEntityIds)
            {
                switch (contactEntity.Type)
                {
                    case ChainXCrmContactEntity.EntityTypes.Contact:
                        var crmContact = factory.GetContactDao().GetByID(contactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmContact);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Case:
                        var crmCase = factory.GetCasesDao().GetByID(contactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmCase);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Opportunity:
                        var crmOpportunity = factory.GetDealDao().GetByID(contactEntity.Id);
                        CRMSecurity.DemandAccessTo(crmOpportunity);
                        break;
                }

                var fileIds = new List<int>();

                foreach (var attachment in item.Attachments.FindAll(attach => !attach.isEmbedded))
                {
                    using (var file = AttachmentManager.GetAttachmentStream(attachment))
                    {
                        var uploadedFileId = ApiHelper.UploadToCrm(file.FileStream, file.FileName,
                                                                       attachment.contentType, contactEntity);
                        if (uploadedFileId > 0)
                        {
                            fileIds.Add(uploadedFileId);
                        }
                    }
                }

                var contentString = GetHistoryContentJson(item);

                ApiHelper.AddToCrmHistory(item, contactEntity, contentString, fileIds);
            }
        }

        private static string GetHistoryContentJson(MailMessageItem item)
        {
            string contentString;

            var contentStruct = new CrmHistoryContent
                {
                    message_id = item.Id
                };

            var serializer = new DataContractJsonSerializer(typeof (CrmHistoryContent));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, contentStruct);
                contentString = Encoding.UTF8.GetString(stream.GetCorrectBuffer());
            }
            return contentString;
        }
    }
}
