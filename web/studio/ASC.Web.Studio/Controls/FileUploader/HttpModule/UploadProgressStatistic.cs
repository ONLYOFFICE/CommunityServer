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


using System.Collections.Generic;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    public class UploadProgressStatistic
    {
        private static readonly Dictionary<string, UploadProgressStatistic> Statistics = new Dictionary<string, UploadProgressStatistic>();
        public const string UploadIdField = "__UixdId";

        public string UploadId { get; set; }
        public long TotalBytes { get; set; }
        public long UploadedBytes { get; set; }
        public bool IsFinished { get; set; }
        public string CurrentFile { get; set; }
        public int CurrentFileIndex { get; set; }
        public float Progress { get; set; }
        public int ReturnCode { get; set; }

        internal UploadProgressStatistic()
        {
            CurrentFile = string.Empty;
            CurrentFileIndex = -1;
        }

        public string ToJson()
        {
            return string.Format("{{\"TotalBytes\":{0},\"Progress\":{1},\"CurrentFileIndex\":{2},\"CurrentFile\":\"{3}\",\"UploadId\":{4},\"IsFinished\":{5},\"Swf\":false,\"ReturnCode\":{6}}}",
                                 TotalBytes, Progress.ToString().Replace(',', '.'), CurrentFileIndex, CurrentFile, UploadId ?? "null", (IsFinished ? "true" : "false"), ReturnCode);
        }

        internal void AddUploadedBytes(int bytesCount)
        {
            UploadedBytes += bytesCount;
            Progress = (float)UploadedBytes/TotalBytes;
            Progress = Progress > 1 ? 1 : Progress;
        }

        public static UploadProgressStatistic GetStatistic(string id)
        {
            UploadProgressStatistic us;
            if (!Statistics.TryGetValue(id, out us))
                us = new UploadProgressStatistic();
            return us;
        }

        internal void EndUpload()
        {
            IsFinished = true;
        }

        internal void BeginFileUpload(string fileName)
        {
            CurrentFile = fileName;
            CurrentFileIndex++;
        }

        internal void AddFormField(string name, string value)
        {
            if (name == UploadIdField)
            {
                UploadId = value;

                if (!Statistics.ContainsKey(value))
                    Statistics.Add(value, this);
            }
        }
    }
}