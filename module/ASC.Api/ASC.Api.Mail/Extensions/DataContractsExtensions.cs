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

using System.Collections.Generic;
using ASC.Api.Mail.DataContracts;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Dal;

namespace ASC.Api.Mail.Extensions
{
    public static class DataContractsExtensions
    {
        public static List<MailAccountData> ToAddressData(this List<AccountInfo> accounts)
        {
            var from_email_list = new List<MailAccountData>();

            foreach (var account in accounts)
            {
                var email_data = new MailAccountData
                    {
                        MailboxId = account.Id,
                        Email = account.Email,
                        Name = account.Name,
                        Enabled = account.Enabled,
                        OAuthConnection = account.OAuthConnection,
                        AuthError = account.AuthError,
                        QuotaError = account.QuotaError,
                        Signature = account.Signature,
                        EMailInFolder = account.EMailInFolder,
                        IsAlias = false,
                        IsGroup = false,
                        IsTeamlabMailbox = account.IsTeamlabMailbox
                    };
                from_email_list.Add(email_data);

                foreach (var alias in account.Aliases)
                {
                    email_data = new MailAccountData
                        {
                            MailboxId = account.Id,
                            Email = alias.Email,
                            Name = account.Name,
                            Enabled = account.Enabled,
                            OAuthConnection = false,
                            AuthError = false,
                            QuotaError = false,
                            Signature = account.Signature,
                            EMailInFolder = "",
                            IsAlias = true,
                            IsGroup = false,
                            IsTeamlabMailbox = false
                        };
                    from_email_list.Add(email_data);
                }

                foreach (var group in account.Groups)
                {
                    if (from_email_list.FindIndex(e => e.Email.Equals(group.Email)) != -1) continue;
                    email_data = new MailAccountData
                        {
                            MailboxId = account.Id,
                            Email = group.Email,
                            Name = "",
                            Enabled = true,
                            OAuthConnection = false,
                            AuthError = false,
                            QuotaError = false,
                            Signature = new SignatureDto(-1, account.Signature.Tenant, "", false),
                            EMailInFolder = "",
                            IsAlias = false,
                            IsGroup = true,
                            IsTeamlabMailbox = false
                        };
                    from_email_list.Add(email_data);
                }
            }

            return from_email_list;
        }
    }
}
