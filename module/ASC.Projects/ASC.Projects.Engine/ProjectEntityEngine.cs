/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
        ISubscriptionProvider SubscriptionProvider { get; set; }
        IRecipientProvider RecipientProvider { get; set; }
        INotifyAction NotifyAction { get; set; }
        FileEngine FileEngine { get; set; }
        EngineFactory Factory { get; set; }

        public ProjectEntityEngine(INotifyAction notifyAction, EngineFactory factory)
        {
            SubscriptionProvider = NotifySource.Instance.GetSubscriptionProvider();
            RecipientProvider = NotifySource.Instance.GetRecipientsProvider();
            NotifyAction = notifyAction;
            FileEngine = factory != null ? factory.GetFileEngine() : null;
            Factory = factory;
        }

        #region Subscription

        public void Subscribe(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            if (recipient == null) return;

            if (!IsUnsubscribed(entity, recipientID))
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

        public bool IsSubscribed(ProjectEntity entity)
        {
            return IsSubscribed(entity, SecurityContext.CurrentAccount.ID);
        }

        public bool IsSubscribed(ProjectEntity entity, Guid recipientID)
        {
            var recipient = RecipientProvider.GetRecipient(recipientID.ToString());

            var objects = new List<String>(SubscriptionProvider.GetSubscriptions(NotifyAction, recipient));

            return !String.IsNullOrEmpty(objects.Find(item => String.Compare(item, entity.NotifyId, StringComparison.OrdinalIgnoreCase) == 0));
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
                FileEngine.SetThumbUrls(files);
                return files;
            }
        }

        public void AttachFile(ProjectEntity entity, object fileId, bool notify)
        {
            if (!ProjectSecurity.CanReadFiles(entity.Project)) return;

            File file;

            using (var dao = FilesIntegration.GetTagDao())
            {
                dao.SaveTags(new Tag(entity.GetType().Name + entity.ID, TagType.System, Guid.Empty) { EntryType = FileEntryType.File, EntryId = fileId });
                file = FileEngine.GetFile(fileId, 0);
                FileEngine.GenerateImageThumb(file);
            }

            if (notify && !Factory.DisableNotifications)
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

        #region Comments

        public Comment SaveOrUpdateComment(ProjectEntity entity, Comment comment)
        {
            var isNew = comment.ID.Equals(Guid.Empty);

            Factory.GetCommentEngine().SaveOrUpdate(comment);

            NotifyNewComment(comment, entity, isNew);

            Subscribe(entity, SecurityContext.CurrentAccount.ID);

            return comment;
        }

        private void NotifyNewComment(Comment comment, ProjectEntity entity, bool isNew)
        {
            if (Factory.DisableNotifications) return;

            var senders = GetSubscribers(entity);

            NotifyClient.Instance.SendNewComment(senders, entity, comment, isNew);
        }

        #endregion
    }
}