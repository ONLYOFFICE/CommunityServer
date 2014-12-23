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
using System.Linq;
using System.Runtime.Serialization;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Dal.DbSchema;

namespace ASC.Mail.Aggregator.Dal
{
    [DataContract(Namespace = "")]
    public class SignatureDto
    {
        public SignatureDto(int id_mailbox, int tenant, string html, bool is_active)
        {
            Tenant = tenant;
            MailboxId = id_mailbox;
            Html = html;
            IsActive = is_active;
        }

        public int Tenant { get; private set;}

        [DataMember]
        public int MailboxId { get; private set; }

        [DataMember]
        public string Html { get; private set; }

        [DataMember]
        public bool IsActive { get; private set; }
    }

    internal class SignatureDal
    {
        private readonly DbManager _manager;

        public SignatureDal(DbManager manager)
        {
            _manager = manager;
        }

        public SignatureDto GetSignature(int id_mailbox, int tenant)
        {
            var select_query = GetSelectSignatureQuery(id_mailbox, tenant);
            var result_list = _manager.ExecuteList(select_query);
            
            if(result_list.Count == 0)
                return new SignatureDto(id_mailbox, tenant, "", false);

            var result = result_list.First();
            return new SignatureDto(id_mailbox, tenant, Convert.ToString(result[0]), Convert.ToBoolean(result[1]));
        }

        public List<SignatureDto> GetSignatures(List<int> mailboxes_ids, int tenant)
        {
            var select_query = GetSelectSignaturesQuery(mailboxes_ids, tenant);
            var result_list = _manager.ExecuteList(select_query)
                                      .ConvertAll(result =>
                                                  new SignatureDto(Convert.ToInt32(result[0]), tenant,
                                                                   Convert.ToString(result[1]),
                                                                   Convert.ToBoolean(result[2])));

            var signatures = new List<SignatureDto>();

            mailboxes_ids.ForEach(id_mailbox =>
                {
                    var signature = result_list.FirstOrDefault(s => s.MailboxId == id_mailbox);
                    signatures.Add(signature ?? new SignatureDto(id_mailbox, tenant, "", false));
                });

            return signatures;
        }

        public void UpdateOrCreateSignature(SignatureDto signature)
        {
            ISqlInstruction query_for_execution;
            if (IsSignatureExist(signature))
            {
                query_for_execution = new SqlUpdate(SignatureTable.name)
                                       .Set(SignatureTable.Columns.html, signature.Html)
                                       .Set(SignatureTable.Columns.is_active, signature.IsActive)
                                       .Where(SignatureTable.Columns.id_tenant, signature.Tenant)
                                       .Where(SignatureTable.Columns.id_mailbox, signature.MailboxId);
            }
            else
            {
                query_for_execution = new SqlInsert(SignatureTable.name)
                                       .InColumnValue(SignatureTable.Columns.html, signature.Html)
                                       .InColumnValue(SignatureTable.Columns.is_active, signature.IsActive)
                                       .InColumnValue(SignatureTable.Columns.id_tenant, signature.Tenant)
                                       .InColumnValue(SignatureTable.Columns.id_mailbox, signature.MailboxId);
            }

            _manager.ExecuteNonQuery(query_for_execution);
        }

        public void DeleteSignature(int id_mailbox, int tenant)
        {
            var delete_signature_query = new SqlDelete(SignatureTable.name)
                                            .Where(SignatureTable.Columns.id_mailbox, id_mailbox)
                                            .Where(SignatureTable.Columns.id_tenant, tenant);

            _manager.ExecuteNonQuery(delete_signature_query);
        }

        private SqlQuery GetSelectSignaturesQuery(List<int> mailboxes_ids, int tenant)
        {
            return new SqlQuery(SignatureTable.name)
                .Select(SignatureTable.Columns.id_mailbox)
                .Select(SignatureTable.Columns.html)
                .Select(SignatureTable.Columns.is_active)
                .Where(SignatureTable.Columns.id_tenant, tenant)
                .Where(Exp.In(SignatureTable.Columns.id_mailbox, mailboxes_ids));
        }

        private SqlQuery GetSelectSignatureQuery(int id_mailbox, int tenant)
        {
            return new SqlQuery(SignatureTable.name)
                .Select(SignatureTable.Columns.html)
                .Select(SignatureTable.Columns.is_active)
                .Where(SignatureTable.Columns.id_mailbox, id_mailbox)
                .Where(SignatureTable.Columns.id_tenant, tenant);
        }

        private bool IsSignatureExist(SignatureDto signature)
        {
            var signature_query = GetSelectSignatureQuery(signature.MailboxId, signature.Tenant);
            return _manager.ExecuteList(signature_query).Any();
        }
    }
}
