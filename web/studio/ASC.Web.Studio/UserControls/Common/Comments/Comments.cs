/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
            var codeHighlighter = new CodeHighlighter();
            Controls.Add(codeHighlighter);
        }

        #region Members

        private static bool _isClientScriptRegistered;
        private int _maxDepthLevel = 8;
        private int _commentIndex;

        private string JsObjName
        {
            get { return String.IsNullOrEmpty(BehaviorID) ? "__comments" + UniqueID : BehaviorID; }
        }

        private IList<CommentInfo> _items = new List<CommentInfo>(0);
        private string _inactiveMessage = string.Empty;
        private string _editCommentToolTip = string.Empty;
        private string _responseCommentToolTip = string.Empty;
        private string _removeCommentToolTip = string.Empty;

        private string _editCommentLink = "Edit";
        private string _removeCommentLink = "Remove";
        private string _responseCommentLink = "Answer";

        private string _previewButton = string.Empty;
        private string _saveButton = string.Empty;
        private string _cancelButton = string.Empty;
        private string _hidePrevuewButton = string.Empty;
        private string _addCommentLink = string.Empty;
        private string _cancelCommentLink = string.Empty;
        private string _commentsTitle = string.Empty;
        private string _commentsCountTitle = string.Empty;
        private string _javaScriptRemoveCommentFunctionName = string.Empty;
        private string _javaScriptPreviewCommentFunctionName = string.Empty;
        private string _javaScriptAddCommentFunctionName = string.Empty;
        private string _javaScriptUpdateCommentFunctionName = string.Empty;
        private string _javaScriptLoadBBcodeCommentFunctionName = string.Empty;
        private string _javaScriptCallBackAddComment = string.Empty;
        private string _objectID = string.Empty;
        private bool _isShowAddCommentBtn = true;
        private string _confirmRemoveCommentMessage = string.Empty;
        private bool _showCaption = true;
        private bool _simple;

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

        public string AttachButton { get; set; }

        public string RemoveAttachButton { get; set; }

        public bool EnableAttachmets { get; set; }

        public string HandlerTypeName { get; set; }

        public string AdditionalSubmitText { get; set; }

        public string PID { get; set; }

        public string EditCommentLink
        {
            get { return _editCommentLink; }
            set { _editCommentLink = value; }
        }

        public string RemoveCommentLink
        {
            get { return _removeCommentLink; }
            set { _removeCommentLink = value; }
        }

        public string ResponseCommentLink
        {
            get { return _responseCommentLink; }
            set { _responseCommentLink = value; }
        }

        public bool Simple
        {
            get { return _simple; }
            set { _simple = value; }
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

        public Func<string, string> UserProfileUrlResolver { get; set; }

        public string InactiveMessage
        {
            get { return _inactiveMessage; }
            set { _inactiveMessage = value; }
        }

        public string EditCommentToolTip
        {
            get { return _editCommentToolTip; }
            set { _editCommentToolTip = value; }
        }

        public string ResponseCommentToolTip
        {
            get { return _responseCommentToolTip; }
            set { _responseCommentToolTip = value; }
        }

        public string RemoveCommentToolTip
        {
            get { return _removeCommentToolTip; }
            set { _removeCommentToolTip = value; }
        }

        public string PreviewButton
        {
            get { return _previewButton; }
            set { _previewButton = value; }
        }

        public string SaveButton
        {
            get { return _saveButton; }
            set { _saveButton = value; }
        }

        public string CancelButton
        {
            get { return _cancelButton; }
            set { _cancelButton = value; }
        }

        public string HidePrevuewButton
        {
            get { return _hidePrevuewButton; }
            set { _hidePrevuewButton = value; }
        }

        public string AddCommentLink
        {
            get { return _addCommentLink; }
            set { _addCommentLink = value; }
        }

        public string CancelCommentLink
        {
            get { return _cancelCommentLink; }
            set { _cancelCommentLink = value; }
        }

        public string CommentsTitle
        {
            get { return _commentsTitle; }
            set { _commentsTitle = value; }
        }

        public string CommentsCountTitle
        {
            get { return _commentsCountTitle; }
            set { _commentsCountTitle = value; }
        }

        public string JavaScriptRemoveCommentFunctionName
        {
            get { return _javaScriptRemoveCommentFunctionName; }
            set { _javaScriptRemoveCommentFunctionName = value; }
        }

        public string JavaScriptPreviewCommentFunctionName
        {
            get { return _javaScriptPreviewCommentFunctionName; }
            set { _javaScriptPreviewCommentFunctionName = value; }
        }

        public string JavaScriptAddCommentFunctionName
        {
            get { return _javaScriptAddCommentFunctionName; }
            set { _javaScriptAddCommentFunctionName = value; }
        }

        public string JavaScriptUpdateCommentFunctionName
        {
            get { return _javaScriptUpdateCommentFunctionName; }
            set { _javaScriptUpdateCommentFunctionName = value; }
        }

        public string JavaScriptLoadBBcodeCommentFunctionName
        {
            get { return _javaScriptLoadBBcodeCommentFunctionName; }
            set { _javaScriptLoadBBcodeCommentFunctionName = value; }
        }

        public string JavaScriptCallBackAddComment
        {
            get { return _javaScriptCallBackAddComment; }
            set { _javaScriptCallBackAddComment = value; }
        }

        public string ConfirmRemoveCommentMessage
        {
            get { return _confirmRemoveCommentMessage; }
            set { _confirmRemoveCommentMessage = value; }
        }

        public string ObjectID
        {
            get { return _objectID; }
            set { _objectID = value; }
        }

        public int TotalCount { get; set; }

        #endregion

        #region Methods

        internal string RealUserProfileLinkResolver(string userID)
        {
            return UserProfileUrlResolver != null
                       ? UserProfileUrlResolver(userID)
                       : CommonLinkUtility.GetUserProfile(userID);
        }

        private void RegisterClientScripts()
        {
            Page.RegisterBodyScripts(VirtualPathUtility.ToAbsolute("~/usercontrols/common/ckeditor/ckeditor-connector.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/js/uploader/ajaxupload.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/comments/js/comments.js"));

            var uploadPath = string.Format("{0}://{1}:{2}{3}", Page.Request.GetUrlRewriter().Scheme, Page.Request.GetUrlRewriter().Host, Page.Request.GetUrlRewriter().Port, VirtualPathUtility.ToAbsolute("~/") + "fckuploader.ashx?newEditor=true");
            uploadPath += "&esid=" + FckDomainName;

            var paramsScript = string.Format(@"
                    CommentsManagerObj.javaScriptAddCommentFunctionName = '{0}';
                    CommentsManagerObj.javaScriptLoadBBcodeCommentFunctionName = '{1}';
                    CommentsManagerObj.javaScriptUpdateCommentFunctionName = '{2}';
                    CommentsManagerObj.javaScriptCallBackAddComment = '{3}';
                    CommentsManagerObj.javaScriptPreviewCommentFunctionName = '{4}';
                    CommentsManagerObj.isSimple = {5};                    
                    CommentsManagerObj._jsObjName = '{6}';
                    CommentsManagerObj.PID = '{7}';
                    CommentsManagerObj.inactiveMessage = '{8}';
                    CommentsManagerObj.EnableAttachmets = {9};
                    CommentsManagerObj.RemoveAttachButton = '{10}';
                    CommentsManagerObj.CkUploadHandlerPath = '{11}';
                    CommentsManagerObj.maxLevel = {12};
                    ",
                                             _javaScriptAddCommentFunctionName, _javaScriptLoadBBcodeCommentFunctionName,
                                             _javaScriptUpdateCommentFunctionName, _javaScriptCallBackAddComment,
                                             _javaScriptPreviewCommentFunctionName, _simple.ToString().ToLower(), JsObjName,
                                             PID, _inactiveMessage, EnableAttachmets.ToString().ToLower(),
                                             RemoveAttachButton, uploadPath, MaxDepthLevel);

            paramsScript += string.Format(@"
                    CommentsManagerObj.OnEditedCommentJS = '{0}';
                    CommentsManagerObj.OnRemovedCommentJS = '{1}';
                    CommentsManagerObj.OnCanceledCommentJS = '{2}';
                    CommentsManagerObj.FckDomainName = '{3}';",
                                          OnEditedCommentJS, OnRemovedCommentJS, OnCanceledCommentJS, FckDomainName);

            paramsScript +=
                "\n" +
                "if(jq('#comments_Uploader').length>0 && '" + HandlerTypeName + "' != '')\n" +
                "{\n" +
                "new AjaxUpload('comments_Uploader', {\n" +
                "action: 'ajaxupload.ashx?type=" + HandlerTypeName + "',\n" +
                "onComplete: CommentsManagerObj.UploadCallBack\n" +
                "});\n}\n";

            if (!Simple)
            {
                paramsScript += string.Format("CommentsManagerObj.InitEditor();");
            }

            Page.RegisterInlineScript(paramsScript);

            if (Simple)
            {
                Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/comments/js/onReady.js"));
            }
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
            confirm.SelectJSCallback = JavaScriptRemoveCommentFunctionName;
            Controls.Add(confirm);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (!_isClientScriptRegistered)
                RegisterClientScripts();

            var sb = new StringBuilder();

            var visibleCommentsCount = TotalCount;

            var isEmpty = CommentsHelper.IsEmptyComments(_items);

            if (_showCaption)
            {
                sb.Append("<div id='commentsTitle' style=\"margin-left:5px;\" class=\"headerPanel\" >" + _commentsTitle + "</div>");
            }

            sb.Append("<a name=\"comments\"></a>");

            sb.Append("<div id=\"noComments\" style=\"" + (!isEmpty ? "display:none;" : "") + "\">" + UserControlsCommonResource.NoComments + "</div>");

            sb.Append("<div id=\"mainContainer\" style='width:100%; margin-top:5px; " + (visibleCommentsCount % 2 == 0 ? "border-bottom:1px solid #ddd;" : "") + "word-wrap: break-word;" + (isEmpty ? "display:none;" : "") + "'>");
            sb.Append(RenderComments() + "</div>");
            sb.Append("<br />");

            if (_isShowAddCommentBtn)
            {
                sb.Append("<a id=\"add_comment_btn\" onclick=\"javascript:CommentsManagerObj.AddNewComment();\">" + _addCommentLink + "</a>");
            }
            sb.Append("<div id=\"commentBox\" style=\"margin-top: 5px; display:none;\">");
            sb.Append("<div id=\"commentBoxContainer\">");
            sb.Append("<input type=\"hidden\" id=\"hdnParentComment\" value=\"\" />");
            sb.Append("<input type=\"hidden\" id=\"hdnAction\" value=\"\" />");
            sb.Append("<input type=\"hidden\" id=\"hdnCommentID\" value=\"\" />");
            sb.Append("<input type=\"hidden\" id=\"hdnObjectID\" value=\"" + _objectID + "\" />");

            sb.Append("<textarea id='commentEditor' name='commentEditor'></textarea>");

            sb.Append("<a name='add_comment'></a>");
            sb.Append("<div id=\"CommentsFckEditorPlaceHolder_" + JsObjName + "\">");

            if (Simple)
                sb.Append("<textarea id='simpleTextArea' name='simpleTextArea' style='width: 100%; height:124px;'></textarea>");

            sb.Append("</div>");
            sb.Append("<div id=\"comment_attachments\" style=\"padding:5px;\">");
            sb.Append("</div>");
            sb.Append("<input id=\"hdn_comment_attachments\" type=\"hidden\" value=\"\" />");
            sb.Append("<div class=\"middle-button-container\" >");
            sb.Append("<a href=\"javascript:void(0);\"  id=\"btnAddComment\" class=\"button\" onclick=\"javascript:CommentsManagerObj.AddComment_Click();return false;\">" + _saveButton + "</a><span class=\"splitter-buttons\"></span>");

            if (EnableAttachmets)
            {
                sb.Append("<a href=\"javascript:void(0);\" id=\"comments_Uploader\" class=\"button\">" + AttachButton + "</a><span class=\"splitter-buttons\"></span>");
            }

            sb.AppendFormat("<a href='javascript:void(0);' id='btnPreview' class='button disable' onclick='javascript:CommentsManagerObj.Preview_Click();return false;'>{0}</a><span class=\"splitter-buttons\"></span>", _previewButton);
            sb.AppendFormat("<a href='javascript:void(0);' id='btnCancel' class='button gray cancelFckEditorChangesButtonMarker' name='{1}' onclick='CommentsManagerObj.Cancel();' />{0}</a>", _cancelButton, "CommentsFckEditor_" + JsObjName);

            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("<div id=\"previewBox\" style=\"display: none; margin-top:20px;\">");
            sb.Append("<div class='headerPanel' style=\"margin-top: 0px;\">" + _previewButton + "</div>");
            sb.Append("<div id=\"previewBoxBody\"></div>");
            sb.Append("<div class=\"middle-button-container\">");
            sb.Append("<a href=\"javascript:void(0);\"  onclick=\"CommentsManagerObj.HidePreview(); return false;\" class=\"button blue\" style=\"margin-right:8px;\">" + _hidePrevuewButton + "</a>");
            sb.Append("</div>");

            sb.Append("</div>");
            sb.Append("</div>");

            writer.Write(sb.ToString());

            confirm.RenderControl(writer);
        }

        #endregion

        #region Methods

        private string RenderComments()
        {
            _commentIndex = 1;

            return RenderComments(_items, 1);
        }

        private string RenderComments(IList<CommentInfo> comments, int commentLevel)
        {
            var sb = new StringBuilder();
            if (comments != null && comments.Count > 0)
            {
                foreach (var comment in comments)
                {
                    sb.Append(
                        CommentsHelper.GetOneCommentHtmlWithContainer(
                            this,
                            comment,
                            commentLevel == 1 || commentLevel > _maxDepthLevel,
                            cmnts => RenderComments(cmnts, commentLevel + 1),
                            ref _commentIndex
                            )
                        );
                }
            }
            return sb.ToString();
        }

        #endregion
    }
}