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


using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using File = ASC.Files.Core.File;

namespace ASC.Web.Files.ThirdPartyApp
{
    public interface IThirdPartyApp
    {
        bool Request(HttpContext context);

        string GetRefreshUrl();

        File GetFile(string fileId, out bool editable);

        string GetFileStreamUrl(File file);

        void SaveFile(string fileId, string fileType, string downloadUrl, Stream stream);
    }

    public class ThirdPartySelector
    {
        public const string AppAttr = "app";
        public static readonly Regex AppRegex = new Regex("^" + AppAttr + @"-(\S+)\|(\S+)$", RegexOptions.Singleline | RegexOptions.Compiled);

        public static string BuildAppFileId(string app, object fileId)
        {
            return AppAttr + "-" + app + "|" + fileId;
        }

        public static string GetFileId(string appFileId)
        {
            return AppRegex.Match(appFileId).Groups[2].Value;
        }

        public static IThirdPartyApp GetAppByFileId(string fileId)
        {
            if (string.IsNullOrEmpty(fileId)) return null;
            var match = AppRegex.Match(fileId);
            return match.Success
                       ? GetApp(match.Groups[1].Value)
                       : null;
        }

        public static IThirdPartyApp GetApp(string app)
        {
            switch (app)
            {
                case GoogleDriveApp.AppAttr:
                    return new GoogleDriveApp();
                case BoxApp.AppAttr:
                    return new BoxApp();
            }

            return new GoogleDriveApp();
        }
    }
}