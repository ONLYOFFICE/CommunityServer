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