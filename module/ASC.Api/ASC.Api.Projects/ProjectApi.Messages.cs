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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Documents;
using ASC.Api.Employee;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.Core;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Specific;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Projects.Engine;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        /// <summary>
        /// Returns the list with the detailed information about all the message matching the filter parameters specified in the request
        /// </summary>
        /// <short>
        ///  Get message by filter
        /// </short>
        ///  <category>Discussions</category>
        /// <param name="projectid" optional="true"> Project ID</param>
        /// <param name="tag" optional="true">Project Tag</param>
        /// <param name="departament" optional="true">Departament GUID</param>
        /// <param name="participant" optional="true">Participant GUID</param>
        /// <param name="createdStart" optional="true">Minimum value of message creation date</param>
        /// <param name="createdStop" optional="true">Maximum value of message creation date</param>
        /// <param name="lastId">Last message ID</param>
        /// <param name="myProjects">Messages in my projects</param>
        /// <param name="follow">Followed messages</param>
        /// <param name="status"></param>
        /// <returns>List of messages</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        [Read(@"message/filter")]
        public IEnumerable<MessageWrapper> GetMessageByFilter(int projectid, int tag, Guid departament, Guid participant,
                                                              ApiDateTime createdStart, ApiDateTime createdStop, int lastId,
                                                              bool myProjects, bool follow, MessageStatus? status)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var filter = CreateFilter();

            filter.DepartmentId = departament;
            filter.UserId = participant;
            filter.FromDate = createdStart;
            filter.ToDate = createdStop;
            filter.TagId = tag;
            filter.MyProjects = myProjects;
            filter.LastId = lastId;
            filter.Follow = follow;
            filter.MessageStatus = status;

            if (projectid != 0)
                filter.ProjectIds.Add(projectid);

            SetTotalCount(messageEngine.GetByFilterCount(filter));

            return messageEngine.GetByFilter(filter).NotFoundIfNull().Select(r => new MessageWrapper(r));
        }

        ///<summary>
        ///Returns the list of all the messages in the discussions within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Messages
        ///</short>
        /// <category>Discussions</category>
        ///<param name="projectid">Project ID</param>
        ///<returns>List of messages</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"{projectid:[0-9]+}/message")]
        public IEnumerable<MessageWrapper> GetProjectMessages(int projectid)
        {
            var project = EngineFactory.ProjectEngine.GetByID(projectid).NotFoundIfNull();

            if (!ProjectSecurity.CanReadMessages(project)) throw ProjectSecurity.CreateSecurityException();

            return EngineFactory.MessageEngine.GetByProject(projectid).Select(x => new MessageWrapper(x));
        }

        ///<summary>
        ///Adds a message to the selected discussion within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Add message
        ///</short>
        /// <category>Discussions</category>
        ///<param name="projectid">Project ID</param>
        ///<param name="title">Discussion title</param>
		///<param name="content">Message text</param>
        ///<param name="participants">IDs (GUIDs) of users separated with ','</param>
        ///<param name="notify">Notify participants</param>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"{projectid:[0-9]+}/message")]
        public MessageWrapper AddProjectMessage(int projectid, string title, string content, string participants, bool? notify)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(@"description can't be empty", "content");

            var project = EngineFactory.ProjectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandCreateMessage(project);

            var messageEngine = EngineFactory.MessageEngine;
            var discussion = new Message
            {
                Description = content,
                Title = title,
                Project = project,
            };

            messageEngine.SaveOrUpdate(discussion, notify.HasValue ? notify.Value : true, ToGuidList(participants));
            MessageService.Send(Request, MessageAction.DiscussionCreated, discussion.Project.Title, discussion.Title);
            
            return new MessageWrapper(discussion);
        }

        ///<summary>
        ///Updates the selected message in the discussion within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update message
        ///</short>
        /// <category>Discussions</category>
        ///<param name="messageid">Message ID</param>
        ///<param name="projectid">Project ID</param>
        ///<param name="title">Discussion title</param>
        ///<param name="content">Message text</param>
        ///<param name="participants">IDs (GUIDs) of users separated with ','</param>
        ///<param name="notify">Notify participants</param>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"message/{messageid:[0-9]+}")]
        public MessageWrapperFull UpdateProjectMessage(int messageid, int projectid, string title, string content, string participants, bool? notify)
        {
            var messageEngine = EngineFactory.MessageEngine;
            
            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            var project = EngineFactory.ProjectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);

            discussion.Project = Update.IfNotEmptyAndNotEquals(discussion.Project, project);
            discussion.Description = Update.IfNotEmptyAndNotEquals(discussion.Description, content);
            discussion.Title = Update.IfNotEmptyAndNotEquals(discussion.Title, title);

            messageEngine.SaveOrUpdate(discussion, notify.HasValue ? notify.Value : true, ToGuidList(participants));
            MessageService.Send(Request, MessageAction.DiscussionUpdated, discussion.Project.Title, discussion.Title);

            return new MessageWrapperFull(discussion, new ProjectWrapperFull(discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID)),  GetProjectMessageSubscribers(messageid));
        }

        ///<summary>
        ///Updates the selected message status
        ///</summary>
        ///<short>
        ///Update message status
        ///</short>
        /// <category>Discussions</category>
        ///<param name="messageid">Message ID</param>
        ///<param name="status">Project ID</param>
        ///<returns></returns>
        ///<exception cref="ArgumentException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"message/{messageid:[0-9]+}/status")]
        public MessageWrapper UpdateProjectMessage(int messageid, MessageStatus status)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);

            discussion.Status = status;
            messageEngine.ChangeStatus(discussion);
            MessageService.Send(Request, MessageAction.DiscussionUpdated, discussion.Project.Title, discussion.Title);

            return new MessageWrapper(discussion);
        }

        ///<summary>
        ///Deletes the message with the ID specified in the request from a project discussion
        ///</summary>
        ///<short>
        ///Delete message
        ///</short>
        /// <category>Discussions</category>
        ///<param name="messageid">Message ID</param>
        ///<returns></returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"message/{messageid:[0-9]+}")]
        public MessageWrapper DeleteProjectMessage(int messageid)
        {
            var discussionEngine = EngineFactory.MessageEngine;
            
            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);
            
            discussionEngine.Delete(discussion);
            MessageService.Send(Request, MessageAction.DiscussionDeleted, discussion.Project.Title, discussion.Title);
            
            return new MessageWrapper(discussion);
        }

        private static IEnumerable<Guid> ToGuidList(string participants)
        {
            return !string.IsNullOrEmpty(participants) ?
                participants.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Guid(x))
                : new List<Guid>();
        }

        ///<summary>
        ///Returns the detailed information about the message with the ID specified in the request
        ///</summary>
        ///<short>
        ///Message
        ///</short>
        /// <category>Discussions</category>
        ///<param name="messageid">Message ID</param>
        ///<returns>Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/{messageid:[0-9]+}")]
        public MessageWrapperFull GetProjectMessage(int messageid)
        {
            var discussion = EngineFactory.MessageEngine.GetByID(messageid).NotFoundIfNull();
            var project = new ProjectWrapperFull(discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID));
            var subscribers = GetProjectMessageSubscribers(messageid);
            var files = EngineFactory.MessageEngine.GetFiles(discussion).Select(x => new FileWrapper(x));
            var comments = EngineFactory.CommentEngine.GetComments(discussion);
            return new MessageWrapperFull(discussion, project, subscribers, files, comments.Where(r=>r.Parent.Equals(Guid.Empty)).Select(x => GetCommentInfo(comments, x, discussion)).ToList());
        }

        ///<summary>
        ///Returns the detailed information about files attached to the message with the ID specified in the request
        ///</summary>
        ///<short>
        ///Message files
        ///</short>
        /// <category>Files</category>
        ///<param name="messageid">Message ID</param>
        ///<returns> List of message files</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/{messageid:[0-9]+}/files")]
        public IEnumerable<FileWrapper> GetMessageFiles(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(message.Project);

            return messageEngine.GetFiles(message).Select(x => new FileWrapper(x));
        }

        ///<summary>
        /// Uploads the file specified in the request to the selected message
        ///</summary>
        ///<short>
        /// Upload file to message
        ///</short>
        /// <category>Files</category>
        ///<param name="messageid">Message ID</param>
        ///<param name="files">File ID</param>
        ///<returns>Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"message/{messageid:[0-9]+}/files")]
        public MessageWrapper UploadFilesToMessage(int messageid, IEnumerable<int> files)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var fileEngine = EngineFactory.FileEngine;

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var filesList = files.ToList();
            var fileNames = new List<string>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                fileNames.Add(file.Title);
                messageEngine.AttachFile(discussion, file.ID, true);
            }

            MessageService.Send(Request, MessageAction.DiscussionAttachedFiles, discussion.Project.Title, discussion.Title, fileNames);

            return new MessageWrapper(discussion);
        }

        ///<summary>
        /// Detaches the selected file from the message with the ID specified in the request
        ///</summary>
        ///<short>
        /// Detach file from message
        ///</short>
        /// <category>Files</category>
		///<param name="messageid">Message ID</param>
		///<param name="fileid">File ID</param>
        ///<returns>Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"message/{messageid:[0-9]+}/files")]
        public MessageWrapper DetachFileFromMessage(int messageid, int fileid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var file = EngineFactory.FileEngine.GetFile(fileid).NotFoundIfNull();
            
            messageEngine.DetachFile(discussion, fileid);
            MessageService.Send(Request, MessageAction.DiscussionDetachedFile, discussion.Project.Title, discussion.Title, file.Title);

            return new MessageWrapper(discussion);
        }

        ///<summary>
        ///Returns the list of latest messages in the discussions within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Latest messages
        ///</short>
        /// <category>Discussions</category>
        ///<returns>List of messages</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message")]
        public IEnumerable<MessageWrapper> GetProjectRecentMessages()
        {
            return EngineFactory.MessageEngine.GetMessages((int)StartIndex, (int)Count).Select(x => new MessageWrapper(x));
        }

        ///<summary>
        ///Returns the list of comments to the messages in the discussions within the project with the ID specified in the request
        ///</summary>
        ///<short>
        ///Message comments
        ///</short>
        /// <category>Comments</category>
        ///<param name="messageid">Message ID</param>
        ///<returns>Comments for message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/{messageid:[0-9]+}/comment")]
        public IEnumerable<CommentWrapper> GetProjectMessagesComments(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var message = messageEngine.GetByID(messageid).NotFoundIfNull();
            
            return EngineFactory.CommentEngine.GetComments(message).Select(x => new CommentWrapper(x, message));
        }

        ///<summary>
        ///Adds a comment to the selected message in a discussion within the project with the content specified in the request. The parent comment ID can also be selected.
        ///</summary>
        ///<short>
        ///Add message comment
        ///</short>
        /// <category>Comments</category>
        ///<param name="messageid">Message ID</param>
        ///<param name="content">Comment content</param>
        ///<param name="parentId">Parrent comment ID</param>
        ///<returns></returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Create(@"message/{messageid:[0-9]+}/comment")]
        public CommentWrapper AddProjectMessagesComment(int messageid, string content, Guid parentId)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(@"Comment text is empty", content);
            if (parentId != Guid.Empty && EngineFactory.CommentEngine.GetByID(parentId) == null) throw new ItemNotFoundException("parent comment not found");

            var comment = new Comment
            {
                Content = content,
                TargetUniqID = ProjectEntity.BuildUniqId<Message>(messageid),
                CreateBy = CurrentUserId,
                CreateOn = Core.Tenants.TenantUtil.DateTimeNow()
            };

            if (parentId != Guid.Empty)
            {
                comment.Parent = parentId;
            }

            var message = EngineFactory.CommentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();

            EngineFactory.CommentEngine.SaveOrUpdateComment(message, comment);
            
            MessageService.Send(Request, MessageAction.DiscussionCommentCreated, message.Project.Title, message.Title);
            
            return new CommentWrapper(comment, message);
        }

        ///<summary>
        ///Subscribe to notifications about the actions performed with the task with the ID specified in the request
        ///</summary>
        ///<short>
        ///Subscribe to message action
        ///</short>
        /// <category>Discussions</category>
        /// <returns>Discussion</returns>
		///<param name="messageid">Message ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Update(@"message/{messageid:[0-9]+}/subscribe")]
        public MessageWrapper SubscribeToMessage(int messageid)
        {
            var discussionEngine = EngineFactory.MessageEngine;
            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            discussionEngine.Follow(discussion);
            MessageService.Send(Request, MessageAction.DiscussionUpdatedFollowing, discussion.Project.Title, discussion.Title);

            return new MessageWrapperFull(discussion, new ProjectWrapperFull(discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID)), GetProjectMessageSubscribers(messageid));
        }

        ///<summary>
        ///Checks subscription to notifications about the actions performed with the discussion with the ID specified in the request
        ///</summary>
        ///<short>
        ///Check subscription to discussion action
        ///</short>
        /// <category>Discussions</category>
		///<param name="messageid">Message ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/{messageid:[0-9]+}/subscribe")]
        public bool IsSubscribedToMessage(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return messageEngine.IsSubscribed(message);
        }

        ///<summary>
        ///Get subscribers
        ///</summary>
        ///<short>
        ///Get subscribers
        ///</short>
        /// <category>Discussions</category>
        ///<param name="messageid">Message ID</param>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/{messageid:[0-9]+}/subscribes")]
        public IEnumerable<EmployeeWraperFull> GetProjectMessageSubscribers(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return messageEngine.GetSubscribers(message).Select(r=> new EmployeeWraperFull(CoreContext.UserManager.GetUsers(new Guid(r.ID))))
                .OrderBy(r=> r.DisplayName).ToList();
        }

        ///<summary>
        ///Get preview
        ///</summary>
        ///<short>
        ///Get preview
        ///</short>
        /// <category>Discussions</category>
        ///<param name="htmltext">html to create preview</param>
        [Create(@"message/discussion/preview")]
        public string GetPreview(string htmltext)
        {
            return HtmlUtility.GetFull(htmltext);
        }
    }
}
