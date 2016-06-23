/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Mail.Aggregator.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ASC.Mail.Aggregator.DbSchema;

namespace ASC.Mail.Aggregator.Dal
{
    internal class SignatureDal
    {
        private readonly DbManager _manager;

        public SignatureDal(DbManager manager)
        {
            _manager = manager;
        }

        public MailSignature GetSignature(int mailboxId, int tenant)
        {
            var signature = GetSignatures(new List<int> { mailboxId }, tenant).FirstOrDefault();

            return signature ?? new MailSignature(mailboxId, tenant, "", false);
        }

        public List<MailSignature> GetSignatures(List<int> mailboxesIds, int tenant)
        {
            if (!mailboxesIds.Any())
                return new List<MailSignature>();

            var selectQuery = GetSelectSignaturesQuery(mailboxesIds, tenant);
            var resultList = _manager.ExecuteList(selectQuery)
                                      .ConvertAll(result =>
                                                  new MailSignature(Convert.ToInt32(result[0]), tenant,
                                                                   Convert.ToString(result[1]),
                                                                   Convert.ToBoolean(result[2])));

            var signatures = new List<MailSignature>();

            mailboxesIds.ForEach(idMailbox =>
                {
                    var signature = resultList.FirstOrDefault(s => s.MailboxId == idMailbox);
                    signatures.Add(signature ?? new MailSignature(idMailbox, tenant, "", false));
                });

            return signatures;
        }

        public void UpdateOrCreateSignature(MailSignature signature)
        {
            ISqlInstruction queryForExecution = new SqlInsert(SignatureTable.Name, true)
                                       .InColumnValue(SignatureTable.Columns.Html, signature.Html)
                                       .InColumnValue(SignatureTable.Columns.IsActive, signature.IsActive)
                                       .InColumnValue(SignatureTable.Columns.Tenant, signature.Tenant)
                                       .InColumnValue(SignatureTable.Columns.MailboxId, signature.MailboxId);

            _manager.ExecuteNonQuery(queryForExecution);
        }

        public void DeleteSignature(int mailboxId, int tenant)
        {
            var deleteSignatureQuery = new SqlDelete(SignatureTable.Name)
                                            .Where(SignatureTable.Columns.MailboxId, mailboxId)
                                            .Where(SignatureTable.Columns.Tenant, tenant);

            _manager.ExecuteNonQuery(deleteSignatureQuery);
        }

        private static SqlQuery GetSelectSignaturesQuery(ICollection mailboxesIds, int tenant)
        {
            return new SqlQuery(SignatureTable.Name)
                .Select(SignatureTable.Columns.MailboxId)
                .Select(SignatureTable.Columns.Html)
                .Select(SignatureTable.Columns.IsActive)
                .Where(SignatureTable.Columns.Tenant, tenant)
                .Where(Exp.In(SignatureTable.Columns.MailboxId, mailboxesIds));
        }
    }
}
