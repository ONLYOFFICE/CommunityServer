/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Util;

namespace ASC.Web.UserControls.Bookmarking
{
    [AjaxNamespace("CommentsUserControl")]
    public partial class CommentsUserControl : System.Web.UI.UserControl
    {
        #region Fields

        private readonly BookmarkingServiceHelper _serviceHelper = BookmarkingServiceHelper.GetCurrentInstanse();

        public long BookmarkID { get; set; }

        #region Bookmark Comments

        private IList<Comment> _bookmarkComments;

        public IList<Comment> BookmarkComments
        {
            get { return _bookmarkComments ?? new List<Comment>(); }
            set { _bookmarkComments = value; }
        }

        #endregion

        public CommentsList Comments
        {
            get
            {
                CommentList = _serviceHelper.Comments;
                return _serviceHelper.Comments;
            }
            set { _serviceHelper.Comments = CommentList; }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            InitComments();
        }

        public void InitComments()
        {
            ConfigureComments(CommentList);
            CommentList.Items = BookmarkingConverter.ConvertCommentList(BookmarkComments);
            Comments = CommentList;
        }

        private void ConfigureComments(CommentsList commentList)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            // add comments permission check		
            commentList.IsShowAddCommentBtn = BookmarkingPermissionsCheck.PermissionCheckCreateComment();
            commentList.CommentsCountTitle = BookmarkComments.Count.ToString();
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "CommentsUserControl.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "CommentsUserControl.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "CommentsUserControl.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "CommentsUserControl.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "CommentsUserControl.UpdateComment";
            commentList.FckDomainName = "bookmarking_comments";
            commentList.TotalCount = BookmarkComments.Count;
            commentList.ShowCaption = false;
            commentList.ObjectID = BookmarkID.ToString();
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse AddComment(string parrentCommentID, long bookmarkID, string text, string pid)
        {
            var resp = new AjaxResponse { rs1 = parrentCommentID };

            var comment = new Comment
                {
                    Content = text,
                    Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    UserID = SecurityContext.CurrentAccount.ID
                };

            var parentID = Guid.Empty;
            try
            {
                if (!string.IsNullOrEmpty(parrentCommentID))
                {
                    parentID = new Guid(parrentCommentID);
                }
            }
            catch
            {
                parentID = Guid.Empty;
            }
            comment.Parent = parentID.ToString();
            comment.BookmarkID = bookmarkID;
            comment.ID = Guid.NewGuid();

            _serviceHelper.AddComment(comment);

            resp.rs2 = GetOneCommentHtmlWithContainer(comment);

            return resp;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string LoadCommentBBCode(string commentID)
        {
            if (string.IsNullOrEmpty(commentID))
                return string.Empty;

            var comment = _serviceHelper.GetCommentById(commentID);
            return comment.Content;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string GetPreview(string text, string commentID)
        {
            return GetHTMLComment(text, commentID);
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string RemoveComment(string commentID, string pid)
        {
            var comment = _serviceHelper.GetCommentById(commentID);
            if (comment != null && BookmarkingPermissionsCheck.PermissionCheckEditComment(comment))
            {
                _serviceHelper.RemoveComment(commentID);
                return commentID;
            }
            return null;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            var resp = new AjaxResponse();
            var comment = _serviceHelper.GetCommentById(commentID);
            if (comment != null)
            {
                if (BookmarkingPermissionsCheck.PermissionCheckEditComment(comment))
                {
                    _serviceHelper.UpdateComment(commentID, text);
                    resp.rs1 = commentID;
                    resp.rs2 = HtmlUtility.GetFull(text) + CodeHighlighter.GetJavaScriptLiveHighlight(true);
                }
            }
            return resp;
        }

        private string GetHTMLComment(string text, string commentID)
        {
            var comment = new Comment
                {
                    Datetime = ASC.Core.Tenants.TenantUtil.DateTimeNow(),
                    UserID = SecurityContext.CurrentAccount.ID
                };

            if (!String.IsNullOrEmpty(commentID))
            {
                comment = _serviceHelper.GetCommentById(commentID);

                comment.Parent = string.Empty;
            }
            comment.Content = text;

            var defComment = new CommentsList();

            ConfigureComments(defComment);

            var ci = BookmarkingConverter.ConvertComment(comment, BookmarkingServiceHelper.GetCurrentInstanse().BookmarkToAdd.Comments);
            ci.IsEditPermissions = false;
            ci.IsResponsePermissions = false;

            var isRoot = string.IsNullOrEmpty(comment.Parent) || comment.Parent.Equals(Guid.Empty.ToString(), StringComparison.CurrentCultureIgnoreCase);

            return CommentsHelper.GetOneCommentHtmlWithContainer(defComment, ci, isRoot, false);
        }

        private string GetOneCommentHtmlWithContainer(Comment comment)
        {
            return CommentsHelper.GetOneCommentHtmlWithContainer(
                Comments,
                BookmarkingConverter.ConvertComment(comment, BookmarkingServiceHelper.GetCurrentInstanse().BookmarkToAdd.Comments),
                comment.Parent.Equals(Guid.Empty.ToString(), StringComparison.CurrentCultureIgnoreCase),
                false);
        }
    }
}