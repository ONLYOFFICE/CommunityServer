/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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


using ASC.Notify.Model;
using System;
using System.Web;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Blogs.Core
{
    public sealed class Constants
    {
        public static readonly Guid ModuleId = new Guid("{6A598C74-91AE-437d-A5F4-AD339BD11BB2}");

        public static readonly Action Action_AddPost = new Action(new Guid("{948AD738-434B-4a88-8E38-7569D332910A}"), "Add post");
        public static readonly Action Action_EditRemovePost = new Action(new Guid("{00E7DFC5-AC49-4fd3-A1D6-98D84E877AC4}"), "Edit post");
        public static readonly Action Action_AddComment = new Action(new Guid("{388C29D3-C662-4a61-BF47-FC2F7094224A}"), "Add comment");
        public static readonly Action Action_EditRemoveComment = new Action(new Guid("{724CBB75-D1C9-451e-BAE0-4DE0DB96B1F7}"), "Edit comment");

        public static INotifyAction NewPost = new NotifyAction("new post");
        public static INotifyAction NewPostByAuthor = new NotifyAction("new personal post");
        public static INotifyAction NewComment = new NotifyAction("new comment");

        public const string _NewBlogSubscribeCategory = "{9C8ED95F-07D2-42d0-B241-C0A51F7D26D5}";

        public static string TagPostSubject = "PostSubject";
        public static string TagURL = "URL";
        public static string TagUserName = "UserName";
        public static string TagUserURL = "UserURL";
        public static string TagDate = "Date";
        public static string TagPostPreview = "PostPreview";
        public static string TagCommentBody = "CommentBody";
        public static string TagCommentURL = "CommentURL";

        public static Guid ModuleID = ASC.Blogs.Core.BlogsSettings.ModuleID;
        public const int MAX_TEXT_LENGTH = 255;

        #region Virtual Path

        public const string BaseVirtualPath = "~/products/community/modules/blogs/";
        public static string GetModuleAbsolutePath(string virualPath)
        {
            if (virualPath == "" || virualPath == "/")
                virualPath = "./";

            return VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(BaseVirtualPath, virualPath));
        }

        public static string DefaultPageUrl { get { return GetModuleAbsolutePath(""); } }
        public static string ViewBlogPageUrl { get { return GetModuleAbsolutePath("viewblog.aspx"); } }
        public static string UserPostsPageUrl { get { return GetModuleAbsolutePath("/") + "?userid="; } }

        #endregion
    }
}