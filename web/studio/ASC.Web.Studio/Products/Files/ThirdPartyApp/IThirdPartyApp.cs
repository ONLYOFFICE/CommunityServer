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

using System.Text.RegularExpressions;
using System.Web;
using ASC.Files.Core;

namespace ASC.Web.Files.ThirdPartyApp
{
    public interface IThirdPartyApp
    {
        bool Request(HttpContext context);

        Token RefreshToken(string refreshToken);

        File GetFile(string fileId, out bool editable);

        string GetFileStreamUrl(File file);

        void SaveFile(string fileId, string downloadUrl);
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