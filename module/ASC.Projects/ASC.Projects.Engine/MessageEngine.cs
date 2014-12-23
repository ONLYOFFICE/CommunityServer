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
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    public class MessageEngine : ProjectEntityEngine
    {
        private readonly EngineFactory engineFactory;
        private readonly IMessageDao messageDao;
        private readonly ICommentDao commentDao;

        public MessageEngine(IDaoFactory daoFactory, EngineFactory engineFactory)
            : base(NotifyConstants.Event_NewCommentForMessage, engineFactory)
        {
            this.engineFactory = engineFactory;
            messageDao = daoFactory.GetMessageDao();
            commentDao = daoFactory.GetCommentDao();
        }

        #region Get Discussion

        public Message GetByID(int id)
        {
            return GetByID(id, true);
        }

        public Message GetByID(int id, bool checkSecurity)
        {
            var message = messageDao.GetById(id);

            if (message != null)
                message.CommentsCount = commentDao.GetCommentsCount(new List<ProjectEntity> {message}).FirstOrDefault();

            if (!checkSecurity)
                return message;

            return CanRead(message) ? message : null;
        }

        public IEnumerable<Message> GetAll()
        {
            return messageDao.GetAll().Where(CanRead);
        }

        public List<Message> GetByProject(int projectID)
        {
            var messages = messageDao.GetByProject(projectID)
                .Where(CanRead)
                .ToList();
            var commentsCount = commentDao.GetCommentsCount(messages.ConvertAll(r => (ProjectEntity)r));

            return messages.Select((message, index) =>
            { 
                                                    message.CommentsCount = commentsCount[index];
                                                    return message;
            }).ToList();

        }

        public List<Message> GetMessages(int startIndex,int maxResult)
        {
            var messages = messageDao.GetMessages(startIndex, maxResult)
                .Where(CanRead)
                .ToList();
            var commentsCount = commentDao.GetCommentsCount(messages.Select(r => (ProjectEntity)r).ToList());

            return messages.Select((message, index) =>
            {
                message.CommentsCount = commentsCount[index];
                return message;
            }).ToList();
            
        }

        public List<Message> GetByFilter(TaskFilter filter)
        {
            var messages = messageDao.GetByFilter(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);

            var commentsCount = commentDao.GetCommentsCount(messages.Select(r => (ProjectEntity)r).ToList());

            return messages.Select((message, index) =>
            {
                message.CommentsCount = commentsCount[index];
                return message;
            }).ToList();
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return messageDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public bool IsExists(int id)
        {
            return messageDao.IsExists(id);
        }

        public bool CanRead(Message message)
        {
            return ProjectSecurity.CanRead(message);
        }

        #endregion

        #region Save, Delete, Attach

        public Message SaveOrUpdate(Message message, bool notify, IEnumerable<Guid> participant, IEnumerable<int> fileIds)
        {
            if (message == null) throw new ArgumentNullException("message");

            var isNew = message.ID == default(int);

            message.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            message.LastModifiedOn = TenantUtil.DateTimeNow();

            if (isNew)
            {
                if (message.CreateBy == default(Guid)) message.CreateBy = SecurityContext.CurrentAccount.ID;
                if (message.CreateOn == default(DateTime)) message.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreateMessage(message.Project);
                messageDao.Save(message);
            }
            else
            {
                ProjectSecurity.DemandEdit(message);
                messageDao.Save(message);
            }

            if (fileIds != null)
            {
                foreach (var fileId in fileIds)
                {
                    AttachFile(message, fileId, false);
                }
            }

            if (!participant.Any())
                participant = GetSubscribers(message).Select(r=> new Guid(r.ID));

            NotifyParticipiant(message, isNew, participant, GetFiles(message), notify);

            return message;
        }

        public Message ChangeStatus(Message message)
        {
            message.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            message.LastModifiedOn = TenantUtil.DateTimeNow();

            ProjectSecurity.DemandEdit(message);
            messageDao.Save(message);

            return message;
        }

        public void Delete(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Project == null) throw new Exception("Project");

            ProjectSecurity.DemandEdit(message);

            messageDao.Delete(message.ID);

            var recipients = GetSubscribers(message);

            if (recipients.Any() && !engineFactory.DisableNotifications)
            {
                NotifyClient.Instance.SendAboutMessageDeleting(recipients, message);
            }

            UnSubscribe(message);
        }


        #endregion

        #region Notify

        protected void NotifyParticipiant(Message message, bool isMessageNew, IEnumerable<Guid> participant, IEnumerable<File> uploadedFiles, bool sendNotify)
        {
            //Don't send anything if notifications are disabled
            if (engineFactory.DisableNotifications) return;

            var subscriptionRecipients = GetSubscribers(message);
            var recipients = new HashSet<Guid>(participant) { SecurityContext.CurrentAccount.ID };

            foreach (var subscriptionRecipient in subscriptionRecipients)
            {
                var subscriptionRecipientId = new Guid(subscriptionRecipient.ID);
                if (!recipients.Contains(subscriptionRecipientId))
                {
                    UnSubscribe(message, subscriptionRecipientId);
                }
            }

            foreach (var subscriber in recipients)
            {
                Subscribe(message, subscriber);
            }

            if (sendNotify && recipients.Any())
            {
                NotifyClient.Instance.SendAboutMessageAction(GetSubscribers(message), message, isMessageNew, FileEngine.GetFileListInfoHashtable(uploadedFiles));
            }
        }

        #endregion
    }
}
