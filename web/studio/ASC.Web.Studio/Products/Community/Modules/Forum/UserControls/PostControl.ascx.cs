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
using System.Text;
using System.Web;
using System.Web.UI;
using AjaxPro;
using ASC.Core.Users;
using ASC.ElasticSearch;
using ASC.Forum;
using ASC.Web.Community.Search;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Forum.Common;

namespace ASC.Web.UserControls.Forum
{
    [AjaxNamespace("PostManager")]
    public partial class PostControl : UserControl
    {
        public Post Post { get; set; }
        public Guid SettingsID { get; set; }
        protected Settings _settings;
        private ForumManager _forumManager;
        public int PostsCount { get; set; }

        public int CurrentPageNumber { get; set; }

        public PostControl()
        {
            CurrentPageNumber = -1;
        }

        public bool IsEven { get; set; }

        protected string _postCSSClass;
        protected string _messageCSSClass;

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(GetType());

            _settings = Community.Forum.ForumManager.Settings;
            _forumManager = _settings.ForumManager;

            _postCSSClass = IsEven ? "tintMedium" : "";

            _messageCSSClass = "forum_mesBox";
            if (!Post.IsApproved && _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Post))
                _messageCSSClass = "tintDangerous forum_mesBox";

        }

        protected string RenderEditedData()
        {
            if (Post.EditCount <= 0)
                return "";

            var sb = new StringBuilder();
            sb.Append("<div class='text-medium-describe' style='padding:2px 5px;'>" + Resources.ForumUCResource.Edited + "&nbsp;&nbsp;");
            sb.Append(Post.EditDate.ToShortDateString() + " " + Post.EditDate.ToShortTimeString());
            sb.Append("<span style='margin-left:5px;'>" + Post.Editor.RenderCustomProfileLink("describe-text", "link gray") + "</span>");
            sb.Append("</div>");

            return sb.ToString();
        }

        protected void ReferenceToPost()
        {
            string refToPost;
            if (CurrentPageNumber != -1)
                refToPost = "<a class=\"link\" href=\"" + _settings.LinkProvider.Post(Post.ID, Post.TopicID, CurrentPageNumber) + "\">#" + Post.ID.ToString() + "</a>";
            else
                refToPost = "&nbsp;";
            Response.Write(refToPost);
        }

        protected string ControlButtons()
        {
            var topic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, Post.TopicID);
            if (topic.Closed)
                return string.Empty;

            var sb = new StringBuilder();

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostCreate, new Topic {ID = Post.TopicID}))
            {
                sb.Append("<a class=\"link gray\" style=\"float:left;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Quote, Post.ID) + "\">" + Resources.ForumUCResource.Quote + "</a>");
                sb.Append("<a class=\"link gray\" style=\"float:left;  margin-left:8px;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Reply, Post.ID) + "\">" + Resources.ForumUCResource.Reply + "</a>");
                sb.Append("<a class=\"link gray\" style=\"float:left; margin-left:8px;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID) + "\">" + Resources.ForumUCResource.NewPostButton + "</a>");
            }

            var isFirst = true;

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, Post) && ShowDeletePostLink())
            {
                sb.AppendFormat("<a class=\"link\" style=\"float:right;\" id='PostDeleteLink{0}' href=\"javascript:ForumManager.DeletePost('" + Post.ID + "')\">" + Resources.ForumUCResource.DeleteButton + "</a>", Post.ID);
                isFirst = false;
            }

            if (_forumManager.ValidateAccessSecurityAction(ForumAction.PostEdit, Post))
            {
                if (!isFirst && ShowDeletePostLink())
                    sb.AppendFormat("<span class='splitter' id='PostDeleteSplitter{0}' style='float:right;'>|</span>", Post.ID);

                sb.Append("<a class=\"link\" style=\"float:right;\" href=\"" + _settings.LinkProvider.NewPost(Post.TopicID, PostAction.Edit, Post.ID) + "\">" + Resources.ForumUCResource.EditButton + "</a>");
                isFirst = false;
            }

            if (!Post.IsApproved && _forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, Post))
            {
                if (!isFirst)
                    sb.Append("<span class='splitter' style='float:right;'>|</span>");

                sb.Append("<a id=\"forum_btap_" + Post.ID + "\" class=\"link\" style=\"margin-left:5px; float:right;\" href=\"javascript:ForumManager.ApprovePost('" + Post.ID + "')\">" + Resources.ForumUCResource.ApproveButton + "</a>");
            }

            return sb.ToString();
        }

        private bool ShowDeletePostLink()
        {
            return PostsCount > 1;
        }

        public static string AttachmentsList(Post post, Guid settingsID)
        {
            var forumManager = Community.Forum.ForumManager.Settings.ForumManager;
            var sb = new StringBuilder();
            if (post.Attachments.Count <= 0)
                return "";

            var closedTopic = ForumDataProvider.GetTopicByID(TenantProvider.CurrentTenantID, post.TopicID).Closed;

            sb.Append("<div class=\"cornerAll borderBase forum_attachmentsBox\">");
            sb.Append("<div class='headerPanel'>" + Resources.ForumUCResource.AttachFiles + "</div>");
            foreach (var attachment in post.Attachments)
            {
                sb.Append("<div id=\"forum_attach_" + attachment.ID + "\" class=\"borderBase  forum_attachItem clearFix\">");
                sb.Append("<table cellspacing='0' cellpadding='0' style='width:100%;'><tr>");
                sb.Append("<td>");
                sb.Append("<a class = 'link' target=\"_blank\" href=\"" + forumManager.GetAttachmentWebPath(attachment) + "\">" + HttpUtility.HtmlEncode(attachment.Name) + "</a>");
                sb.Append("</td>");

                sb.Append("<td style=\"text-align:right;width:100px;\"><span class=\"text-medium-describe\">" + ((float) attachment.Size/1024f).ToString("####0.##") + " KB</span></td>");

                if (forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentDelete, post) && !closedTopic)
                {
                    sb.Append("<td style=\"text-align:right;width:100px;\">");
                    sb.Append("<a class=\"link\" href=\"javascript:ForumManager.DeleteAttachment('" + attachment.ID + "','" + post.ID + "');\">" + Resources.ForumUCResource.DeleteButton + "</a>");
                    sb.Append("</td>");
                }

                sb.Append("</tr></table>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeletePost(int idPost, Guid settingsID)
        {
            _forumManager = Community.Forum.ForumManager.Settings.ForumManager;
            var resp = new AjaxResponse {rs2 = idPost.ToString()};

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.PostDelete, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                var result = RemoveDataHelper.RemovePost(post);
                if (result == DeletePostResult.Successfully)
                {
                    resp.rs1 = "1";
                    resp.rs3 = Resources.ForumUCResource.SuccessfullyDeletePostMessage;
                }
                else if (result == DeletePostResult.ReferencesBlock)
                {
                    resp.rs1 = "0";
                    resp.rs3 = Resources.ForumUCResource.ExistsReferencesChildPosts;
                }
                else
                {
                    resp.rs1 = "0";
                    resp.rs3 = Resources.ForumUCResource.ErrorDeletePost;
                }
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoApprovedPost(int idPost, Guid settingsID)
        {
            _forumManager = Community.Forum.ForumManager.Settings.ForumManager;
            var resp = new AjaxResponse {rs2 = idPost.ToString()};

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.ApprovePost, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                ForumDataProvider.ApprovePost(TenantProvider.CurrentTenantID, post.ID);
                resp.rs1 = "1";
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }
            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse DoDeleteAttachment(int idAttachment, int idPost, Guid settingsID)
        {
            _forumManager = Community.Forum.ForumManager.Settings.ForumManager;
            var resp = new AjaxResponse {rs2 = idAttachment.ToString()};

            var post = ForumDataProvider.GetPostByID(TenantProvider.CurrentTenantID, idPost);
            if (post == null)
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            if (!_forumManager.ValidateAccessSecurityAction(ForumAction.AttachmentDelete, post))
            {
                resp.rs1 = "0";
                resp.rs3 = Resources.ForumUCResource.ErrorAccessDenied;
                return resp;
            }

            try
            {
                var attachment = post.Attachments.Find(a => a.ID == idAttachment);
                if (attachment != null)
                {
                    ForumDataProvider.RemoveAttachment(TenantProvider.CurrentTenantID, attachment.ID);
                    _forumManager.RemoveAttachments(attachment.OffsetPhysicalPath);
                }

                resp.rs1 = "1";
                resp.rs3 = Resources.ForumUCResource.SuccessfullyDeleteAttachmentMessage;
            }
            catch (Exception e)
            {
                resp.rs1 = "0";
                resp.rs3 = HttpUtility.HtmlEncode(e.Message);
            }

            return resp;
        }
    }
}