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
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Core;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.UserControls.Bookmarking.Common.Util;
using Newtonsoft.Json;

namespace ASC.Web.UserControls.Bookmarking
{
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
            set
            { 
                _serviceHelper.Comments = CommentList;
                BookmarkingServiceHelper.UpdateCurrentInstanse(_serviceHelper);
            }
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

            commentList.BehaviorID = "commentsObj";
            commentList.ModuleName = "bookmarks";
            commentList.FckDomainName = "bookmarking_comments";
            commentList.TotalCount = BookmarkComments.Count;
            commentList.ShowCaption = false;
            commentList.ObjectID = BookmarkID.ToString();
        }
      
    }
}