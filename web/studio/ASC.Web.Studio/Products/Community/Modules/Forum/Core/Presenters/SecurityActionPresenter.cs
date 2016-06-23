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
using ASC.Common.Security;
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

            switch (e.Action)
            {
                case ForumAction.ReadPosts:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.ReadPostsAction);
                    break;

                case ForumAction.PostCreate:
                    
                    Topic topic = (Topic)e.TargetObject;    
                    if (CommunitySecurity.CheckPermissions(topic, Constants.PostCreateAction))
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
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TopicCloseAction);
                    break;

                case ForumAction.TopicSticky:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TopicStickyAction);
                    break;

                case ForumAction.TopicEdit:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TopicEditAction);
                    break;

                case ForumAction.TopicDelete:
                    _view.IsAccessible = CommunitySecurity.CheckPermissions(securityObject, Constants.TopicDeleteAction);
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
