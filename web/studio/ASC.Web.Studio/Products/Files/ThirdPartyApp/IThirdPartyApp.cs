/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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