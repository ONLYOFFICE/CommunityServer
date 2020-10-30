/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
