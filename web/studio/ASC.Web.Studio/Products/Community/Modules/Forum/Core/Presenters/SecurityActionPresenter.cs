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
using ASC.Common.Security;
using ASC.Core;
using ASC.Forum.Module;
using ASC.Web.Community.Product;

namespace ASC.Forum
{   

    internal class SecurityActionPresenter : PresenterTemplate<ISecurityActionView>
    {
        protected override void RegisterView()
        {
            _view.ValidateAccess+=new EventHandler<SecurityAccessEventArgs>(ValidateAccessHandler);
        }

        private void ValidateAccessHandler(object sender, SecurityAccessEventArgs e)
        {
            ISecurityObject securityObject = null;
            if (e.TargetObject is ISecurityObject)
                securityObject = (ISecurityObject)e.TargetObject;
            var topic = e.TargetObject as Topic;
            var isTopicAutor = topic != null && topic.PosterID == SecurityContext.CurrentAccount.ID;

            switch (e.Action)
            {
                case ForumAction.ReadPosts:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.ReadPostsAction);
                    break;

                case ForumAction.PostCreate:
                    if (topic == null || CommunitySecurity.CheckPermissions(topic, Constants.PostCreateAction))
                    {   
                        if(!topic.Closed)
                            _view.IsAccessible = true;

                        else if (topic.Closed && CommunitySecurity.CheckPermissions(topic, Constants.TopicCloseAction))
                            _view.IsAccessible = true;

                        else
                            _view.IsAccessible = false;
                    }
                    else
                        _view.IsAccessible = false;

                    break;
                    
                case ForumAction.ApprovePost:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.PostApproveAction);
                    break;

                case ForumAction.PostEdit:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.PostEditAction);
                    break;

                case ForumAction.PostDelete:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.PostDeleteAction);
                    break;

                case ForumAction.TopicCreate:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TopicCreateAction);
                    break;

                case ForumAction.PollCreate:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.PollCreateAction);
                    break;

                case ForumAction.TopicClose:
                    _view.IsAccessible = isTopicAutor || CommunitySecurity.CheckPermissions(securityObject, Constants.TopicCloseAction);
                    break;

                case ForumAction.TopicSticky:
                    _view.IsAccessible = isTopicAutor || CommunitySecurity.CheckPermissions(securityObject, Constants.TopicStickyAction);
                    break;

                case ForumAction.TopicEdit:
                    _view.IsAccessible = isTopicAutor || CommunitySecurity.CheckPermissions(securityObject, Constants.TopicEditAction);
                    break;

                case ForumAction.TopicDelete:
                    _view.IsAccessible = isTopicAutor || CommunitySecurity.CheckPermissions(securityObject, Constants.TopicDeleteAction);
                    break;

                case ForumAction.PollVote:

                    Question question = (Question)e.TargetObject;
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(new Topic() { ID = question.TopicID}, Constants.PollVoteAction);
                    break;


                case ForumAction.TagCreate:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TagCreateAction);
                    break;
                
                case ForumAction.AttachmentCreate:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.AttachmentCreateAction);
                    break;

                case ForumAction.AttachmentDelete:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.AttachmentDeleteAction);
                    break;
               
                case ForumAction.GetAccessForumEditor:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.ForumManagementAction);
                    break;

                case ForumAction.GetAccessTagEditor:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TagManagementAction);
                    break;
            }
        }
    }
}
