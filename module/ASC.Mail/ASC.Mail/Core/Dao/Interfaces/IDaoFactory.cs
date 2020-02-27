/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Data;

namespace ASC.Mail.Core.Dao.Interfaces
{
    public interface IDaoFactory : IDisposable
    {
        IDbManager DbManager { get; }

        IFolderDao CreateFolderDao(int tenant, string user);

        IImapFlagsDao CreateImapFlagsDao();

        IImapSpecialMailboxDao CreateImapSpecialMailboxDao();

        IMailboxSignatureDao CreateMailboxSignatureDao(int tenant, string user);

        IMailboxAutoreplyDao CreateMailboxAutoreplyDao(int tenant, string user);

        IMailboxAutoreplyHistoryDao CreateMailboxAutoreplyHistoryDao(int tenant, string user);

        IMailboxDao CreateMailboxDao();

        IDisplayImagesAddressDao CreateDisplayImagesAddressDao(int tenant, string user);

        IAlertDao CreateAlertDao(int tenant, string user = null);

        ITagDao CreateTagDao(int tenant, string user);

        ITagMailDao CreateTagMailDao(int tenant, string user);

        ITagAddressDao CreateTagAddressDao(int tenant, string user);

        IMailboxProviderDao CreateMailboxProviderDao();

        IMailboxDomainDao CreateMailboxDomainDao();

        IMailboxServerDao CreateMailboxServerDao();

        IAccountDao CreateAccountDao(int tenant, string user);

        IAttachmentDao CreateAttachmentDao(int tenant, string user);

        IContactDao CreateContactDao(int tenant, string user);

        IContactInfoDao CreateContactInfoDao(int tenant, string user);

        IContactCardDao CreateContactCardDao(int tenant, string user);

        ICrmContactDao CreateCrmContactDao(int tenant, string user);

        ICrmLinkDao CreateCrmLinkDao(int tenant, string user);

        IMailDao CreateMailDao(int tenant, string user);

        IMailInfoDao CreateMailInfoDao(int tenant, string user);

        IChainDao CreateChainDao(int tenant, string user);

        IServerDao CreateServerDao();

        IServerDnsDao CreateServerDnsDao(int tenant, string user);

        IServerAddressDao CreateServerAddressDao(int tenant);

        IServerGroupDao CreateServerGroupDao(int tenant);

        IServerDomainDao CreateServerDomainDao(int tenant);

        IUserFolderDao CreateUserFolderDao(int tenant, string user);

        IUserFolderTreeDao CreateUserFolderTreeDao(int tenant, string user);

        IUserFolderXMailDao CreateUserFolderXMailDao(int tenant, string user);
    }
}
