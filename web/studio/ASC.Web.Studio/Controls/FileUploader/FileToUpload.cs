/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.IO;
using System.Web;

namespace ASC.Web.Studio.Controls.FileUploader
{
    public class FileToUpload
    {
        public string FileName { get; private set; }
        public Stream InputStream { get; private set; }
        public string FileContentType { get; private set; }
        public long ContentLength { get; private set; }

        public FileToUpload(HttpContext context)
        {
            if (IsHtml5Upload(context))
            {
                FileName = GetFileName(context);
                InputStream = context.Request.InputStream;
                FileContentType = GetFileContentType(context);
                ContentLength = (int)context.Request.InputStream.Length;
            }
            else
            {
                var file = context.Request.Files[0];
                FileName = file.FileName;
                InputStream = file.InputStream;
                FileContentType = file.ContentType;
                ContentLength = file.ContentLength;
            }
            if (string.IsNullOrEmpty(FileContentType))
            {
                FileContentType = MimeMapping.GetMimeMapping(FileName) ?? string.Empty;
            }
            FileName = FileName.Replace("'", "_").Replace("\"", "_");
        }

        public static bool HasFilesToUpload(HttpContext context)
        {
            return 0 < context.Request.Files.Count || (IsHtml5Upload(context) && context.Request.InputStream != null);
        }

        private static string GetFileName(HttpContext context)
        {
            return context.Request["fileName"];
        }

        private static string GetFileContentType(HttpContext context)
        {
            return context.Request["fileContentType"];
        }

        private static bool IsHtml5Upload(HttpContext context)
        {
            return "html5".Equals(context.Request["type"]);
        }
    }
}