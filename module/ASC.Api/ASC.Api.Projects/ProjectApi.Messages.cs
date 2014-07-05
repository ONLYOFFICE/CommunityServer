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
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        ///<summary>
        ///Returns the list with the detailed information about all the message matching the filter parameters specified in the request
        ///</summary>
        ///<short>
        /// Get message by filter
        ///</short>
        /// <category>Discussions</category>
        ///<param name="projectid" optional="true"> Project ID</param>
        ///<param name="tag" optional="true">Project Tag</param>
        ///<param name="departament" optional="true">Departament GUID</param>
        ///<param name="participant" optional="true">Participant GUID</param>
        ///<param name="createdStart" optional="true">Minimum value of message creation date</param>
        ///<param name="createdStop" optional="true">Maximum value of message creation date</param>
        ///<param name="lastId">Last message ID</param>
        ///<param name="myProjects">Messages in my projects</param>
        ///<param name="follow">Followed messages</param>
        ///<returns>List of messages</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"message/filter")]
        public IEnumerable<MessageWrapper> GetMessageByFilter(int projectid, int tag, Guid departament, Guid participant,
                                                              ApiDateTime createdStart, ApiDateTime createdStop, int lastId,
                                                              bool myProjects, bool follow)
        {
            var messageEngine = EngineFactory.GetMessageEngine();
            var taskFilter = new TaskFilter
            {
                DepartmentId = departament,
                UserId = participant,
                FromDate = createdStart,
                ToDate = createdStop,
                SortBy = _context.SortBy,
                SortOrder = !_context.SortDescending,
                SearchText = _context.FilterValue,
                TagId = tag,
                Offset = _context.StartIndex,
                Max = _context.Count,
                MyProjects = myProjects,
                LastId = lastId,
                Follow = follow
            };

            if (projectid != 0)
                taskFilter.ProjectIds.Add(projectid);

            _context.SetDataPaginated();
            _context.SetDataFiltered();
            _context.SetDataSorted();
            _context.TotalCount = messageEngine.GetByFilterCount(taskFilter);

            return messageEngine.GetByFilter(taskFilter).NotFoundIfNull().Select(r => new MessageWrapper(r)).ToSmartList();
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
            var project = EngineFactory.GetProjectEngine().GetByID(projectid).NotFoundIfNull();

            if (!ProjectSecurity.CanReadMessages(project)) throw ProjectSecurity.CreateSecurityException();

            return EngineFactory.GetMessageEngine().GetByProject(projectid).Select(x => new MessageWrapper(x)).ToSmartList();
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

            var project = EngineFactory.GetProjectEngine().GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandCreateMessage(project);

            var discussion = new Message
            {
                Content = content,
                Title = title,
                Project = project,
            };

            discussion = EngineFactory.GetMessageEngine().SaveOrUpdate(discussion, notify ?? true, ToGuidList(participants), null);
            MessageService.Send(_context, MessageAction.DiscussionCreated, discussion.Project.Title, discussion.Title);
            
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
        public MessageWrapper UpdateProjectMessage(int messageid, int projectid, string title, string content, string participants, bool? notify)
        {
            var messageEngine = EngineFactory.GetMessageEngine();
            
            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            var project = EngineFactory.GetProjectEngine().GetByID(projectid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);

            discussion.Project = Update.IfNotEmptyAndNotEquals(discussion.Project, project);
            discussion.Content = Update.IfNotEmptyAndNotEquals(discussion.Content, content);
            discussion.Title = Update.IfNotEmptyAndNotEquals(discussion.Title, title);

            discussion = messageEngine.SaveOrUpdate(discussion, notify ?? true, ToGuidList(participants), null);
            MessageService.Send(_context, MessageAction.DiscussionUpdated, discussion.Project.Title, discussion.Title);

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
            var discussionEngine = EngineFactory.GetMessageEngine();
            
            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandEdit(discussion);
            
            discussionEngine.Delete(discussion);
            MessageService.Send(_context, MessageAction.DiscussionDeleted, discussion.Project.Title, discussion.Title);
            
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
        public MessageWrapper GetProjectMessage(int messageid)
        {
            return new MessageWrapper(EngineFactory.GetMessageEngine().GetByID(messageid).NotFoundIfNull());
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
            var messageEngine = EngineFactory.GetMessageEngine();
            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandReadFiles(message.Project);

            return messageEngine.GetFiles(message).Select(x => new FileWrapper(x)).ToSmartList();
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
            var messageEngine = EngineFactory.GetMessageEngine();
            var fileEngine = EngineFactory.GetFileEngine();

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var filesList = files.ToList();
            var fileNames = new List<string>();
            foreach (var fileid in filesList)
            {
                var file = fileEngine.GetFile(fileid, 1).NotFoundIfNull();
                fileNames.Add(file.Title);
                messageEngine.AttachFile(discussion, file.ID, true);
            }

            MessageService.Send(_context, MessageAction.DiscussionAttachedFiles, discussion.Project.Title, discussion.Title, fileNames);

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
            var messageEngine = EngineFactory.GetMessageEngine();
            var fileEngine = EngineFactory.GetFileEngine();

            var discussion = messageEngine.GetByID(messageid).NotFoundIfNull();
            ProjectSecurity.DemandReadFiles(discussion.Project);

            var file = fileEngine.GetFile(fileid, 1).NotFoundIfNull();
            
            messageEngine.DetachFile(discussion, fileid);
            MessageService.Send(_context, MessageAction.DiscussionDetachedFile, discussion.Project.Title, discussion.Title, file.Title);

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
            var messages = EngineFactory.GetMessageEngine().GetMessages((int)_context.StartIndex, (int)_context.Count).Select(x => new MessageWrapper(x));
            _context.SetDataPaginated();
            return messages.ToSmartList();
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
            var messageEngine = EngineFactory.GetMessageEngine();

            if (!messageEngine.IsExists(messageid)) throw new ItemNotFoundException();
            return EngineFactory.GetCommentEngine().GetComments(messageEngine.GetByID(messageid)).Select(x => new CommentWrapper(x)).ToSmartList();
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
            var messageEngine = EngineFactory.GetMessageEngine();
            var commentEngine = EngineFactory.GetCommentEngine();

            if (!messageEngine.IsExists(messageid)) throw new ItemNotFoundException();
            if (string.IsNullOrEmpty(content)) throw new ArgumentException(@"Comment text is empty", content);
            if (parentId != Guid.Empty && commentEngine.GetByID(parentId) == null) throw new ItemNotFoundException("parent comment not found");

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

            comment = messageEngine.SaveOrUpdateComment(messageEngine.GetByID(messageid).NotFoundIfNull(), comment);

            var message = messageEngine.GetByID(messageid);
            MessageService.Send(_context, MessageAction.DiscussionCommentCreated, message.Project.Title, message.Title);
            
            return new CommentWrapper(comment);
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
            var discussionEngine = EngineFactory.GetMessageEngine();
            var discussion = discussionEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            discussionEngine.Follow(discussion);
            MessageService.Send(_context, MessageAction.DiscussionUpdatedFollowing, discussion.Project.Title, discussion.Title);

            return new MessageWrapper(discussion);
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
        [Read(@"message/{message:[0-9]+}/subscribe")]
        public bool IsSubscribedToMessage(int messageid)
        {
            var messageEngine = EngineFactory.GetMessageEngine();

            var message = messageEngine.GetByID(messageid).NotFoundIfNull();

            ProjectSecurity.DemandAuthentication();

            return messageEngine.IsSubscribed(message);
        }
    }
}
