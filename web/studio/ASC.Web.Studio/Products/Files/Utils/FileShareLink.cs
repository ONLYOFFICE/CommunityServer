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
using System.Web;
using ASC.Common.Utils;
using ASC.Files.Core;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Utility;
using File = ASC.Files.Core.File;
using FileShare = ASC.Files.Core.Security.FileShare;

namespace ASC.Web.Files.Utils
{
    public static class FileShareLink
    {
        public static string GetLink(File file, bool withHash = true)
        {
            var url = file.DownloadUrl;

            if (FileUtility.CanWebView(file.Title))
                url = FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID);

            if (withHash)
            {
                var linkParams = CreateKey(file.ID.ToString());
                url += "&" + FilesLinkUtility.DocShareKey + "=" + HttpUtility.UrlEncode(linkParams);
            }

            return CommonLinkUtility.GetFullAbsolutePath(url);
        }

        public static string CreateKey(string fileId)
        {
            return Signature.Create(fileId, Global.GetDocDbKey());
        }

        public static string Parse(string doc)
        {
            return Signature.Read<string>(doc ?? String.Empty, Global.GetDocDbKey());
        }

        public static bool Check(string doc, bool checkRead, IFileDao fileDao, out File file)
        {
            var fileShare = Check(doc, fileDao, out file);
            return (!checkRead
                    && (fileShare == FileShare.ReadWrite
                        || fileShare == FileShare.CustomFilter
                        || fileShare == FileShare.Review
                        || fileShare == FileShare.FillForms
                        || fileShare == FileShare.Comment))
                || (checkRead && fileShare != FileShare.Restrict);
        }

        public static FileShare Check(string doc, IFileDao fileDao, out File file)
        {
            file = null;
            if (string.IsNullOrEmpty(doc)) return FileShare.Restrict;
            var fileId = Parse(doc);
            file = fileDao.GetFile(fileId);
            if (file == null) return FileShare.Restrict;

            var filesSecurity = Global.GetFilesSecurity();
            if (filesSecurity.CanEdit(file, FileConstant.ShareLinkId)) return FileShare.ReadWrite;
            if (filesSecurity.CanCustomFilterEdit(file, FileConstant.ShareLinkId)) return FileShare.CustomFilter;
            if (filesSecurity.CanReview(file, FileConstant.ShareLinkId)) return FileShare.Review;
            if (filesSecurity.CanFillForms(file, FileConstant.ShareLinkId)) return FileShare.FillForms;
            if (filesSecurity.CanComment(file, FileConstant.ShareLinkId)) return FileShare.Comment;
            if (filesSecurity.CanRead(file, FileConstant.ShareLinkId)) return FileShare.Read;
            return FileShare.Restrict;
        }
    }
}