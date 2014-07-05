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
using System.Text;

namespace ASC.Web.Studio.UserControls.Common.Comments
{
    public delegate string RenderInnerComments(IList<CommentInfo> comments);

    public class CommentsHelper
    {
        public const string EvenClass = "";
        public const string OddClass = "tintMedium";
        public const string EvenStyle = "";
        public const string OddStyle = "border-bottom: 1px solid #DDD;border-top: 1px solid #DDD;";

        public static string GetOneCommentHtml(
            CommentsList control,
            CommentInfo comment,
            bool odd)
        {
            return GetOneCommentHtml(
                comment,
                odd,
                control.RealUserProfileLinkResolver,
                control.Simple,
                control.BehaviorID,
                control.EditCommentLink,
                control.ResponseCommentLink,
                control.RemoveCommentLink,
                control.InactiveMessage,
                control.ConfirmRemoveCommentMessage,
                control.JavaScriptRemoveCommentFunctionName,
                control.PID
                );
        }

        public static string GetOneCommentHtml(
            CommentInfo comment,
            bool odd,
            Func<string, string> userProfileLink,
            bool simple,
            string jsObjName,
            string editCommentLink,
            string responseCommentLink,
            string removeCommentLink,
            string inactiveMessage,
            string confirmRemoveCommentMessage,
            string javaScriptRemoveCommentFunctionName,
            string PID)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='blockComments " + (odd ? OddClass : EvenClass) + "' id=\"comment_" + comment.CommentID + "\" style='" + (odd ? OddStyle : EvenStyle) + " padding: 8px 5px 8px 15px;'><a name=\"" + comment.CommentID + "\"></a>");
            if (comment.Inactive)
            {
                sb.AppendFormat("<div  style=\"padding:10px;\">{0}</div>", inactiveMessage);
            }
            else
            {
                sb.Append("<table width='100%' cellpadding=\"0\" style='table-layout:fixed;' cellspacing=\"0\" border=\"0\" >");
                sb.Append("<tr><td valign=\"top\" width='90'>");
                sb.Append(comment.UserAvatar);
                sb.Append("</td><td valign=\"top\"><div style=\"min-height:55px;\"><div>");
                sb.Append("<a style=\"margin-left:10px;\" class=\"link bold\" href=\"" + userProfileLink(comment.UserID.ToString()) + "\">" + comment.UserFullName + "</a>");
                sb.Append("&nbsp;&nbsp;");
                sb.Append("<span class=\"text-medium-describe\" style='padding-left:5px;'>" + (String.IsNullOrEmpty(comment.TimeStampStr) ? comment.TimeStamp.ToLongDateString() : comment.TimeStampStr) + "</span>");

                sb.Append("</div>");

                //if (!comment.IsRead)
                //{
                //    sb.Append("<td valign=\"top\" style=\"padding-top:2px;\" align=\"right\">&nbsp;&nbsp;&nbsp;<img src=\"" + newImageFile + "\" /></td>");
                //}
                if (!string.IsNullOrEmpty(comment.UserPost))
                    sb.AppendFormat("<div style=\"padding-top:2px;padding-left:10px;\" class='describe-text'>{0}</div>", comment.UserPost.HtmlEncode());

                sb.AppendFormat("<div id='content_{0}' style='padding-top:8px;padding-left:10px;' class='longWordsBreak'>", comment.CommentID);
                sb.Append(comment.CommentBody);
                sb.Append("</div>");

                if (comment.Attachments != null && comment.Attachments.Count > 0)
                {
                    sb.Append("<div id='attacments_" + comment.CommentID + "' style=\"padding-top:10px;padding-left:15px;\">");
                    var k = 0;
                    foreach (var attach in comment.Attachments)
                    {
                        if (k != 0)
                            sb.Append(", ");

                        sb.Append("<a class=\"linkDescribe\" href=\"" + attach.FilePath + "\">" + attach.FileName.HtmlEncode() + "</a>");
                        sb.Append("<input name='attacment_name_" + comment.CommentID + "' type='hidden' value='" + attach.FileName + "' />");
                        sb.Append("<input name='attacment_path_" + comment.CommentID + "' type='hidden' value='" + attach.FilePath + "' />");
                        k++;
                    }
                    sb.Append("</div>");
                }
                sb.Append("</div>");
                sb.Append("<div clas='clearFix' style=\"margin: 10px 0px 0px 10px; height:19px;\" >");

                var drowSplitter = false;

                if (comment.IsResponsePermissions)
                {
                    sb.AppendFormat("<div style='float:left;'><a class=\"link dotline gray\" id=\"response_{0}\" href=\"javascript:void(0);\" onclick=\"javascript:CommentsManagerObj.ResponseToComment(this, '{0}');return false;\" >{1}</a></div>",
                                    comment.CommentID, responseCommentLink);
                }

                sb.Append("<div style='float:right;'>");

                if (comment.IsEditPermissions)
                {
                    sb.AppendFormat("<div style='float:right;'><a class=\"link dotline gray\" id=\"edit_{0}\" href=\"javascript:void(0);\" onclick=\"javascript:CommentsManagerObj.EditComment(this, '{0}');return false;\" >{1}</a>",
                                    comment.CommentID, editCommentLink);
                    drowSplitter = true;
                }

                if (comment.IsEditPermissions)
                {
                    if (drowSplitter) sb.Append("<span class=\"text-medium-describe  splitter\"> </span>");

                    sb.AppendFormat("<a class=\"link dotline gray\" id=\"remove_{0}\" href=\"javascript:void(0);\" onclick=\"javascript:if(window.confirm('{2}')){{AjaxPro.onLoading = function(b){{}}; {3}('{0}'," + (String.IsNullOrEmpty(PID) ? "" : "'" + PID + "' ,") + "CommentsManagerObj.callbackRemove);}}return false;\" >{1}</a>",
                                    comment.CommentID, removeCommentLink, confirmRemoveCommentMessage, javaScriptRemoveCommentFunctionName);
                }
                sb.Append("</div>");

                sb.Append("</div>");
                sb.Append("</td></tr></table>");
            }
            sb.Append("</div>");

