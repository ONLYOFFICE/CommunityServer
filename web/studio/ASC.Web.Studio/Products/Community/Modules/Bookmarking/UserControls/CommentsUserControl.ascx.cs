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