/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Api.Attributes;
using ASC.Api.Documents;
using ASC.Api.Employee;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        /// <summary>
        /// Returns a list with the detailed information about all the messages matching the filter parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered messages
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="projectid" optional="true"> Project ID</param>
        /// <param type="System.Int32, System" method="url" name="tag" optional="true">Project tag</param>
        /// <param type="System.Guid, System" method="url" name="departament" optional="true">Departament GUID</param>
        /// <param type="System.Guid, System" method="url" name="participant" optional="true">Participant GUID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStart" optional="true">Minimum value of message creation date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="createdStop" optional="true">Maximum value of message creation date</param>
        /// <param type="System.Int32, System" method="url" name="lastId">Last message ID</param>
        /// <param type="System.Boolean, System" method="url" name="myProjects">Specifies whether to return messages only from my projects or not</param>
        /// <param type="System.Boolean, System" method="url" name="follow">Specifies whether to return messages only from followed discussions or not</param>
        /// <param type="System.Nullable{ASC.Projects.Core.Domain.MessageStatus}, System" method="url" name="status">Message status ("Open" or "Archived")</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">List of messages</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"message/filter")]
        public IEnumerable<MessageWrapper> GetMessageByFilter(int projectid, int tag, Guid departament, Guid participant,
                                                              ApiDateTime createdStart, ApiDateTime createdStop, int lastId,
                                                              bool myProjects, bool follow, MessageStatus? status)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var filter = CreateFilter(EntityType.Message);

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

            return messageEngine.GetByFilter(filter).NotFoundIfNull().Select(MessageWrapperSelector).ToList();
        }

        /// <summary>
        /// Returns a list of all the discussion messages within a project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get messages
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">List of messages</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/{projectid}/message</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"{projectid:[0-9]+}/message")]
        public IEnumerable<MessageWrapper> GetProjectMessages(int projectid)
        {
            var project = EngineFactory.ProjectEngine.GetByID(projectid).NotFoundIfNull();

            if (!ProjectSecurity.CanRead<Message>(project)) throw ProjectSecurity.CreateSecurityException();

            return EngineFactory.MessageEngine.GetByProject(projectid).Select(MessageWrapperSelector).ToList();
        }

        /// <summary>
        /// Adds a message to the selected discussion within a project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Add a message
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="projectid">Project ID</param>
        /// <param type="System.String, System" name="title">Discussion title</param>
        /// <param type="System.String, System" name="content">Message text</param>
        /// <param type="System.String, System" name="participants">User IDs (GUIDs) separated with ','</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="notify">Specifies whether to notify participants about a message or not</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Message</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/{projectid}/message</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"{projectid:[0-9]+}/message")]
        public MessageWrapper AddProjectMessage(int projectid, string title, string content, string participants, bool? notify)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException(@"title can't be empty", "title");
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(@"description can't be empty", "content");

            var project = EngineFactory.ProjectEngine.GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandCreate<Message>(project);

            var messageEngine = EngineFactory.MessageEngine;
            var discussion = new Message
            {
                Description = content,
                Title = title,
                Project = project,
            };

            messageEngine.SaveOrUpdate(discussion, notify.HasValue ? notify.Value : true, ToGuidList(participants));
            MessageService.Send(Request, MessageAction.DiscussionCreated, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title);

            return MessageWrapperSelector(discussion);
        }

        /// <summary>
        /// Updates the selected discussion message from a project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Update a message
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <param type="System.Int32, System" name="projectid">Project ID</param>
        /// <param type="System.String, System" name="title">Discussion title</param>
        /// <param type="System.String, System" name="content">New message text</param>
        /// <param type="System.String, System" name="participants">New user IDs (GUIDs) separated with ','</param>
        /// <param type="System.Nullable{System.Boolean}, System" name="notify">Specifies whether to notify participants about a message or not</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapperFull, ASC.Api.Projects">Updated message</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}</path>
        /// <httpMethod>PUT</httpMethod>
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
            MessageService.Send(Request, MessageAction.DiscussionUpdated, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title);

            return new MessageWrapperFull(this, discussion, new ProjectWrapperFull(this, discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID)), GetProjectMessageSubscribers(messageid));
        }

        /// <summary>
        /// Updates the selected message status.
        /// </summary>
        /// <short>
        /// Update a message status
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <param type="ASC.Projects.Core.Domain.MessageStatus, ASC.Projects.Core.Domain" name="status">New message status ("Open" or "Archived")</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Updated message</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/status</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"message/{messageid:[0-9]+}/status")]
        public MessageWrapper UpdateProjectMessage(int messageid, MessageStatus status)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);

            discussion.Status = status;
            messageEngine.ChangeStatus(discussion);
            MessageService.Send(Request, MessageAction.DiscussionUpdated, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title);

            return MessageWrapperSelector(discussion);
        }

        /// <summary>
        /// Deletes a message with the ID specified in the request from a project discussion.
        /// </summary>
        /// <short>
        /// Delete a message
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Message</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"message/{messageid:[0-9]+}")]
        public MessageWrapper DeleteProjectMessage(int messageid)
        {
            var discussionEngine = EngineFactory.MessageEngine;

            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);

            discussionEngine.Delete(discussion);
            MessageService.Send(Request, MessageAction.DiscussionDeleted, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title);

            return MessageWrapperSelector(discussion);
        }

        private static IEnumerable<Guid> ToGuidList(string participants)
        {
            return participants != null ?
                 participants.Equals(string.Empty) ? new List<Guid>() : participants.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Guid(x))
                : null;
        }

        /// <summary>
        /// Returns the detailed information about a discussion message with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a message
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapperFull, ASC.Api.Projects">Message</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"message/{messageid:[0-9]+}")]
        public MessageWrapperFull GetProjectMessage(int messageid)
        {
            var discussion = EngineFactory.MessageEngine.GetByID(messageid).NotFoundIfNull();
            var project = ProjectWrapperFullSelector(discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID));
            var subscribers = GetProjectMessageSubscribers(messageid);
            var files = EngineFactory.MessageEngine.GetFiles(discussion).Select(FileWrapperSelector);
            var comments = EngineFactory.CommentEngine.GetComments(discussion);
            return new MessageWrapperFull(this, discussion, project, subscribers, files, comments.Where(r => r.Parent.Equals(Guid.Empty)).Select(x => GetCommentInfo(comments, x, discussion)).ToList());
        }

        /// <summary>
        /// Returns the detailed information about files attached to the discussion message with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get message files
        /// </short>
        /// <category>Files</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">List of message files</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/files</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"message/{messageid:[0-9]+}/files")]
        public IEnumerable<FileWrapper> GetMessageFiles(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(message.Project);

            return messageEngine.GetFiles(message).Select(FileWrapperSelector);
        }

        /// <summary>
        /// Uploads files specified in the request to the selected discussion message.
        /// </summary>
        /// <short>
        /// Upload files to the message
        /// </short>
        /// <category>Files</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="files">File IDs</param>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Message</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/files</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"message/{messageid:[0-9]+}/files")]
        public MessageWrapper UploadFilesToMessage(int messageid, IEnumerable<int> files)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var fileEngine = EngineFactory.FileEngine;

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var filesList = files.ToList();
            var attachments = new List<Files.Core.File>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                attachments.Add(file);
                messageEngine.AttachFile(discussion, file.ID, true);
            }

            MessageService.Send(Request, MessageAction.DiscussionAttachedFiles, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title, attachments.Select(x => x.Title));

            return MessageWrapperSelector(discussion);
        }

        ///<summary>
        ///Detaches the selected file from the discussion message with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Detach a file from a message
        ///</short>
        ///<category>Files</category>
        ///<param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        ///<param type="System.Int32, System" name="fileid">File ID</param>
        ///<returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<path>api/2.0/project/message/{messageid}/files</path>
        ///<httpMethod>DELETE</httpMethod>
        [Delete(@"message/{messageid:[0-9]+}/files")]
        public MessageWrapper DetachFileFromMessage(int messageid, int fileid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var file = EngineFactory.FileEngine.GetFile(fileid).NotFoundIfNull();

            messageEngine.DetachFile(discussion, fileid);
            MessageService.Send(Request, MessageAction.DiscussionDetachedFile, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title, file.Title);

            return MessageWrapperSelector(discussion);
        }

        ///<summary>
        ///Detaches the selected files from the discussion message with the ID specified in the request.
        ///</summary>
        ///<short>
        ///Detach files from a message
        ///</short>
        ///<category>Discussions</category>
        ///<param type="System.Int32, System" name="messageid">Message ID</param>
        ///<param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="files">File IDs</param>
        ///<returns>Message</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<path>api/2.0/project/message/{messageid}/filesmany</path>
        /// <httpMethod>DELETE</httpMethod>
        ///<visible>false</visible>
        [Delete(@"message/{messageid:[0-9]+}/filesmany")]
        public MessageWrapper DetachFileFromMessage(int messageid, IEnumerable<int> files)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var fileEngine = EngineFactory.FileEngine;

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var filesList = files.ToList();
            var attachments = new List<Files.Core.File>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid).NotFoundIfNull();
                attachments.Add(file);
                messageEngine.DetachFile(discussion, fileid);
            }

            MessageService.Send(Request, MessageAction.DiscussionDetachedFile, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title, attachments.Select(x => x.Title));

            return MessageWrapperSelector(discussion);
        }

        /// <summary>
        /// Returns a list of the recent discussion messages from a project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get recent messages
        /// </short>
        /// <category>Discussions</category>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">List of recent messages</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"message")]
        public IEnumerable<MessageWrapper> GetProjectRecentMessages()
        {
            return EngineFactory.MessageEngine.GetMessages((int)StartIndex, (int)Count).Select(MessageWrapperSelector);
        }

        /// <summary>
        /// Returns a list of the discussion message comments from a project with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get message comments
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.CommentWrapper, ASC.Api.Projects">Message comments</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/comment</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"message/{messageid:[0-9]+}/comment")]
        public IEnumerable<CommentWrapper> GetProjectMessagesComments(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;
            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            return EngineFactory.CommentEngine.GetComments(message).Select(x => new CommentWrapper(this, x, message));
        }

        /// <summary>
        /// Adds a comment to the selected discussion message with the text specified in the request. The parent comment ID can be also selected.
        /// </summary>
        /// <short>
        /// Add a message comment
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <param type="System.String, System" name="content">Comment text</param>
        /// <param type="System.Guid, System" name="parentId">Parent comment ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.CommentWrapper, ASC.Api.Projects">Comment</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/comment</path>
        /// <httpMethod>POST</httpMethod>
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

            MessageService.Send(Request, MessageAction.DiscussionCommentCreated, MessageTarget.Create(comment.ID), message.Project.Title, message.Title);

            return new CommentWrapper(this, comment, message);
        }

        /// <summary>
        /// Subscribes to the notifications about the actions performed in the discussion with the selected message.
        /// </summary>
        /// <short>
        /// Subscribe to discussion
        /// </short>
        /// <category>Discussions</category>
        /// <returns type="ASC.Api.Projects.Wrappers.MessageWrapper, ASC.Api.Projects">Discussion</returns>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/subscribe</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"message/{messageid:[0-9]+}/subscribe")]
        public MessageWrapper SubscribeToMessage(int messageid)
        {
            var discussionEngine = EngineFactory.MessageEngine;
            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            discussionEngine.Follow(discussion);
            MessageService.Send(Request, MessageAction.DiscussionUpdatedFollowing, MessageTarget.Create(discussion.ID), discussion.Project.Title, discussion.Title);

            return new MessageWrapperFull(this, discussion, ProjectWrapperFullSelector(discussion.Project, EngineFactory.FileEngine.GetRoot(discussion.Project.ID)), GetProjectMessageSubscribers(messageid));
        }

        /// <summary>
        /// Checks subscription to the notifications about the actions performed in the discussion with the selected message.
        /// </summary>
        /// <short>
        /// Check the discussion subscription
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns>Boolean value: true - subscibed, false - unsubscribed</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/subscribe</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"message/{messageid:[0-9]+}/subscribe")]
        public bool IsSubscribedToMessage(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return messageEngine.IsSubscribed(message);
        }

        /// <summary>
        /// Returns a list of all the subscribers to the discussion with the selected message.
        /// </summary>
        /// <short>
        /// Get discussion subscribers
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.Int32, System" method="url" name="messageid">Message ID</param>
        /// <returns>List of subscibers</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/message/{messageid}/subscribes</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        /// <returns type="ASC.Api.Employee.EmployeeWraperFull, ASC.Api.Employee"></returns>
        [Read(@"message/{messageid:[0-9]+}/subscribes")]
        public IEnumerable<EmployeeWraperFull> GetProjectMessageSubscribers(int messageid)
        {
            var messageEngine = EngineFactory.MessageEngine;

            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return messageEngine.GetSubscribers(message).Select(r => GetEmployeeWraperFull(new Guid(r.ID)))
                .OrderBy(r => r.DisplayName).ToList();
        }

        /// <summary>
        /// Returns a preview of the discussion message.
        /// </summary>
        /// <short>
        /// Get a message preview
        /// </short>
        /// <category>Discussions</category>
        /// <param type="System.String, System" name="htmltext">Message text in the HTML format</param>
        /// <returns>Message preview</returns>
        /// <path>api/2.0/project/message/discussion/preview</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"message/discussion/preview")]
        public string GetPreview(string htmltext)
        {
            return HtmlUtility.GetFull(htmltext, false);
        }
    }
}
