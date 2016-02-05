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
using System.Collections;
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
        public SignatureDto(int mailboxId, int tenant, string html, bool isActive)
        {
            Tenant = tenant;
            MailboxId = mailboxId;
            Html = html;
            IsActive = isActive;
        }

        public int Tenant { get; private set;}

        [DataMember(Name = "mailboxId")]
        public int MailboxId { get; private set; }

        [DataMember(Name = "html")]
        public string Html { get; private set; }

        [DataMember(Name = "isActive")]
        public bool IsActive { get; private set; }
    }

    internal class SignatureDal
    {
        private readonly DbManager _manager;

        public SignatureDal(DbManager manager)
        {
            _manager = manager;
        }

        public SignatureDto GetSignature(int mailboxId, int tenant)
        {
            var signature = GetSignatures(new List<int> {mailboxId}, tenant).FirstOrDefault();

            return signature ?? new SignatureDto(mailboxId, tenant, "", false);
        }

        public List<SignatureDto> GetSignatures(List<int> mailboxesIds, int tenant)
        {
            if(!mailboxesIds.Any())
                return new List<SignatureDto>();

            var selectQuery = GetSelectSignaturesQuery(mailboxesIds, tenant);
            var resultList = _manager.ExecuteList(selectQuery)
                                      .ConvertAll(result =>
                                                  new SignatureDto(Convert.ToInt32(result[0]), tenant,
                                                                   Convert.ToString(result[1]),
                                                                   Convert.ToBoolean(result[2])));

            var signatures = new List<SignatureDto>();

            mailboxesIds.ForEach(idMailbox =>
                {
                    var signature = resultList.FirstOrDefault(s => s.MailboxId == idMailbox);
                    signatures.Add(signature ?? new SignatureDto(idMailbox, tenant, "", false));
                });

            return signatures;
        }

        public void UpdateOrCreateSignature(SignatureDto signature)
        {
            ISqlInstruction queryForExecution;
            if (IsSignatureExist(signature))
            {
                queryForExecution = new SqlUpdate(SignatureTable.name)
                                       .Set(SignatureTable.Columns.html, signature.Html)
                                       .Set(SignatureTable.Columns.is_active, signature.IsActive)
                                       .Where(SignatureTable.Columns.id_tenant, signature.Tenant)
                                       .Where(SignatureTable.Columns.id_mailbox, signature.MailboxId);
            }
            else
            {
                queryForExecution = new SqlInsert(SignatureTable.name)
                                       .InColumnValue(SignatureTable.Columns.html, signature.Html)
                                       .InColumnValue(SignatureTable.Columns.is_active, signature.IsActive)
                                       .InColumnValue(SignatureTable.Columns.id_tenant, signature.Tenant)
                                       .InColumnValue(SignatureTable.Columns.id_mailbox, signature.MailboxId);
            }

            _manager.ExecuteNonQuery(queryForExecution);
        }

        public void DeleteSignature(int mailboxId, int tenant)
        {
            var deleteSignatureQuery = new SqlDelete(SignatureTable.name)
                                            .Where(SignatureTable.Columns.id_mailbox, mailboxId)
                                            .Where(SignatureTable.Columns.id_tenant, tenant);

            _manager.ExecuteNonQuery(deleteSignatureQuery);
        }

        private static SqlQuery GetSelectSignaturesQuery(ICollection mailboxesIds, int tenant)
        {
            return new SqlQuery(SignatureTable.name)
                .Select(SignatureTable.Columns.id_mailbox)
                .Select(SignatureTable.Columns.html)
                .Select(SignatureTable.Columns.is_active)
                .Where(SignatureTable.Columns.id_tenant, tenant)
                .Where(Exp.In(SignatureTable.Columns.id_mailbox, mailboxesIds));
        }

        private bool IsSignatureExist(SignatureDto signature)
        {
            var signatureQuery = GetSelectSignaturesQuery(new List<int> { signature.MailboxId }, signature.Tenant);
            return _manager.ExecuteList(signatureQuery).Any();
        }
    }
}
