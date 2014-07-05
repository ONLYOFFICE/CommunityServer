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
