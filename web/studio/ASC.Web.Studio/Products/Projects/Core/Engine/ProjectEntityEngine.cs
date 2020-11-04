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
using ASC.Files.Core;
using ASC.Notify.Model;
using ASC.Notify.Recipients;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using ASC.Web.Files.Api;

namespace ASC.Projects.Engine
{
    public class ProjectEntityEngine
    {
        public ISubscriptionProvider SubscriptionProvider { get; set; }
        public IRecipientProvider RecipientProvider { get; set; }
        public INotifyAction NotifyAction { get; set; }
        public FileEngine FileEngine { get; set; }
        public ProjectSecurity ProjectSecurity { get; set; }

        public bool DisableNotifications { get; set; }

        public ProjectEntityEngine(INotifyAction notifyAction, bool disableNotifications)
        {
            SubscriptionProvider = NotifySource.Instance.GetSubscriptionProvider();
            RecipientProvider = NotifySource.Instance.GetRecipientsProvider();
            NotifyAction = notifyAction;
            DisableNotifications = disableNotifications;
        }

        public virtual ProjectEntity GetEntityByID(int id)
        {
            return null;
        }

        #region Subscription

        public void Subscribe(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            if (recipient == null) return;

            if (!IsUnsubscribed(entity, recipientID) || entity.CanEdit())
                SubscriptionProvider.Subscribe(NotifyAction, entity.NotifyId, recipient);
        }

        public void UnSubscribe(ProjectEntity entity)
        {
            UnSubscribe(entity, SecurityContext.CurrentAccount.ID);
        }

        public void UnSubscribe(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            if (recipient == null) return;

            SubscriptionProvider.UnSubscribe(NotifyAction, entity.NotifyId, recipient);
        }

        public void UnSubscribeAll<T>(T entity) where T: ProjectEntity
        {
            SubscriptionProvider.UnSubscribe(NotifyAction, entity.NotifyId);
        }

        public void UnSubscribeAll<T>(List<T> entity) where T: ProjectEntity
        {
            entity.ForEach(UnSubscribeAll);
        }

        public bool IsSubscribed(ProjectEntity entity)
        {
            return IsSubscribed(entity, SecurityContext.CurrentAccount.ID);
        }

        public bool IsSubscribed(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            var objects = SubscriptionProvider.GetSubscriptions(NotifyAction, recipient);

            return objects.Any(item => string.Compare(item, entity.NotifyId, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public bool IsUnsubscribed(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            return recipient != null && SubscriptionProvider.IsUnsubscribe((IDirectRecipient)recipient, NotifyAction, entity.NotifyId);
        }

        public void Follow(ProjectEntity entity)
        {
            Follow(entity, SecurityContext.CurrentAccount.ID);
        }

        public void Follow(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            if (recipient == null) return;

            if (!IsSubscribed(entity, recipientID))
                SubscriptionProvider.Subscribe(NotifyAction, entity.NotifyId, recipient);
            else
                SubscriptionProvider.UnSubscribe(NotifyAction, entity.NotifyId, recipient);
        }

        public List<IRecipient> GetSubscribers(ProjectEntity entity)
        {
            return SubscriptionProvider.GetRecipients(NotifyAction, entity.NotifyId).ToList();
        }

        #endregion

        #region Files

        public IEnumerable<File> GetFiles(ProjectEntity entity)
        {
            if (entity == null) return new List<File>();

            if (!ProjectSecurity.CanReadFiles(entity.Project)) return new List<File>();

            using (var tagdao = FilesIntegration.GetTagDao())
            using (var filedao = FilesIntegration.GetFileDao())
            {
                var ids = tagdao.GetTags(entity.GetType().Name + entity.ID, TagType.System).Where(t => t.EntryType == FileEntryType.File).Select(t => t.EntryId).ToArray();
                var files = 0 < ids.Length ? filedao.GetFiles(ids) : new List<File>();

                var rootId = FileEngine.GetRoot(entity.Project.ID);

                //delete tags when file moved from project folder
                files.Where(file => !file.RootFolderId.Equals(rootId)).ToList()
                    .ForEach(file =>
                    {
                        DetachFile(entity, file.ID);
                        files.Remove(file);
                    });

                files.ForEach(r => r.Access = FileEngine.GetFileShare(r, entity.Project.ID));
                return files;
            }
        }

        public void AttachFile(ProjectEntity entity, object fileId, bool notify = false)
        {
            if (!ProjectSecurity.CanReadFiles(entity.Project)) return;

            File file;

            using (var dao = FilesIntegration.GetTagDao())
            {
                dao.SaveTags(new Tag(entity.GetType().Name + entity.ID, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileId });
                file = FileEngine.GetFile(fileId, 0);
            }

            if (notify && !DisableNotifications)
            {
                var senders = GetSubscribers(entity);
                NotifyClient.Instance.SendNewFile(senders, entity, file.Title);
            }
        }

        public void DetachFile(ProjectEntity entity, object fileId)
        {
            if (!ProjectSecurity.CanReadFiles(entity.Project)) return;

            using (var dao = FilesIntegration.GetTagDao())
            {
                dao.RemoveTags(new Tag(entity.GetType().Name + entity.ID, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileId });
            }
        }

        #endregion
    }
}