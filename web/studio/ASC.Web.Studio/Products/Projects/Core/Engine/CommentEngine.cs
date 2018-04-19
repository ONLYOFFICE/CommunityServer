/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Generic;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;

namespace ASC.Projects.Engine
{
    public class CommentEngine
    {
        public IDaoFactory DaoFactory { get; set; }
        public bool DisableNotifications { get; set; }
        public TaskEngine TaskEngine { get; set; }
        public MessageEngine MessageEngine { get; set; }

        public CommentEngine(bool disableNotifications, EngineFactory factory)
        {
            DisableNotifications = disableNotifications;
            TaskEngine = factory.TaskEngine;
            MessageEngine = factory.MessageEngine;
        }

        public List<Comment> GetComments(DomainObject<int> targetObject)
        {
            return targetObject != null ? DaoFactory.CommentDao.GetAll(targetObject) : new List<Comment>();
        }

        public Comment GetByID(Guid id)
        {
            return DaoFactory.CommentDao.GetById(id);
        }

        public int Count(DomainObject<int> targetObject)
        {
            return targetObject == null ? 0 : DaoFactory.CommentDao.Count(targetObject);
        }

        public List<int> Count(List<ProjectEntity> targets)
        {
            return DaoFactory.CommentDao.Count(targets);
        }

        public int Count(ProjectEntity target)
        {
            return DaoFactory.CommentDao.Count(target);
        }

        public void SaveOrUpdate(Comment comment)
        {
            if (comment == null) throw new ArgumentNullException("comment");

            if (comment.CreateBy == default(Guid)) comment.CreateBy = SecurityContext.CurrentAccount.ID;

            var now = TenantUtil.DateTimeNow();
            if (comment.CreateOn == default(DateTime)) comment.CreateOn = now;

            DaoFactory.CommentDao.Save(comment);
        }

        public ProjectEntity GetEntityByTargetUniqId(Comment comment)
        {
            var engine = GetProjectEntityEngine(comment);
            if (engine == null) return null;

            return engine.GetEntityByID(comment.TargetID);
        }

        public Comment SaveOrUpdateComment(ProjectEntity entity, Comment comment)
        {
            var isNew = comment.OldGuidId.Equals(Guid.Empty);

            if (isNew)
            {
                ProjectSecurity.DemandCreateComment(entity);
            }
            else
            {
                var message = entity as Message;
                if (message != null)
                {
                    ProjectSecurity.DemandEditComment(message, comment);
                }
                else
                {
                    ProjectSecurity.DemandEditComment(entity.Project, comment);
                }
            }

            SaveOrUpdate(comment);

            NotifyNewComment(entity, comment, isNew);

            GetProjectEntityEngine(comment).Subscribe(entity, SecurityContext.CurrentAccount.ID);

            return comment;
        }

        private void NotifyNewComment(ProjectEntity entity, Comment comment, bool isNew)
        {
            if (DisableNotifications) return;

            var senders = GetProjectEntityEngine(comment).GetSubscribers(entity);

            NotifyClient.Instance.SendNewComment(senders, entity, comment, isNew);
        }

        private ProjectEntityEngine GetProjectEntityEngine(Comment comment)
        {
            switch (comment.TargetType)
            {
                case "Task":
                    return TaskEngine;
                case "Message":
                    return MessageEngine;
                default:
                    return null;
            }
        }
    }
}