            return sb.ToString();
        }

        public static string GetOneCommentHtmlWithContainer(
            CommentsList control,
            CommentInfo comment,
            bool isFirstLevel,
            bool odd)
        {
            return GetOneCommentHtmlWithContainer(
                comment,
                isFirstLevel,
                odd,
                control.RealUserProfileLinkResolver,
                control.Simple,
                control.BehaviorID,
                control.EditCommentLink,
                control.ResponseCommentLink,
                control.RemoveCommentLink,
                control.InactiveMessage,
                control.ConfirmRemoveCommentMessage,
                control.JavaScriptRemoveCommentFunctionName,
                control.PID
                );
        }

        public static string GetOneCommentHtmlWithContainer(
            CommentInfo comment,
            bool isFirstLevel,
            bool odd,
            Func<string, string> userProfileLink,
            bool simple,
            string jsObjName,
            string editCommentLink,
            string responseCommentLink,
            string removeCommentLink,
            string inactiveMessage,
            string confirmRemoveCommentMessage,
            string javaScriptRemoveCommentFunctionName,
            string PID
            )
        {
            var cntr = odd ? 1 : 2;
            return GetOneCommentHtmlWithContainer(
                comment,
                isFirstLevel,
                userProfileLink,
                simple,
                jsObjName,
                editCommentLink,
                responseCommentLink,
                removeCommentLink,
                inactiveMessage,
                confirmRemoveCommentMessage,
                javaScriptRemoveCommentFunctionName,
                PID,
                null,
                ref cntr
                );
        }

        public static string GetOneCommentHtmlWithContainer(
            CommentsList control,
            CommentInfo comment,
            bool isFirstLevel,
            RenderInnerComments renderFunction,
            ref int commentIndex)
        {
            return GetOneCommentHtmlWithContainer(
                comment,
                isFirstLevel,
                control.RealUserProfileLinkResolver,
                control.Simple,
                control.BehaviorID,
                control.EditCommentLink,
                control.ResponseCommentLink,
                control.RemoveCommentLink,
                control.InactiveMessage,
                control.ConfirmRemoveCommentMessage,
                control.JavaScriptRemoveCommentFunctionName,
                control.PID,
                renderFunction,
                ref commentIndex
                );
        }

        public static string GetOneCommentHtmlWithContainer(
            CommentInfo comment,
            bool isFirstLevel,
            Func<string, string> userProfileLink,
            bool simple,
            string jsObjName,
            string editCommentLink,
            string responseCommentLink,
            string removeCommentLink,
            string inactiveMessage,
            string confirmRemoveCommentMessage,
            string javaScriptRemoveCommentFunctionName,
            string PID,
            RenderInnerComments renderFunction,
            ref int commentIndex
            )
        {
            if (comment.Inactive && IsEmptyComments(comment.CommentList))
                return String.Empty;

            var sb = new StringBuilder();

            sb.AppendFormat("<div style=\"{1}\" id=\"container_{0}\">",
                            comment.CommentID, (!isFirstLevel ? "margin-left: 35px;" : String.Empty));

            sb.Append(
                GetOneCommentHtml(
                    comment,
                    commentIndex%2 == 1,
                    userProfileLink,
                    simple,
                    jsObjName,
                    editCommentLink,
                    responseCommentLink,
                    removeCommentLink,
                    inactiveMessage,
                    confirmRemoveCommentMessage,
                    javaScriptRemoveCommentFunctionName,
                    PID
                    )
                );

            commentIndex++;

            if (renderFunction != null && comment.CommentList != null && comment.CommentList.Count > 0)
                sb.Append(renderFunction(comment.CommentList));

            sb.Append("</div>");

            return sb.ToString();
        }

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
    }
}