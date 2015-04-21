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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Utils;
using ASC.Api.Wiki.Wrappers;
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
            return version != null ? new PageWrapper(_engine.GetPage(name, (int)version)) : new PageWrapper(_engine.GetPage(name));
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
            return new PageWrapper(_engine.SavePage(new Page { PageName = name, Body = body }));
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
        /// Updates the comment to the selected wiki page with the comment content specified in the request
        /// </summary>
        /// <short>Update comment</short>
        /// <section>Comments</section>
        /// <param name="id">Comment ID</param>
        /// <param name="body">Comment content</param>
        /// <returns>Comment info</returns>
        /// <category>Wiki</category>
        [Update("wiki/comment/{id}")]
        public CommentWrapper UpdateComment(string id, string body)
        {
            return new CommentWrapper(_engine.UpdateComment(new Comment { Id = new Guid(id), Body = body }));
        }

        /// <summary>
        /// Deletes the comment with the ID specified in the request from the selected wiki page
        /// </summary>
        /// <short>Delete comment</short>
        /// <section>Comment</section>
        /// <param name="id">Comment ID</param>
        /// <category>Wiki</category>
        [Delete("wiki/comment/{id}")]
        public void DeleteComment(string id)
        {
            _engine.RemoveComment(new Guid(id));
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
    }
}
