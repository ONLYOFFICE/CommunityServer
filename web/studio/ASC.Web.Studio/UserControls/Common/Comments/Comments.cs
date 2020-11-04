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
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Common.Comments
{
    [ToolboxData("<{0}:CommentsList runat=server></{0}:CommentsList>")]
    public sealed class CommentsList : WebControl
    {
        public delegate string FCKBasePathRequestHandler();

        public CommentsList()
        {
        }

        #region Members

        private static bool _isClientScriptRegistered;
        private int _maxDepthLevel = 8;

        private string JsObjName
        {
            get { return String.IsNullOrEmpty(BehaviorID) ? "__comments" + UniqueID : BehaviorID; }
        }

        private IList<CommentInfo> _items = new List<CommentInfo>(0);

        private string _objectID = string.Empty;
        private bool _isShowAddCommentBtn = true;
        private bool _showCaption = true;

        #endregion

        #region Properties

        public string FckDomainName { get; set; }

        public string OnEditedCommentJS { get; set; }
        public string OnRemovedCommentJS { get; set; }
        public string OnCanceledCommentJS { get; set; }

        public int MaxDepthLevel
        {
            get { return _maxDepthLevel; }
            set { _maxDepthLevel = value; }
        }

        public bool IsShowAddCommentBtn
        {
            get { return _isShowAddCommentBtn; }
            set { _isShowAddCommentBtn = value; }
        }

        public bool ShowCaption
        {
            get { return _showCaption; }
            set { _showCaption = value; }
        }

        public string BehaviorID { get; set; }

        public IList<CommentInfo> Items
        {
            get { return _items; }
            set { _items = value; }
        }
        
        public string ModuleName { get; set; }

        public string JavaScriptCallBackAddComment { get; set; }

        public string ConfirmRemoveCommentMessage { get; set; }

        public string ObjectID
        {
            get { return _objectID; }
            set { _objectID = value; }
        }

        public int TotalCount { get; set; }

        public bool? InitJS { get; set; }

        #endregion

        #region Methods

        public static bool IsActiveOrHasActiveComments(CommentInfo item)
        {
            if (item == null)
                return false;

            if (!item.Inactive)
                return true;

            //then if Inactive == true
            if (item.CommentList == null || item.CommentList.Count == 0)
                return false;

            foreach (var c in item.CommentList)
            {
                if (IsActiveOrHasActiveComments(c))
                {
                    return true;
                }
            }
            return false;
        }


        public static List<CommentInfo> CleanInactive(List<CommentInfo> items)
        {
            if (items == null)
                return null;

            for (var i = 0; i < items.Count; i++)
            {
                if (!IsActiveOrHasActiveComments(items[i]))
                {
                    items.RemoveAt(i);
                    i--;
                }
                else
                {
                    items[i].CommentList = CleanInactive(items[i].CommentList.ToList());
                }
            }
            return items;
        }

        private void RegisterClientScripts()
        {
            Page
                .RegisterBodyScripts("~/UserControls/Common/ckeditor/ckeditor-connector.js",
                    "~/js/uploader/ajaxupload.js",
                    "~/UserControls/Common/Comments/js/comments.js",
                    "~/js/third-party/highlight.pack.js")
                .RegisterInlineScript("hljs.initHighlightingOnLoad();")
                .RegisterStyle("~/UserControls/Common/Comments/css/codehighlighter/vs.less");


            var uploadPath = string.Format("{0}://{1}:{2}{3}", Page.Request.GetUrlRewriter().Scheme, Page.Request.GetUrlRewriter().Host, Page.Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true");
            uploadPath += "&esid=" + FckDomainName;

            Items = CleanInactive(Items.ToList());

            var paramsScript = string.Format(@"
                        CommentsManagerObj.javaScriptCallBackAddComment = '{0}';
                        CommentsManagerObj.moduleName = '{1}';
                        ",
                                            JavaScriptCallBackAddComment,
                                            ModuleName);

            paramsScript += string.Format(@"
                    CommentsManagerObj._jsObjName = '{0}';
                    CommentsManagerObj.CkUploadHandlerPath = '{1}';
                    CommentsManagerObj.maxLevel = {2};
                    CommentsManagerObj.comments = '{3}';
                    CommentsManagerObj.isEmpty = {4};
                    CommentsManagerObj.isShowAddCommentBtn = {5};
                    CommentsManagerObj.objectID = '{6}',
                    CommentsManagerObj.showCaption = {7};
                    CommentsManagerObj.maxDepthLevel = {8};
                    CommentsManagerObj.total = {9};
                    ",
                                             JsObjName,
                                             uploadPath,
                                             MaxDepthLevel,
                                              Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Items))),
                                             IsEmptyComments(_items).ToString().ToLower(),
                                             _isShowAddCommentBtn.ToString().ToLower(),
                                             _objectID,
                                             _showCaption.ToString().ToLower(),
                                             _maxDepthLevel,
                                             TotalCount);

            paramsScript += string.Format(@"
                    CommentsManagerObj.OnEditedCommentJS = '{0}';
                    CommentsManagerObj.OnRemovedCommentJS = '{1}';
                    CommentsManagerObj.OnCanceledCommentJS = '{2}';
                    CommentsManagerObj.FckDomainName = '{3}';",
                                          OnEditedCommentJS,
                                          OnRemovedCommentJS,
                                          OnCanceledCommentJS,
                                          FckDomainName);


            if (!InitJS.HasValue || InitJS.Value)
            {
                paramsScript += string.Format("CommentsManagerObj.Init();");
            }

            Page.RegisterInlineScript(paramsScript);

        }

        #endregion

        #region Events

        private Confirm confirm;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Visible)
            {
                RegisterClientScripts();
                _isClientScriptRegistered = true;
            }

            confirm = (Confirm)Page.LoadControl(Confirm.Location);
            confirm.Title = Resource.ConfirmRemoveCommentTitle;
            confirm.SelectTitle = ConfirmRemoveCommentMessage;
            Controls.Add(confirm);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (!_isClientScriptRegistered)
                RegisterClientScripts();

            writer.Write("<div id=\"commentsTempContainer_" + _objectID + "\"></div>");

            confirm.RenderControl(writer);
        }

        #endregion

        #region Methods

        public static bool IsEmptyComments(IList<CommentInfo> comments)
        {
            if (comments == null)
                return true;

            foreach (var c in comments)
            {
                if (!c.Inactive)
                {
                    return false;
                }
                if (c.CommentList != null && !IsEmptyComments(c.CommentList))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}