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


using ASC.Common.Data;
using ASC.Mail.Core.Dao;
using ASC.Mail.Core.Dao.Interfaces;

namespace ASC.Mail.Core
{
    public class DaoFactory : IDaoFactory
    {
        public IDbManager DbManager { get; private set; }

        public DaoFactory()
        {
            DbManager = new DbManager(Defines.CONNECTION_STRING_NAME);
        }

        public DaoFactory(IDbManager dbManager)
        {
            DbManager = dbManager;
        }

        public IFolderDao CreateFolderDao(int tenant, string user)
        {
            return new FolderDao(DbManager, tenant, user);
        }

        public IImapFlagsDao CreateImapFlagsDao()
        {
            return new ImapFlagsDao(DbManager);
        }

        public IImapSpecialMailboxDao CreateImapSpecialMailboxDao()
        {
            return new ImapSpecialMailboxDao(DbManager);
        }

        public IMailboxSignatureDao CreateMailboxSignatureDao(int tenant, string user)
        {
            return new MailboxSignatureDao(DbManager, tenant, user);
        }

        public IMailboxAutoreplyDao CreateMailboxAutoreplyDao(int tenant, string user)
        {
            return new MailboxAutoreplyDao(DbManager, tenant, user);
        }

        public IMailboxAutoreplyHistoryDao CreateMailboxAutoreplyHistoryDao(int tenant, string user)
        {
            return new MailboxAutoreplyHistoryDao(DbManager, tenant, user);
        }

        public IMailboxDao CreateMailboxDao()
        {
            return new MailboxDao(DbManager);
        }

        public IDisplayImagesAddressDao CreateDisplayImagesAddressDao(int tenant, string user)
        {
            return new DisplayImagesAddressDao(DbManager, tenant, user);
        }

        public IAlertDao CreateAlertDao(int tenant, string user = null)
        {
            return new AlertDao(DbManager, tenant, user);
        }

        public ITagDao CreateTagDao(int tenant, string user)
        {
            return new TagDao(DbManager, tenant, user);
        }

        public ITagMailDao CreateTagMailDao(int tenant, string user)
        {
            return new TagMailDao(DbManager, tenant, user);
        }

        public ITagAddressDao CreateTagAddressDao(int tenant, string user)
        {
            return new TagAddressDao(DbManager, tenant, user);
        }

        public IMailboxProviderDao CreateMailboxProviderDao()
        {
            return new MailboxProviderDao(DbManager);
        }

        public IMailboxDomainDao CreateMailboxDomainDao()
        {
            return new MailboxDomainDao(DbManager);
        }

        public IMailboxServerDao CreateMailboxServerDao()
        {
            return new MailboxServerDao(DbManager);
        }

        public IAccountDao CreateAccountDao(int tenant, string user)
        {
            return new AccountDao(DbManager, tenant, user);
        }

        public IAttachmentDao CreateAttachmentDao(int tenant, string user)
        {
            return new AttachmentDao(DbManager, tenant, user);
        }

        public IContactDao CreateContactDao(int tenant, string user)
        {
            return new ContactDao(DbManager, tenant, user);
        }

        public IContactInfoDao CreateContactInfoDao(int tenant, string user)
        {
            return new ContactInfoDao(DbManager, tenant, user);
        }

        public IContactCardDao CreateContactCardDao(int tenant, string user)
        {
            return new ContactCardDao(DbManager, tenant, user);
        }

        public ICrmContactDao CreateCrmContactDao(int tenant, string user)
        {
            return new CrmContactDao(DbManager, tenant, user);
        }

        public ICrmLinkDao CreateCrmLinkDao(int tenant, string user)
        {
            return new CrmLinkDao(DbManager, tenant, user);
        }

        public IMailDao CreateMailDao(int tenant, string user)
        {
            return new MailDao(DbManager, tenant, user);
        }

        public IMailInfoDao CreateMailInfoDao(int tenant, string user)
        {
            return new MailInfoDao(DbManager, tenant, user);
        }

        public IChainDao CreateChainDao(int tenant, string user)
        {
            return new ChainDao(DbManager, tenant, user);
        }

        public IServerDao CreateServerDao()
        {
            return new ServerDao(DbManager);
        }

        public IServerDnsDao CreateServerDnsDao(int tenant, string user)
        {
            return new ServerDnsDao(DbManager, tenant, user);
        }

        public IServerAddressDao CreateServerAddressDao(int tenant)
        {
            return new ServerAddressDao(DbManager, tenant);
        }

        public IServerGroupDao CreateServerGroupDao(int tenant)
        {
            return new ServerGroupDao(DbManager, tenant);
        }

        public IServerDomainDao CreateServerDomainDao(int tenant)
        {
            return new ServerDomainDao(DbManager, tenant);
        }

        public IUserFolderDao CreateUserFolderDao(int tenant, string user)
        {
            return new UserFolderDao(DbManager, tenant, user);
        }

        public IUserFolderTreeDao CreateUserFolderTreeDao(int tenant, string user)
        {
            return new UserFolderTreeDao(DbManager, tenant, user);
        }

        public IUserFolderXMailDao CreateUserFolderXMailDao(int tenant, string user)
        {
            return new UserFolderXMailDao(DbManager, tenant, user);
        }

        public IFilterDao CreateFilterDao(int tenant, string user)
        {
            return new FilterDao(DbManager, tenant, user);
        }

        public void Dispose()
        {
            DbManager.Dispose();
        }
    }
}
