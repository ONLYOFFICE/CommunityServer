/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Configuration;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Core;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Specific;
using System.Xml.XPath;

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

        private const int MailCrmHistoryCategory = -3;

        private readonly MailBoxManager _manager;
        private readonly string _baseUrl;
        private readonly int _tenant_id;
        private readonly Guid _user_id;

        private string BaseUrl{get { return _baseUrl; }}
        private MailBoxManager Manager { get { return _manager; } }
        private int TenantId { get { return _tenant_id; } }
        private Guid UserId { get { return _user_id; } }

        public CrmHistoryDal(MailBoxManager manager, int tenant_id, string user_id)
        {
            _manager = manager;
            _baseUrl = WebConfigurationManager.AppSettings["api.url"].Trim('~', '/');
            _tenant_id = tenant_id;
            _user_id = new Guid(user_id);
        }

        public void AddRelationshipEvents(MailMessageItem item)
        {
            CoreContext.TenantManager.SetCurrentTenant(TenantId);
            SecurityContext.AuthenticateMe(CoreContext.Authentication.GetAccountByID(_user_id));
            var factory = new DaoFactory(CoreContext.TenantManager.GetCurrentTenant().TenantId, CRMConstants.DatabaseId);
            foreach (var contact_entity in item.LinkedCrmEntityIds)
            {
                switch (contact_entity.Type)
                {
                    case ChainXCrmContactEntity.EntityTypes.Contact:
                        var crm_contact = factory.GetContactDao().GetByID(contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_contact);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Case:
                        var crm_case = factory.GetCasesDao().GetByID(contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_case);
                        break;
                    case ChainXCrmContactEntity.EntityTypes.Opportunity:
                        var crm_opportunity = factory.GetDealDao().GetByID(contact_entity.Id);
                        CRMSecurity.DemandAccessTo(crm_opportunity);
                        break;
                }

                var file_ids = StoreAttachmentsToCrm(item, contact_entity);
                var content_string = GetHistoryContentJson(item);
                AddRelationshipEventWithCrmApi(item, contact_entity, content_string, file_ids);
            }
        }

        private List<int> StoreAttachmentsToCrm(MailMessageItem item, CrmContactEntity entity)
        {
            var file_ids = new List<int>();
            foreach (var attachment in item.Attachments.FindAll(attach => !attach.isEmbedded))
            {
                using (var file = AttachmentManager.GetAttachmentStream(attachment))
                {
                    var uploaded_file_id = UploadFileToCrm(file.FileStream, file.FileName, attachment.contentType, entity);
                    if (uploaded_file_id > 0)
                    {
                        file_ids.Add(uploaded_file_id);
                    }
                }
            }
            return file_ids;
        }

        private int UploadFileToCrm(Stream file, string filename, string content_type, CrmContactEntity entity)
        {
            var post_parameters = new Dictionary<string, object>
            {
                {"entityType", entity.Type.StringName()},
                {"enitityid", entity.Id},
                {"storeOriginalFileFlag", true}
            };

            var request_uri_builder = GetUploadToCrmUrl(entity.Id, entity.Type.StringName());
            var auth_cookie = ApiHelper.GetAuthCookie(UserId, request_uri_builder.Host);

            post_parameters.Add("file", new FormUpload.FileParameter(file, filename, content_type));
            var responce = FormUpload.MultipartFormDataPost(request_uri_builder.Uri.ToString(), "", post_parameters, auth_cookie);
            var uploaded_file_id = ParseUploadResponse(responce);
            return uploaded_file_id;
        }

        private static int ParseUploadResponse(HttpWebResponse responce)
        {
            if (responce != null)
            {
                var responce_stream = responce.GetResponseStream();
                if (responce_stream != null)
                {
                    var xdoc = new XPathDocument(responce_stream);
                    var navigator = xdoc.CreateNavigator();
                    var res = navigator.SelectSingleNode("/result/response/id");
                    if (res != null)
                    {
                        return res.ValueAsInt;
                    }
                }
            }
            return -1;
        }

        private UriBuilder GetUploadToCrmUrl(int id, string crm_entity_type)
        {
            var upload_url = String.Format("{2}/crm/{0}/{1}/files/upload.xml", crm_entity_type, id, BaseUrl);
            return ApiHelper.GetApiRequestUrl(upload_url);
        }

        private UriBuilder GetAddRelationshipEventCrmUrl()
        {
            var add_url = string.Format("{0}/{1}", BaseUrl, "crm/history.json");
            return ApiHelper.GetApiRequestUrl(add_url);
        }

        private void AddRelationshipEventWithCrmApi(MailMessageItem item, CrmContactEntity entity, string content_string, List<int> files_id)
        {
            var body_builder = new StringBuilder();
            body_builder
                .AppendFormat("content={0}", HttpUtility.UrlEncode(content_string))
                .AppendFormat("&categoryId={0}", MailCrmHistoryCategory)
                .AppendFormat("&created={0}", HttpUtility.UrlEncode(new ApiDateTime(item.Date).ToString()));

            var crm_entity_type = entity.Type.StringName();
            if (crm_entity_type == ChainXCrmContactEntity.CrmEntityTypeNames.contact)
            {
                body_builder.AppendFormat("&contactId={0}&entityId=0", entity.Id);
            }
            else
            {
                if (crm_entity_type != ChainXCrmContactEntity.CrmEntityTypeNames.Case
                    && crm_entity_type != ChainXCrmContactEntity.CrmEntityTypeNames.opportunity)
                    throw new ArgumentException(String.Format("Invalid crm entity type: {0}", crm_entity_type));
                body_builder.AppendFormat("&contactId=0&entityId={0}&entityType={1}", entity.Id, crm_entity_type);
            }

            if (files_id != null)
            {
                foreach (var id in files_id)
                {
                    body_builder.AppendFormat("&fileId[]={0}", id);
                }
            }

            var request_uri_builder = GetAddRelationshipEventCrmUrl();
            var auth_cookie = ApiHelper.GetAuthCookie(UserId, request_uri_builder.Host);

            byte[] body_bytes = Encoding.UTF8.GetBytes(body_builder.ToString());
            var request_stream = new MemoryStream();
            request_stream.Write(body_bytes, 0, body_bytes.Length);
            request_stream.Position = 0;
            FormUpload.PostForm(request_uri_builder.ToString(), "", "application/x-www-form-urlencoded",
                                request_stream, auth_cookie);
        }

        private static string GetHistoryContentJson(MailMessageItem item)
        {
            string content_string;

            var content_struct = new CrmHistoryContent
                {
                    message_id = item.Id
                };

            var serializer = new DataContractJsonSerializer(typeof (CrmHistoryContent));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, content_struct);
                content_string = Encoding.UTF8.GetString(stream.GetCorrectBuffer());
            }
            return content_string;
        }
    }
}
