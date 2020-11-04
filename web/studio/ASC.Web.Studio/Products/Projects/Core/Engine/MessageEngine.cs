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
using System.Collections.Generic;
using System.Linq;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.ElasticSearch;
using ASC.Files.Core;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Projects.Core.Search;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    public class MessageEngine : ProjectEntityEngine
    {
        public IDaoFactory DaoFactory { get; set; }

        public MessageEngine(bool disableNotifications) : base(NotifyConstants.Event_NewCommentForMessage, disableNotifications)
        {
        }

        #region Get Discussion

        public override ProjectEntity GetEntityByID(int id)
        {
            return GetByID(id);
        }

        public Message GetByID(int id)
        {
            return GetByID(id, true);
        }

        public Message GetByID(int id, bool checkSecurity)
        {
            var message = DaoFactory.MessageDao.GetById(id);

            if (message != null)
                message.CommentsCount = DaoFactory.CommentDao.Count(new List<ProjectEntity> { message }).FirstOrDefault();

            if (!checkSecurity)
                return message;

            return CanRead(message) ? message : null;
        }

        public IEnumerable<Message> GetAll()
        {
            return DaoFactory.MessageDao.GetAll().Where(CanRead);
        }

        public IEnumerable<Message> GetByProject(int projectID)
        {
            var messages = DaoFactory.MessageDao.GetByProject(projectID)
                .Where(CanRead)
                .ToList();
            var commentsCount = DaoFactory.CommentDao.Count(messages.ConvertAll(r => (ProjectEntity)r));

            return messages.Select((message, index) =>
            {
                message.CommentsCount = commentsCount[index];
                return message;
            });
        }

        public IEnumerable<Message> GetMessages(int startIndex, int maxResult)
        {
            var messages = DaoFactory.MessageDao.GetMessages(startIndex, maxResult)
                .Where(CanRead)
                .ToList();
            var commentsCount = DaoFactory.CommentDao.Count(messages.Select(r => (ProjectEntity)r).ToList());

            return messages.Select((message, index) =>
            {
                message.CommentsCount = commentsCount[index];
                return message;
            });
        }

        public IEnumerable<Message> GetByFilter(TaskFilter filter)
        {
            var messages = DaoFactory.MessageDao.GetByFilter(filter, ProjectSecurity.CurrentUserAdministrator,
                ProjectSecurity.IsPrivateDisabled);

            var commentsCount = DaoFactory.CommentDao.Count(messages.Select(r => (ProjectEntity)r).ToList());

            return messages.Select((message, index) =>
            {
                message.CommentsCount = commentsCount[index];
                return message;
            });
        }

        public int GetByFilterCount(TaskFilter filter)
        {
            return DaoFactory.MessageDao.GetByFilterCount(filter, ProjectSecurity.CurrentUserAdministrator,
                ProjectSecurity.IsPrivateDisabled);
        }

        public List<Tuple<Guid, int, int>> GetByFilterCountForReport(TaskFilter filter)
        {
            return DaoFactory.MessageDao.GetByFilterCountForReport(filter, ProjectSecurity.CurrentUserAdministrator, ProjectSecurity.IsPrivateDisabled);
        }

        public bool IsExists(int id)
        {
            return DaoFactory.MessageDao.IsExists(id);
        }

        public bool CanRead(Message message)
        {
            return ProjectSecurity.CanRead(message);
        }

        #endregion

        #region Save, Delete, Attach

        public Message SaveOrUpdate(Message message, bool notify, IEnumerable<Guid> participant,
            IEnumerable<int> fileIds = null)
        {
            if (message == null) throw new ArgumentNullException("message");

            var isNew = message.ID == default(int);

            message.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            message.LastModifiedOn = TenantUtil.DateTimeNow();

            if (isNew)
            {
                if (message.CreateBy == default(Guid)) message.CreateBy = SecurityContext.CurrentAccount.ID;
                if (message.CreateOn == default(DateTime)) message.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandCreate<Message>(message.Project);
                DaoFactory.MessageDao.Save(message);
            }
            else
            {
                ProjectSecurity.DemandEdit(message);
                DaoFactory.MessageDao.Save(message);
            }

            if (fileIds != null)
            {
                foreach (var fileId in fileIds)
                {
                    AttachFile(message, fileId);
                }
            }

            if (participant == null)
                participant = GetSubscribers(message).Select(r => new Guid(r.ID)).ToList();

            NotifyParticipiant(message, isNew, participant, GetFiles(message), notify);

            FactoryIndexer<DiscussionsWrapper>.IndexAsync(message);

            return message;
        }

        public Message ChangeStatus(Message message)
        {
            message.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            message.LastModifiedOn = TenantUtil.DateTimeNow();

            ProjectSecurity.DemandEdit(message);
            DaoFactory.MessageDao.Save(message);

            return message;
        }

        public void Delete(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.Project == null) throw new Exception("Project");

            ProjectSecurity.DemandEdit(message);

            DaoFactory.MessageDao.Delete(message.ID);

            var recipients = GetSubscribers(message);

            if (recipients.Any() && !DisableNotifications)
            {
                NotifyClient.Instance.SendAboutMessageDeleting(recipients, message);
            }

            UnSubscribeAll(message);

            FactoryIndexer<DiscussionsWrapper>.DeleteAsync(message);
        }

        #endregion

        #region Notify

        protected void NotifyParticipiant(Message message, bool isMessageNew, IEnumerable<Guid> participant,
            IEnumerable<File> uploadedFiles, bool sendNotify)
        {
            //Don't send anything if notifications are disabled
            if (DisableNotifications) return;

            var subscriptionRecipients = GetSubscribers(message);

            var recipients = new HashSet<Guid>(participant);

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
                NotifyClient.Instance.SendAboutMessageAction(GetSubscribers(message), message, isMessageNew,
                    FileEngine.GetFileListInfoHashtable(uploadedFiles));
            }
        }

        #endregion
    }
}