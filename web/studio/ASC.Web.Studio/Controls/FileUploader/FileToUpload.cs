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
        public bool NeedSaveToTemp { get; private set; }

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

            NeedSaveToTemp = Convert.ToBoolean(GetNeedSaveToTemp(context));

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

        private static string GetNeedSaveToTemp(HttpContext context)
        {
            return context.Request["needSaveToTemp"];
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