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


using System.Collections.Generic;
using ASC.Mail.Core.Entities;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IMailboxAutoreplyHistoryDao
    {
        /// <summary>
        ///     Get autoreplyHistory emails by mailbox id.
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <param name="email"></param>
        /// <param name="autoreplyDaysInterval"></param>
        /// <returns>list of emails</returns>
        List<string> GetAutoreplyHistorySentEmails(int mailboxId, string email, int autoreplyDaysInterval);

        /// <summary>
        ///     Save or update autoreplyHistory
        /// </summary>
        /// <param name="autoreplyHistory"></param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int SaveAutoreplyHistory(MailboxAutoreplyHistory autoreplyHistory);

        /// <summary>
        ///     Delete autoreplyHistory
        /// </summary>
        /// <param name="mailboxId">id</param>
        /// <returns>count of affected rows: 1 - OK, 0 - Fail</returns>
        int DeleteAutoreplyHistory(int mailboxId);
    }
}
