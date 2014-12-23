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
using ASC.Common.Security.Authentication;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel.Base;

namespace ASC.Mail.Server.Administration.ServerModel
{
    public class MailAccountModel : MailAccountBase, IMailAccount
    {

        public IAccount TeamlabAccount { get; private set; }

        public MailAccountModel(IAccount teamlab_account, string login)
            : base(login)
        {
            if (teamlab_account == null)
                throw new ArgumentException("Invalid account", "teamlab_account");

            TeamlabAccount = teamlab_account;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IMailAccount))
            {
                return false;
            }

            var other = (IMailAccount)obj;

            return TeamlabAccount.Equals(other.TeamlabAccount) && Login.Equals(other.Login);
        }

        public override int GetHashCode()
        {
            return TeamlabAccount.GetHashCode() ^ Login.GetHashCode();
        }
    }
}
