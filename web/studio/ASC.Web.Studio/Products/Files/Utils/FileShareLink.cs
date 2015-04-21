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
        public static string GetLink(File file)
        {
            var url = file.ViewUrl;

            if (!FileUtility.CanImageView(file.Title) && TenantExtra.GetTenantQuota().DocsEdition)
                url = FilesLinkUtility.GetFileWebPreviewUrl(file.Title, file.ID);

            var linkParams = CreateKey(file.ID.ToString());
            url += "&" + FilesLinkUtility.DocShareKey + "=" + HttpUtility.UrlEncode(linkParams);

            return CommonLinkUtility.GetFullAbsolutePath(url);
        }

        public static string CreateKey(string fileId)
        {
            return Signature.Create(fileId, Global.GetDocDbKey());
        }

        public static string Parse(string key)
        {
            return Signature.Read<string>(key ?? String.Empty, Global.GetDocDbKey());
        }

        public static bool Check(string key, bool checkRead, IFileDao fileDao, out File file)
        {
            var fileShare = Check(key, fileDao, out file);
            return (!checkRead && fileShare == FileShare.ReadWrite) || (checkRead && fileShare <= FileShare.Read);
        }

        public static FileShare Check(string key, IFileDao fileDao, out File file)
        {
            file = null;
            if (string.IsNullOrEmpty(key)) return FileShare.Restrict;
            var fileId = Parse(key);
            file = fileDao.GetFile(fileId);
            if (file == null) return FileShare.Restrict;

            var filesSecurity = Global.GetFilesSecurity();
            if (filesSecurity.CanEdit(file, FileConstant.ShareLinkId)) return FileShare.ReadWrite;
            if (filesSecurity.CanRead(file, FileConstant.ShareLinkId)) return FileShare.Read;
            return FileShare.Restrict;
        }
    }
}