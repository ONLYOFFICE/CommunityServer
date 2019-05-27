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


using ASC.Common.Logging;
using ASC.Mail.Core.Engine;

namespace ASC.Mail.Core
{
    public class EngineFactory
    {
        public string UserId { get; private set; }
        public int Tenant { get; private set; }

        public ILog Log { get; private set; }

        public EngineFactory(int tenant, string userId = null, ILog log = null)
        {
            UserId = userId;
            Tenant = tenant;

            Log = log;
        }

        private AutoreplyEngine _autoreplyEngine;
        public AutoreplyEngine AutoreplyEngine
        {
            get { return _autoreplyEngine ?? (_autoreplyEngine = new AutoreplyEngine(Tenant, UserId, Log)); }
        }

        private EmailInEngine _emailInEngine;
        public EmailInEngine EmailInEngine {
            get { return _emailInEngine ?? (_emailInEngine = new EmailInEngine(Log)); }
        }

        private CalendarEngine _calendarEngine;
        public CalendarEngine CalendarEngine
        {
            get { return _calendarEngine ?? (_calendarEngine = new CalendarEngine(Log)); }
        }

        private AlertEngine _alertEngine;
        public AlertEngine AlertEngine
        {
            get { return _alertEngine ?? (_alertEngine = new AlertEngine(Tenant, UserId, Log)); }
        }

        private TagEngine _tagEngine;
        public TagEngine TagEngine
        {
            get { return _tagEngine ?? (_tagEngine = new TagEngine(Tenant, UserId, Log)); }
        }

        private FolderEngine _folderEngine;
        public FolderEngine FolderEngine
        {
            get { return _folderEngine ?? (_folderEngine = new FolderEngine(Tenant, UserId, Log)); }
        }

        private OperationEngine _operationEngine;
        public OperationEngine OperationEngine
        {
            get { return _operationEngine ?? (_operationEngine = new OperationEngine()); }
        }

        private MailBoxSettingEngine _mailBoxSettingEngine;
        public MailBoxSettingEngine MailBoxSettingEngine
        {
            get { return _mailBoxSettingEngine ?? (_mailBoxSettingEngine = new MailBoxSettingEngine(Log)); }
        }

        private MailboxEngine _mailboxEngine;
        public MailboxEngine MailboxEngine
        {
            get { return _mailboxEngine ?? (_mailboxEngine = new MailboxEngine(Tenant, UserId, Log)); }
        }

        private AccountEngine _accountEngine;
        public AccountEngine AccountEngine
        {
            get { return _accountEngine ?? (_accountEngine = new AccountEngine(Tenant, UserId, Log)); }
        }

        private QuotaEngine _quotaEngine;
        public QuotaEngine QuotaEngine
        {
            get { return _quotaEngine ?? (_quotaEngine = new QuotaEngine(Tenant, Log)); }
        }

        private AttachmentEngine _attachmentEngine;
        public AttachmentEngine AttachmentEngine
        {
            get { return _attachmentEngine ?? (_attachmentEngine = new AttachmentEngine(Tenant, UserId, Log)); }
        }

        private ContactEngine _contactEngine;
        public ContactEngine ContactEngine
        {
            get { return _contactEngine ?? (_contactEngine = new ContactEngine(Tenant, UserId, Log)); }
        }

        private CrmLinkEngine _crmLinkEngine;
        public CrmLinkEngine CrmLinkEngine
        {
            get { return _crmLinkEngine ?? (_crmLinkEngine = new CrmLinkEngine(Tenant, UserId, Log)); }
        }

        private MessageEngine _mailEngine;
        public MessageEngine MessageEngine
        {
            get { return _mailEngine ?? (_mailEngine = new MessageEngine(Tenant, UserId, Log)); }
        }

        private DisplayImagesAddressEngine _displayImagesAddressEngine;
        public DisplayImagesAddressEngine DisplayImagesAddressEngine
        {
            get { return _displayImagesAddressEngine ?? (_displayImagesAddressEngine = new DisplayImagesAddressEngine(Tenant, UserId)); }
        }

        private SignatureEngine _signatureEngine;
        public SignatureEngine SignatureEngine
        {
            get { return _signatureEngine ?? (_signatureEngine = new SignatureEngine(Tenant, UserId, Log)); }
        }

        private DraftEngine _draftEngine;
        public DraftEngine DraftEngine
        {
            get { return _draftEngine ?? (_draftEngine = new DraftEngine(Tenant, UserId, log: Log)); }
        }

        private ChainEngine _chainEngine;
        public ChainEngine ChainEngine
        {
            get { return _chainEngine ?? (_chainEngine = new ChainEngine(Tenant, UserId, Log)); }
        }

        private SpamEngine _spamEngine;
        public SpamEngine SpamEngine
        {
            get { return _spamEngine ?? (_spamEngine = new SpamEngine(Tenant, UserId, Log)); }
        }

        private ServerEngine _serverEngine;
        public ServerEngine ServerEngine
        {
            get { return _serverEngine ?? (_serverEngine = new ServerEngine(Tenant, UserId, Log)); }
        }

        private ServerDomainEngine _serverDomainEngine;
        public ServerDomainEngine ServerDomainEngine
        {
            get { return _serverDomainEngine ?? (_serverDomainEngine = new ServerDomainEngine(Tenant, UserId, Log)); }
        }

        private ServerMailboxEngine _serverMailboxEngine;
        public ServerMailboxEngine ServerMailboxEngine
        {
            get { return _serverMailboxEngine ?? (_serverMailboxEngine = new ServerMailboxEngine(Tenant, UserId, Log)); }
        }

        private ServerMailgroupEngine _serverMailgroupEngine;
        public ServerMailgroupEngine ServerMailgroupEngine
        {
            get { return _serverMailgroupEngine ?? (_serverMailgroupEngine = new ServerMailgroupEngine(Tenant, UserId, Log)); }
        }

        private MailGarbageEngine _mailGarbageEngine;
        public MailGarbageEngine MailGarbageEngine
        {
            get { return _mailGarbageEngine ?? (_mailGarbageEngine = new MailGarbageEngine(Log)); }
        }

        private UserFolderEngine _userFolderEngine;
        public UserFolderEngine UserFolderEngine
        {
            get { return _userFolderEngine ?? (_userFolderEngine = new UserFolderEngine(Tenant, UserId, Log)); }
        }

        private TestEngine _testEngine;
        public TestEngine TestEngine
        {
            get { return _testEngine ?? (_testEngine = new TestEngine(Tenant, UserId, Log)); }
        }

        private FilterEngine _filterEngine;
        public FilterEngine FilterEngine
        {
            get { return _filterEngine ?? (_filterEngine = new FilterEngine(Tenant, UserId, Log)); }
        }

        private IndexEngine _indexEngine;
        public IndexEngine IndexEngine
        {
            get { return _indexEngine ?? (_indexEngine = new IndexEngine(Tenant, UserId, Log)); }
        }
    }
}
