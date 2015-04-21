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
using System.Text;
using ASC.Web.Studio.Utility.HtmlUtility;

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
            CommentInfo comment,
            bool odd,
            Func<string, string> userProfileLink,
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

                if (!string.IsNullOrEmpty(comment.UserPost))
                    sb.AppendFormat("<div style=\"padding-top:2px;padding-left:10px;\" class='describe-text'>{0}</div>", comment.UserPost.HtmlEncode());

                sb.AppendFormat("<div id='content_{0}' style='padding-top:8px;padding-left:10px;' class='longWordsBreak'>", comment.CommentID);
                sb.Append(HtmlUtility.GetFull(comment.CommentBody));
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

                    sb.AppendFormat(
                        "<a class=\"link dotline gray\" id=\"remove_{0}\" href=\"javascript:void(0);\" onclick=\"javascript:AjaxPro.onLoading = function(b){{}}; " +
                        "StudioConfirm.OpenDialog('', function() {{{3}('{0}'," + (String.IsNullOrEmpty(PID) ? "" : "'" + PID + "' ,") + "CommentsManagerObj.callbackRemove)}}); return false;\" >{1}</a>",
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
            var cntr = odd ? 1 : 2;
            return GetOneCommentHtmlWithContainer(
                comment,
                isFirstLevel,
                control.RealUserProfileLinkResolver,
                control.BehaviorID,
                control.EditCommentLink,
                control.ResponseCommentLink,
                control.RemoveCommentLink,
                control.InactiveMessage,
                control.ConfirmRemoveCommentMessage,
                control.JavaScriptRemoveCommentFunctionName,
                control.PID,
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