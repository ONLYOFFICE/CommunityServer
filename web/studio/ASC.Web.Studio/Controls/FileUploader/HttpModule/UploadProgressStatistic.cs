/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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