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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Wiki.Wrappers;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Community.Product;
using ASC.Web.Community.Wiki.Common;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Wiki;
using ASC.Web.UserControls.Wiki.Data;
using File = ASC.Web.UserControls.Wiki.Data.File;

namespace ASC.Api.Community
{
    public partial class CommunityApi
    {
        private readonly WikiEngine _engine = new WikiEngine();

        /// <summary>
        /// Creates a new wiki page with the page name and content specified in the request
        /// </summary>
        /// <short>Create page</short>
        /// <param name="name">Page name</param>
        /// <param name="body">Page content</param>
        /// <returns>page info</returns>
        /// <category>Wiki</category>
        [Create("wiki")]
        public PageWrapper CreatePage(string name, string body)
        {
            return new PageWrapper(_engine.CreatePage(new Page { PageName = name, Body = body }));
        }

        /// <summary>
        /// Returns the list of all pages in wiki or pages in wiki category specified in the request
        /// </summary>
        /// <short>Pages</short>
        /// <section>Pages</section>
        /// <param name="category">Category name</param>
        /// <returns></returns>
        /// <category>Wiki</category>
        [Read("wiki")]
        public IEnumerable<PageWrapper> GetPages(string category)
        {
            return category == null
                ? _engine.GetPages().ConvertAll(x => new PageWrapper(x))
                : _engine.GetPages(category).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Return the detailed information about the wiki page with the name and version specified in the request
        /// </summary>
        /// <short>Page</short>
        /// <section>Pages</section>
        /// <param name="name">Page name</param>
        /// <param name="version">Page version</param>
        /// <returns>Page info</returns>
        /// <category>Wiki</category>
        [Read("wiki/{name}")]
        public PageWrapper GetPage(string name, int? version)
        {
            if(string.IsNullOrEmpty(name)) throw new ArgumentException();

            var page = version != null ? _engine.GetPage(name, (int)version) : _engine.GetPage(name);

            if (page == null) throw new Exception("wiki page not found");

            return new PageWrapper(page);
        }

        /// <summary>
        /// Returns the list of history changes for the wiki page with the name specified in the request
        /// </summary>
        /// <short>History</short>
        /// <section>Pages</section>
        /// <param name="page">Page name</param>
        /// <returns>List of pages</returns>
        /// <category>Wiki</category>
        [Read("wiki/{page}/story")]
        public IEnumerable<PageWrapper> GetHistory(string page)
        {
            return _engine.GetPageHistory(page).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Returns the list of wiki pages with the name matching the search query specified in the request
        /// </summary>
        /// <short>Search</short>
        /// <section>Pages</section>
        /// <param name="name">Part of the page name</param>
        /// <returns>List of pages</returns>
        /// <category>Wiki</category>
        [Read("wiki/search/byname/{name}")]
        public IEnumerable<PageWrapper> SearchPages(string name)
        {
            return _engine.SearchPagesByName(name).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Returns the list of wiki pages with the content matching the search query specified in the request
        /// </summary>
        /// <short>Search</short>
        /// <section>Pages</section>
        /// <param name="content">Part of the page content</param>
        /// <returns>List of pages</returns>
        /// <category>Wiki</category>
        [Read("wiki/search/bycontent/{content}")]
        public IEnumerable<PageWrapper> SearchPagesByContent(string content)
        {
            return _engine.SearchPagesByContent(content).ConvertAll(x => new PageWrapper(x));
        }

        /// <summary>
        /// Updates the wiki page with the name and content specified in the request
        /// </summary>
        /// <short>Update page</short>
        /// <section>Pages</section>
        /// <param name="name">Page name</param>
        /// <param name="body">Page content</param>
        /// <returns>Page info</returns>
        /// <category>Wiki</category>
        [Update("wiki/{name}")]
        public PageWrapper UpdatePage(string name, string body)
        {
            return new PageWrapper(_engine.UpdatePage(new Page { PageName = name, Body = body }));
        }

        /// <summary>
        /// Deletes the wiki page with the name specified in the request
        /// </summary>
        /// <short>Delete page</short>
        /// <section>Pages</section>
        /// <param name="name">Page name</param>
        /// <category>Wiki</category>
        [Delete("wiki/{name}")]
        public void DeletePage(string name)
        {
            _engine.RemovePage(name);
        }

        /// <summary>
        /// Creates the comment to the selected wiki page with the content specified in the request
        /// </summary>
        /// <short>Create comment</short>
        /// <section>Comments</section>
        /// <param name="page">Page name</param>
        /// <param name="content">Comment content</param>
        /// <param name="parentId">Comment parent id</param>
        /// <returns>Comment info</returns>
        /// <category>Wiki</category>
        [Create("wiki/{page}/comment")]
        public CommentWrapper CreateComment(string page, string content, string parentId)
        {
            var parentIdGuid = String.IsNullOrEmpty(parentId) ? Guid.Empty : new Guid(parentId);
            return new CommentWrapper(_engine.CreateComment(new Comment { PageName = page, Body = content, ParentId = parentIdGuid }));
        }

        /// <summary>
        /// Returns the list of all comments to the wiki page with the name specified in the request
        /// </summary>
        /// <short>All comments</short>
        /// <section>Comments</section>
        /// <param name="page">Page name</param>
        /// <returns>List of comments</returns>
        /// <category>Wiki</category>
        [Read("wiki/{page}/comment")]
        public List<CommentWrapper> GetComments(string page)
        {
            return _engine.GetComments(page).ConvertAll(x => new CommentWrapper(x));
        }

        /// <summary>
        /// Uploads the selected files to the wiki 'Files' section
        /// </summary>
        /// <short>Upload files</short>
        /// <param name="files">List of files to upload</param>
        /// <returns>List of files</returns>
        /// <category>Wiki</category>
        [Create("wiki/file")]
        public IEnumerable<FileWrapper> UploadFiles(IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) throw new ArgumentNullException("files");

            return files.Select(file => new FileWrapper(_engine.CreateOrUpdateFile(new File { FileName = file.FileName, FileSize = file.ContentLength, UploadFileName = file.FileName }, file.InputStream)));
        }

        /// <summary>
        /// Returns the detailed file info about the file with the specified name in the wiki 'Files' section
        /// </summary>
        /// <short>File</short>
        /// <section>Files</section>
        /// <param name="name">File name</param>
        /// <returns>File info</returns>
        /// <category>Wiki</category>
        [Read("wiki/file/{name}")]
        public FileWrapper GetFile(string name)
        {
            return new FileWrapper(_engine.GetFile(name));
        }

        /// <summary>
        /// Deletes the files with the specified name from the wiki 'Files' section
        /// </summary>
        /// <short>Delete file</short>
        /// <param name="name">File name</param>
        /// <category>Wiki</category>
        [Delete("wiki/file/{name}")]
        public void DeleteFile(string name)
        {
            _engine.RemoveFile(name);
        }

        /// <summary>
        /// Get comment preview with the content specified in the request
        /// </summary>
        /// <short>Get comment preview</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <param name="htmltext">Comment content</param>
        /// <returns>Comment info</returns>
        /// <category>Wiki</category>
        [Create("wiki/comment/preview")]
        public CommentInfo GetWikiCommentPreview(string commentid, string htmltext)
        {
            var comment = !string.IsNullOrEmpty(commentid) ? _engine.GetComment(new Guid(commentid)) : new Comment();
            comment.Date = TenantUtil.DateTimeNow();
            comment.UserId = SecurityContext.CurrentAccount.ID;
            comment.Body = htmltext;

            var info = GetCommentInfo(comment);
            info.IsEditPermissions = false;
            info.IsResponsePermissions = false;
            return info;
        }

        /// <summary>
        ///Remove comment with the id specified in the request
        /// </summary>
        /// <short>Remove comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <returns>Comment info</returns>
        /// <category>Wiki</category>
        [Delete("wiki/comment/{commentid}")]
        public string RemoveWikiComment(string commentid)
        {
            _engine.DeleteComment(new Guid(commentid));
            return commentid;
        }



        /// <category>Wiki</category>
        [Create("wiki/comment")]
        public CommentInfo AddWikiComment(string parentcommentid, string entityid, string content)
        {
            CommunitySecurity.DemandPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddComment);


            var parentIdGuid = String.IsNullOrEmpty(parentcommentid) ? Guid.Empty : new Guid(parentcommentid);
            var newComment = _engine.CreateComment(new Comment { Body = content, PageName = entityid, ParentId = parentIdGuid });

            return GetCommentInfo(newComment);
        }

        /// <summary>
        /// Updates the comment to the selected wiki page with the comment content specified in the request
        /// </summary>
        /// <short>Update comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <param name="content">Comment content</param>
        /// <returns></returns>
        /// <category>Wiki</category>
        [Update("wiki/comment/{commentid}")]
        public string UpdateWikiComment(string commentid, string content)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException();

            _engine.UpdateComment(new Comment { Id = new Guid(commentid), Body = content });
            return HtmlUtility.GetFull(content);
        }

        private static CommentInfo GetCommentInfo(Comment comment)
        {
            var info = new CommentInfo
            {
                CommentID = comment.Id.ToString(),
                UserID = comment.UserId,
                TimeStamp = comment.Date,
                TimeStampStr = comment.Date.Ago(),
                IsRead = true,
                Inactive = comment.Inactive,
                CommentBody = HtmlUtility.GetFull(comment.Body),
                UserFullName = DisplayUserSettings.GetFullUserName(comment.UserId),
                UserProfileLink = CommonLinkUtility.GetUserProfile(comment.UserId),
                UserAvatarPath = UserPhotoManager.GetBigPhotoURL(comment.UserId),
                IsEditPermissions = CommunitySecurity.CheckPermissions(new WikiObjectsSecurityObject(comment), ASC.Web.Community.Wiki.Common.Constants.Action_EditRemoveComment),
                IsResponsePermissions = CommunitySecurity.CheckPermissions(ASC.Web.Community.Wiki.Common.Constants.Action_AddComment),
                UserPost = CoreContext.UserManager.GetUsers(comment.UserId).Title
            };

            return info;
        }




    }
}
